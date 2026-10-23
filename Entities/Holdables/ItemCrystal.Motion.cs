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
        public override void Update()
        {
            base.Update();
            if (destroyed)
            {
                return;
            }
            else
            {
                CheckAgainstColliders();
                if (Hold.Entity != null &&
                    ((ISourceScene)Hold.Scene).SourceScene.Components
                    .GetTrackedComponents("Celeste.HoldableCollider") != null && Hold.Scene != null)
                {
                    Hold.CheckAgainstColliders();
                }
            }

            frameCount += 1f;
            if (!Hold.IsHeld)
            {
                foreach (OriginalSpringState spring in springs)
                {
                    if (SourceEntityCollision.Check(this, spring))
                    {
                        OnCollideSpring(spring);
                    }
                }

                if (tutorialGui != null)
                {
                    if (shouldShowTutorial)
                    {
                        tutorialGui.Open =
                            (((ISourceVectorEnvironment)engine).VectorArithmetic.Length(
                                playerEntity.Position - Position) < 64f);
                    }
                    else
                    {
                        tutorialGui.Open = false;
                    }
                }
            }

            hardVerticalHitSoundCooldown -= engine.DeltaTime;
            base.Depth = 100;
            if (Hold.IsHeld)
            {
                prevLiftSpeed = Vector2.Zero;
                return;
            }

            CarryableActorMotion.AdvanceSpeed(this, engine, Hold, ref Speed, ref prevLiftSpeed, ref noGravityTimer);
            previousPosition = base.ExactPosition;
            MoveH(Speed.X * engine.DeltaTime, onCollideH);
            MoveV(Speed.Y * engine.DeltaTime, onCollideV);
            if (base.Center.X > (float)Level.Bounds.Right)
            {
                MoveH(32f * engine.DeltaTime);
                if (base.Right > (float)Level.Bounds.Right)
                {
                    base.Right = Level.Bounds.Right;
                    Speed.X *= -0.4f;
                }
            }
            else if (base.Left < (float)Level.Bounds.Left)
            {
                base.Left = Level.Bounds.Left;
                Speed.X *= -0.4f;
            }
            else if (base.Top < (float)(Level.Bounds.Top - 4))
            {
                base.Top = Level.Bounds.Top + 4;
                Speed.Y = 0f;
            }
            else if (base.Bottom > (float)Level.Bounds.Bottom + 4f)
            {
                Add(new SimulationCoroutine(engine, DestroyWithoutItems()));
                base.Bottom = Level.Bounds.Bottom - 4;
            }

            if (base.X < (float)(Level.Bounds.Left + 10))
            {
                MoveH(32f * engine.DeltaTime);
            }

            RoomPlayerSession entity = ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>();
            OriginalTempleGateState templeGate = CollideFirst<OriginalTempleGateState>("Celeste.TempleGate");
            if (templeGate != null && entity != null)
            {
                templeGate.Collidable = false;
                MoveH((float)(Math.Sign(entity.X - base.X) * 32) * engine.DeltaTime);
                templeGate.Collidable = true;
            }
        }
    }
}
