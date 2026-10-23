using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Components.Animation;
using Celeste.Core.Display;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Content.Maps.Metadata;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.CherryHelper.Entities.Markers;
using Celeste.Mods.CherryHelper.Runtime.Fields;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Platforms.Foundation;

namespace Celeste.Mods.CherryHelper.Entities.Fields
{
    [CustomEntity("CherryHelper/NightItemLockfield")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.EntityToggleField")]
    [SceneTracked(true)]
    public class EntityToggleField : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly EntityToggleFieldRuntime runtime;
        private Type[] resolvedTypes = Array.Empty<Type>();
        private string[] resolvedNames = Array.Empty<string>();
        public List<string> itemsToBan;
        public List<SceneEntityState> instancesStolen = new();
        public bool stolenempty = true;
        public string nightId = "";
        public bool on;
        public bool lucid;
        public bool firstTime;
        public Dictionary<SceneEntityState, AssistRectangle> assign = new();
        public RoomTransitionListener transbunny;
        private bool can;
        public bool assistOnSolid = true;

        public EntityToggleField(ISourceEngineContext engine, Vector2 position, int width, int height)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            this.engine = engine;
            runtime = EntityToggleFieldRuntime.For(engine);
            Collider = new HitboxColliderGeometry(width, height);
        }

        public EntityToggleField(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Width, data.Height)
        {
            itemsToBan = new List<string>(data.Attr("ItemsToSteal", "").Split(','));
            nightId = data.Attr("nightId", "");
            lucid = data.Bool("lucid", false);
            assistOnSolid = data.Bool("AssistRectangleOnSolid", true);
            if (data.Bool("startAppeared", false))
            {
                on = true;
                firstTime = false;
                return;
            }

            firstTime = true;
            Add(transbunny = new RoomTransitionListener { OnInBegin = Steal });
        }

        internal void ResolveTypes()
        {
            var names = itemsToBan;
            resolvedNames = new string[names.Count];
            resolvedTypes = new Type[names.Count];
            var assembly = ((IEverestFakeEntryAssemblyEnvironment)engine).FakeEntryAssembly;
            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                resolvedNames[i] = name;
                if (name.Length > 0)
                    resolvedTypes[i] = assembly.GetType("Celeste." + name, false, true)
                                       ?? assembly.GetType("Celeste.Mod." + name, false, true) ??
                                       assembly.GetType(name, false, true) ?? GetType();
            }
        }

        private void EnsureNames()
        {
            if (resolvedNames.Length != itemsToBan.Count)
            {
                ResolveTypes();
                return;
            }

            for (int i = 0; i < resolvedNames.Length; i++)
                if (resolvedNames[i] != itemsToBan[i])
                {
                    ResolveTypes();
                    return;
                }
        }

        public static List<EntityToggleField> FindById(OriginalLevelSceneRoot level, string nightId)
        {
            var found = new List<EntityToggleField>();
            foreach (var value in level.Scheduler.Entities.GetTrackedEntities(typeof(EntityToggleField)))
                if (value is EntityToggleField field && field.nightId == nightId)
                    found.Add(field);
            return found;
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            runtime.Add(this);
            ResolveTypes();
            Add(new SimulationCoroutine(engine, delayForAdded()));
        }

        public override void Removed(object scene)
        {
            runtime.Remove(this);
            base.Removed(scene);
        }

        public override void SceneEnd(object scene)
        {
            runtime.Remove(this);
            base.SceneEnd(scene);
        }

        private IEnumerator delayForAdded()
        {
            can = false;
            yield return 0.2;
            can = true;
        }

        private IEnumerator FlickerBlackhole()
        {
            var tween = SourceTweenState.Create(engine.TweenPool, SourceTweenState.TweenMode.Oneshot, null, .3f, true);
            tween.OnUpdate = t => engine.Effects.Glitch = .5f * (1f - t.Eased);
            Add(tween);
            yield break;
        }

        public void HandleVfx()
        {
            if (!firstTime && can && ((IOriginalLevelScreenEffectsProcess)engine).AllowGlitch)
                Add(new SimulationCoroutine(engine, FlickerBlackhole()));
        }

        private void Steal()
        {
            EnsureNames();
            var level = (OriginalLevelSceneRoot)Scene;
            var entities = level.Scheduler.Entities.Entities;
            for (int n = 0; n < entities.Count; n++)
            {
                if (entities[n] is not SceneEntityState entity || entity.Collider == null) continue;
                var outer = Collider.Bounds;
                var bounds = entity.Collider.Bounds;
                if (outer.Left > bounds.Left || outer.Top > bounds.Top || outer.Right < bounds.Right ||
                    outer.Bottom < bounds.Bottom) continue;
                for (int i = 0; i < resolvedTypes.Length; i++)
                {
                    if (resolvedNames[i].Length == 0 || entity.GetType() != resolvedTypes[i]) continue;
                    instancesStolen.Add(entity);
                    stolenempty = false;
                    HandleVfx();
                    if (entity is SourceSolidEntityState && assistOnSolid)
                    {
                        assign.Add(entity,
                            new AssistRectangle(entity.Position, (int)entity.Width, (int)entity.Height,
                                PackedColor.White * .3f, engine));
                        assign.TryGetValue(entity, out var marker);
                        marker.SecretAlpha = .1f;
                        level.Scheduler.AddEntity(marker);
                    }

                    entity.RemoveSelf();
                    firstTime = false;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            var level = (OriginalLevelSceneRoot)Scene;
            if (on && !stolenempty)
            {
                HandleVfx();
                foreach (var entity in instancesStolen)
                {
                    level.Scheduler.AddEntity(entity);
                    if (entity is SourceSolidEntityState && assistOnSolid)
                    {
                        assign.TryGetValue(entity, out var marker);
                        marker.RemoveSelf();
                        assign.Remove(entity);
                        var player = level.Scheduler.Entities.GetTrackedEntity<RoomPlayerSession>();
                        if (entity.Collider.Bounds.Contains((int)player.X, (int)player.Y) && player != null)
                            player.Die(Vector2.Zero, false, false);
                    }
                }

                instancesStolen.Clear();
                stolenempty = true;
                firstTime = false;
            }
            else if (!on && stolenempty) Steal();
        }
    }
}
