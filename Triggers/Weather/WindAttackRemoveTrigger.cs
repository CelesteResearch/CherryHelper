using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Entities.Hazards.Snowballs;
using Celeste.Game.Execution.Levels;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.CherryHelper.Triggers.Weather
{
    [CustomEntity("CherryHelper/SnowballStop")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.WindAttackRemoveTrigger")]
    public class WindAttackRemoveTrigger : SourceTriggerEntityState
    {
        public OriginalLevelSceneRoot Level;

        public WindAttackRemoveTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration,
                data.Position + offset, data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            Level = Scene as OriginalLevelSceneRoot;
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            base.OnEnter(player);
            if (((ISourceScene)Scene).SourceScene.Entities.FindFirst<OriginalSnowballState>() != null)
            {
                var snowball = ((ISourceScene)Scene).SourceScene.Entities.FindFirst<OriginalSnowballState>();
                Level.SourceScene.RemoveEntity(snowball);
                RemoveSelf();
            }
        }
    }
}
