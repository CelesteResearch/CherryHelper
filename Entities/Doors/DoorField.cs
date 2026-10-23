// SPDX-License-Identifier: MIT

using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Geometry;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Chapters.Flow;
using Celeste.Game.Components.Talking;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Execution.Session;
using Celeste.Game.Gameplay.Collectibles.Strawberries;
using Celeste.Game.Gameplay.Platforms.Cassette;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Intro;
using Celeste.Simulation.Presentation.Scene.Transitions;

namespace Celeste.Mods.CherryHelper.Entities.Doors
{
    [SceneTracked]
    [CustomEntity("CherryHelper/DoorField")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.DoorField")]
    public class DoorField : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        public string toLevel;
        private bool EndLevel;
        private Vector2 spawnOffset;
        public string NextChapterSSID = "";
        private bool SeamlessNextChapter;
        private int ChapterAreaMode;
        private OriginalTalkComponent talkie;

        public static DoorField Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room, Vector2 offset,
            OriginalLevelEntityData data) => new(level.EngineContext, data, offset);

        public DoorField(ISourceEngineContext engine, Vector2 position, int width, int height)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            this.engine = engine;
            Collider = new HitboxColliderGeometry(width, height);
        }

        public DoorField(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Width, data.Height)
        {
            toLevel = EntityAttributeReader.Attr(data.Values, "toLevel", "");
            EndLevel = EntityAttributeReader.Bool(data.Values, "EndLevel");
            NextChapterSSID = EntityAttributeReader.Attr(data.Values, "SIDOfMap", "");
            SeamlessNextChapter = EntityAttributeReader.Bool(data.Values, "LoadAnotherBin", true);
            spawnOffset = new Vector2(EntityAttributeReader.Int(data.Values, "spawnOffsetX"),
                EntityAttributeReader.Int(data.Values, "spawnOffsetY"));
            switch (EntityAttributeReader.Attr(data.Values, "Side", ""))
            {
                case "A": ChapterAreaMode = 0; break;
                case "B": ChapterAreaMode = 1; break;
                case "C": ChapterAreaMode = 2; break;
            }
        }

        public override void Added(object scene)
        {
            Add(talkie = new OriginalTalkComponent((IOriginalTalkProcess)engine,
                new IntRect(0, 0, (int)Collider.Width, (int)Collider.Height),
                new Vector2(.5f * Collider.Width, .5f * Collider.Height), OnEnter));
            talkie.Enabled = true;
            talkie.Visible = true;
            talkie.PlayerMustBeFacing = false;
            base.Added(scene);
        }

        private void OnEnter(RoomPlayerSession obj) => Add(new SimulationCoroutine(engine, EnterRoutine(obj)));

        private IEnumerator EnterRoutine(RoomPlayerSession obj)
        {
            obj.Machine.State = 11;
            obj.Dummy.AutoAnimate = false;
            obj.Sprite.Play("lookUp");
            yield return .8f;
            var level = (OriginalLevelSceneRoot)Scene;
            var entity = level.SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>();
            if (entity != null)
                ((ISourceScreenWipeEnvironment)engine).ProcessWipe.SpotlightFocusPoint =
                    entity.Position - level.Camera.Position - new Vector2(0f, 8f);
            yield return new SpotlightWipePresentation(level, false);
            Add(new SimulationCoroutine(engine, Teleport(obj)));
        }

        private IEnumerator Teleport(RoomPlayerSession player)
        {
            yield return 1.7f;
            var level = (OriginalLevelSceneRoot)Scene;
            if (SeamlessNextChapter && NextChapterSSID != "")
            {
                if (EndLevel) level.RegisterAreaComplete();
                var areas = ((IOriginalAreaKeyEnvironment)engine).AreaKeys;
                var session = ((IOriginalSessionEnvironment)engine).Sessions.CreateSession(
                    areas.ToKey(areas.GetAreaBySID(NextChapterSSID), ChapterAreaMode));
                OriginalLevelEntryRouting.Go(session, false, engine.Frame.Scenes,
                    ((IOriginalLevelEntryEnvironment)engine).LevelEntries);
            }
            else if (EndLevel) level.CompleteArea(false, true, true);
            else
                level.SourceScene.OnEndOfFrame += delegate
                {
                    OriginalStrawberryTransfer.StoreStrawberries(player.Leader);
                    level.RemoveEntity(player);
                    level.UnloadLevel();
                    level.Session.Keys.Clear();
                    level.Session.Level = toLevel;
                    level.Session.RespawnPoint =
                        level.GetSpawnPoint(spawnOffset + new Vector2(level.Bounds.Left, level.Bounds.Top));
                    level.LoadLevel(PlayerIntroType.None);
                    level.SourceScene.Entities.GetTrackedEntity<OriginalCassetteBlockManagerState>()
                        ?.Get<RoomTransitionListener>()
                        ?.OnOutBegin?.Invoke();
                    ((IOriginalGameplayProcess)engine).AudioSession.SetMusicParam("fade", 1f);
                    OriginalStrawberryTransfer.RestoreStrawberries(level.SourceScene.Entities
                        .GetTrackedEntity<RoomPlayerSession>().Leader);
                    level.Camera.Y -= 8f;
                };
        }
    }
}
