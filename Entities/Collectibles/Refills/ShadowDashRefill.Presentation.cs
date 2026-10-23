using System;
using System.Numerics;
using Celeste.Game.Gameplay.Collectibles.Refills;
using Celeste.Simulation.Presentation.Particles;
using Celeste.Simulation.Presentation.Particles.Definitions;

namespace Celeste.Mods.CherryHelper.Entities.Collectibles.Refills
{
    public partial class ShadowDashRefill
    {
        private readonly struct PresentationPhases(ShadowDashRefill owner) : IRefillPresentationPhases
        {
            public void UpdateTimerAndGlow<TContext>(in TContext context)
                where TContext : struct, IRefillPresentationContext
            {
                var emission = new ShadowParticles(owner);
                RefillPresentation.UpdateTimerAndGlow(owner, in context, owner.level, ref owner.respawnTimer,
                    owner.respawnAction, in emission, emission);
            }

            public void UpdateY() => RefillPresentation.UpdateY(owner.sprite, owner.flash, owner.bloom, owner.sine);

            public void UpdateLightAndFlash<TContext>(in TContext context)
                where TContext : struct, IRefillPresentationContext =>
                RefillPresentation.UpdateLighting(owner, in context, owner.sprite, owner.flash, owner.light,
                    owner.bloom);
        }

        private readonly struct ShadowParticles(ShadowDashRefill owner)
            : IRefillGlowParticleSource, IRefillParticleEmission
        {
            public ParticleTypePresentation GetGlow() => owner.badelineParticles.P_Ambience;

            public void Emit(ParticleSystemPresentation particles, ParticleTypePresentation type, int amount,
                Vector2 position, Vector2 range) => particles.Emit(type, amount, position, range);

            public void Emit(ParticleSystemPresentation particles, ParticleTypePresentation type, int amount,
                Vector2 position, Vector2 range, float direction) =>
                particles.Emit(type, amount, position, range, direction);
        }

        private void Respawn() => RefillPresentation.Respawn(this, engine, level, sprite, outline, wiggler,
            badelineParticles.P_Move, "event:/game/general/diamond_return", -100, new ShadowParticles(this));
    }
}
