using System.Collections;
using Celeste.Core.Scheduling;
using Celeste.Game.Gameplay.Collectibles.Refills;
using Celeste.Game.Controls.Source;
using Celeste.Simulation;

namespace Celeste.Mods.CherryHelper.Entities.Collectibles.Refills
{
    public partial class ShadowDashRefill
    {
        private void OnPlayer(RoomPlayerSession player)
        {
            if (!module.Session.HasShadowDash)
            {
                engine.SpatialAudio.Play("event:/game/general/diamond_touch", Position);
                engine.RumbleSession.Rumble((int)RumbleStrength.Medium, (int)RumbleLength.Medium);
                Collidable = false;
                Add(new SimulationCoroutine(engine, RefillRoutine(player)));
                player.UseRefill(false);
                module.Session.HasShadowDash = true;
                respawnTimer = 2.5f;
            }
        }

        private IEnumerator RefillRoutine(RoomPlayerSession player)
        {
            RefillPresentation.BeginRoutine(engine);
            yield return null;
            level.Shake();
            RefillPresentation.HideForRoutine(this, sprite, flash, outline, oneUse);
            yield return .05f;
            RefillPresentation.FinishRoutine(this, engine, level, engine.RefillParticles.P_ShatterTwo, player);
            if (oneUse) RemoveSelf();
        }
    }
}
