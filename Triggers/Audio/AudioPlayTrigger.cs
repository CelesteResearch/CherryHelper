using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Processes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.CherryHelper.Triggers.Audio
{
    [CustomEntity("CherryHelper/AudioPlayTrigger")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.AudioPlayTrigger")]
    public class AudioPlayTrigger : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        private string eventToPlay;
        private bool onlyOnce;

        public AudioPlayTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration,
                data.Position + offset, data.Width, data.Height,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            eventToPlay = data.Attr("AudioEventToPlay");
            onlyOnce = data.Bool("OnlyOnce");
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            base.OnEnter(player);
            ((IOriginalGameplayProcess)engine).AudioSession.Play(eventToPlay);
            if (onlyOnce)
                RemoveSelf();
        }
    }
}
