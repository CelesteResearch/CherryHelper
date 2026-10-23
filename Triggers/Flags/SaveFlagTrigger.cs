// SPDX-License-Identifier: MIT

using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest;
using Celeste.Everest.Diagnostics.Logging;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.CherryHelper.Modules;
using Celeste.Mods.CherryHelper.Runtime.Flags;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.CherryHelper.Triggers.Flags
{
    [CustomEntity("CherryHelper/SaveFlagTrigger")]
    [SceneTracked(false)]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.SaveFlagTrigger")]
    public class SaveFlagTrigger : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly SavedFlagModuleBinding binding;
        public string Flag;
        public bool State;
        public bool CurrentState;

        public SaveFlagTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset) : base(
            ((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
            data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            binding = SavedFlagModuleBinding.For(engine);
            Flag = data.Attr("Flag", "");
            State = data.Bool("State", false);
            CurrentState = data.Bool("CurrentState", true);
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            base.OnEnter(player);
            var level = (OriginalLevelSceneRoot)Scene;
            if (CurrentState)
            {
                binding.Module.SaveFlag(level.Session, Flag, level.Session.GetFlag(Flag));
                level.Session.SetFlag(Flag, level.Session.GetFlag(Flag));
                ((IEverestLoggerProcessEnvironment)engine).Logger.Log(EverestLogLevel.Info,
                    "CherryHelper/SaveFlagTrigger", $"Setting state to {level.Session.GetFlag(Flag)}");
            }
            else
            {
                binding.Module.SaveFlag(level.Session, Flag, State);
                level.Session.SetFlag(Flag, State);
            }
        }
    }
}
