using System;
using System.Collections.Generic;
using System.Collections;
using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Scheduling;
using Celeste.Mods.CherryHelper.Modules;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Player.Dash;
using Celeste.Simulation.Gameplay.Player.Presentation.Hair;
using Celeste.Simulation.Presentation.Player.Death;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.CherryHelper.Runtime.ShadowDash
{
    public sealed class ShadowDashProcess : IDisposable
    {
        private readonly ISourceEngineContext engine;
        private readonly CherryHelperModule module;
        private readonly List<IDisposable> hooks = new();
        private SimulationCoroutine shadowEndDelayCoroutine;

        public ShadowDashProcess(ISourceEngineContext engine, CherryHelperModule module)
        {
            this.engine = engine;
            this.module = module;
            try
            {
                hooks.Add(new Hook(typeof(RoomPlayerSession).GetMethod(nameof(RoomPlayerSession.Die)),
                    (Func<Func<RoomPlayerSession, Vector2, bool, bool, PlayerDeathPresentation>, RoomPlayerSession,
                        Vector2, bool, bool, PlayerDeathPresentation>)Die));
                hooks.Add(new Hook(typeof(DashSequence).GetMethod(nameof(DashSequence.End)),
                    (Action<Action<DashSequence>, DashSequence>)DashEnd));
                hooks.Add(new Hook(typeof(DashSequence).GetMethod(nameof(DashSequence.Begin)),
                    (Action<Action<DashSequence>, DashSequence>)DashBegin));
                hooks.Add(new Hook(typeof(PlayerHair).GetMethod(nameof(PlayerHair.GetHairColor)),
                    (Func<Func<PlayerHair, int, PackedColor>, PlayerHair, int, PackedColor>)HairColor));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private PlayerDeathPresentation Die(
            Func<RoomPlayerSession, Vector2, bool, bool, PlayerDeathPresentation> original,
            RoomPlayerSession player, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (!ReferenceEquals(player.EngineContext, engine))
                return original(player, direction, evenIfInvincible, registerDeathInStats);
            if (!module.Session.ShadowDashActive || evenIfInvincible)
            {
                module.Session.HasShadowDash = false;
                module.Session.ShadowDashActive = false;
                return original(player, direction, evenIfInvincible, registerDeathInStats);
            }

            return null;
        }

        private void DashBegin(Action<DashSequence> original, DashSequence dash)
        {
            if (!ReferenceEquals(dash.EngineContext, engine))
            {
                original(dash);
                return;
            }

            if (module.Session.HasShadowDash)
            {
                module.Session.ShadowDashActive = true;
                shadowEndDelayCoroutine?.Cancel();
                shadowEndDelayCoroutine?.RemoveSelf();
            }

            module.Session.HasShadowDash = false;
            original(dash);
        }

        private void DashEnd(Action<DashSequence> original, DashSequence dash)
        {
            original(dash);
            if (!ReferenceEquals(dash.EngineContext, engine)) return;
            var player = ((RoomPlayerWorld)dash.World).SourcePlayer;
            if (player.Machine.State != 2 && module.Session.ShadowDashActive)
            {
                shadowEndDelayCoroutine = new SimulationCoroutine(engine, ShadowEndDelay());
                player.Add(shadowEndDelayCoroutine);
            }
        }

        private IEnumerator ShadowEndDelay()
        {
            yield return .03f;
            module.Session.ShadowDashActive = false;
        }

        private PackedColor HairColor(Func<PlayerHair, int, PackedColor> original, PlayerHair hair, int index) =>
            ReferenceEquals(hair.EngineContext, engine) && module.Session.HasShadowDash
                ? PackedColor.FromNonPremultiplied(hair.Color.R / 4, hair.Color.G / 4, hair.Color.B / 4, 255)
                : original(hair, index);

        public void Dispose()
        {
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear();
            shadowEndDelayCoroutine?.Cancel();
            shadowEndDelayCoroutine?.RemoveSelf();
            shadowEndDelayCoroutine = null;
        }
    }
}
