using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Scheduling;
using Celeste.Core.Reflection.Runtime;
using Celeste.Everest;
using Celeste.Everest.Diagnostics.Logging;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.CherryHelper.Triggers.Player
{
    [CustomEntity("CherryHelper/PlayerStateChange")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.ChangePlayerStateTrigger")]
    public class ChangePlayerStateTrigger : SourceTriggerEntityState
    {
        private static readonly string sourcePlayerRegistryKey =
            typeof(global::Celeste.Simulation.RoomPlayerSession).FullName;

        private readonly ISourceEngineContext engine;
        public int state;
        private readonly bool onlyOnce;

        public ChangePlayerStateTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration,
                data.Position + offset, data.Width, data.Height,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            state = data.Int("State");
            onlyOnce = data.Bool("onlyOnce");
        }

        public IEnumerator emptyCoroutine()
        {
            yield break;
        }

        public override void Update()
        {
            base.Update();
            if (CollideFirstPlayer()?.Machine.State == 18)
                OnEnter(CollideFirstPlayer());
        }

        private RoomPlayerSession CollideFirstPlayer() =>
            SourceTrackedEntityCollision.First<RoomPlayerSession>(
                ((ISourceScene)Scene).SourceScene.Entities, sourcePlayerRegistryKey, this);

        public override void OnEnter(RoomPlayerSession player)
        {
            base.OnEnter(player);
            if (state >= 25)
                state = 0;
            if (state == 24)
                ((IEverestLoggerProcessEnvironment)engine).Logger.Log(EverestLogLevel.Info,
                    "CherryHelper/ChangePlayerStateTrigger",
                    "Fling Bird State crashes the game, please refrain from using state 24.");
            else
                player.Machine.ForceState(state);
            if (onlyOnce)
                RemoveSelf();
        }
    }
}
