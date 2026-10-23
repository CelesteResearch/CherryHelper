using System;
using Celeste.Scene.Tracking;
using Celeste.Game.Gameplay.Holdables.TheoCrystal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Components.Animation;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display.Colors;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Components.Oscillation;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Actors.Foundation;
using Celeste.Simulation.Gameplay.Holdables;
using Celeste.Simulation.Gameplay.Holdables.TheoCrystal;
using Celeste.Simulation.Gameplay.Interaction.Springs;
using Celeste.Simulation.Gameplay.Effects.Debris;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Platforms.StaticMovers;
using Celeste.Simulation.Presentation.Effects.Mirrors;
using Celeste.Simulation.Presentation.Hud.Tutorials.Birds;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Execution.Startup;
using Celeste.Game.Components.Switches;
using Celeste.Game.Gameplay.Mechanisms.DashSwitches;
using Celeste.Game.Gameplay.Doors.TempleGates;
using Celeste.Game.Entities.Platforms.SwapBlocks;
using Celeste.Game.Gameplay.Platforms.Cassette;
using Celeste.Game.Gameplay.Interaction.Springs;
using Celeste.Game.Gameplay.Checkpoints.Summit;
using Celeste.Game.Gameplay.Collectibles.Refills;
using Celeste.Game.Gameplay.Mechanisms.Switches;
using Celeste.Game.Gameplay.Holdables.Gliders;
using Celeste.Game.Gameplay.Hazards.FireBalls;
using Celeste.Game.Entities.IntroCar;
using Celeste.Game.Gameplay.Enemies.Seekers;
using Celeste.Game.Gameplay.Enemies.Badeline.Chasers;
using Celeste.Game.Entities.Collectibles.Hearts;
using Celeste.Game.Gameplay.Platforms.Clouds;
using Celeste.Game.Entities.Interaction.Boosters;
using Celeste.Game.Gameplay.Mechanisms.Core;
using Celeste.Game.Entities.StarJump;
using Celeste.Mods.CherryHelper.Components.Holdables;

namespace Celeste.Mods.CherryHelper.Entities.Holdables
{
    public partial class ItemCrystal : SourceActorEntityState
    {
        private void OnCollideH(PixelCollision data)
        {
            if (data.Hit is OriginalDashSwitchState)
            {
                (data.Hit as OriginalDashSwitchState).OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
            }

            PlaySound("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }

            Speed.X *= -0.4f;
        }

        private void OnCollideV(PixelCollision data)
        {
            if (data.Hit is OriginalDashSwitchState)
            {
                (data.Hit as OriginalDashSwitchState).OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
            }

            if (Speed.Y > 0f && frameCount > 15f)
            {
                if (hardVerticalHitSoundCooldown <= 0f)
                {
                    PlaySound("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity",
                        ScalarMath.ClampedMap(Speed.Y, 0f, 200f));
                    hardVerticalHitSoundCooldown = 0.5f;
                }
                else
                {
                    PlaySound("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
                }
            }

            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }

            if (Speed.Y > 140f && !(data.Hit is OriginalSwapBlockState) && !(data.Hit is OriginalDashSwitchState))
            {
                Speed.Y *= -0.6f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position;
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(base.Right, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(base.Left, base.Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(base.X, base.Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(base.X, base.Top);
                positionRange = Vector2.UnitX * 6f;
            }

            Level.Particles.Emit(((IOriginalGameplayProcess)engine).ParticleTypes.TheoCrystal.P_Impact, 12, position,
                positionRange, direction);
        }
    }
}
