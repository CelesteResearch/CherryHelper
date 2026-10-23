using System.Numerics;
using Celeste.Core.Scheduling;
using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Mods.CherryHelper.Gameplay.Platforms.Crushing;
using Celeste.Simulation.Gameplay.Platforms.Crushing;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.CherryHelper.Entities.Platforms.Crushing
{
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.UninterruptedNRCB")]
    [CustomEntity("CherryHelper/UI_NRCB")]
    public class UninterruptedCrushBlock : NonReturningCrushBlockBase
    {
        public static ParticleTypePresentation P_Impact;

        public static UninterruptedCrushBlock Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset, OriginalLevelEntityData data) => new(level.EngineContext, data, offset,
            level.SolidWorldFactory.Create(data.Position + offset, false));

        public UninterruptedCrushBlock(ISourceEngineContext engine, EntityData data, Vector2 offset,
            ISourceSolidWorld world)
            : base(engine, data, offset, world, "objects/uninterruptedNRK", true)
        {
        }

        protected override bool MoveHorizontalCheck(float amount) => MoveHCheck(amount);
        protected override bool MoveVerticalCheck(float amount) => MoveVCheck(amount);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private bool MoveHCheck(float amount) =>
            CrushingSolidMotion.CheckHorizontal(this, level, amount, CrushingCollisionCallback);

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private bool MoveVCheck(float amount) =>
            CrushingSolidMotion.CheckVertical(this, level, amount, true, CrushingCollisionCallback);
    }
}
