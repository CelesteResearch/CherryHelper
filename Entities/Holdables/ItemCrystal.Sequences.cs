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
        public IEnumerator DestroySequence()
        {
            if (destroyed || destroying)
            {
                yield break;
            }

            destroying = true;
            PlayerRelease();
            base.Collider = new CircleColliderGeometry(((ISourceVectorEnvironment)engine).VectorArithmetic, 5f);
            for (int i = 0; i < cassetteBlocks.Count; i++)
            {
                if (SourceEntityCollision.Check(this, cassetteBlocks[i]))
                {
                    break;
                }
            }

            base.Collider = pushRadius;
            level.Displacement.AddBurst(Position, 0.35f, 5f, 30f, 0.15f);
            RoomPlayerSession player = Hold.Holder;
            if (player != null && !SourceSceneCollision.CheckLine(((ISourceScene)Scene).SourceScene.Entities,
                    "Celeste.Solid", Position + Vector2.UnitY * -10f, player.Center))
            {
                player.ExplodeLaunch(Position, snapUp: true);
            }

            base.Collider = hitBox;
            PlayerRelease();
            switch (item)
            {
                case ItemCrystalContents.Refill:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalRefillState(Position, false, false, (IOriginalRefillProcess)engine));
                    break;
                case ItemCrystalContents.Two_Refill:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalRefillState(Position, true, false, (IOriginalRefillProcess)engine));
                    break;
                case ItemCrystalContents.Touch_Switch:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalTouchSwitchState(Position, (IOriginalTouchSwitchProcess)engine));
                    if (switcher != null)
                    {
                        Remove(switcher);
                    }

                    break;
                case ItemCrystalContents.Jellyfish:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalGliderState(Position, true, false, engine));
                    break;
                case ItemCrystalContents.Fireball:
                    Vector2[] nodes = new Vector2[1];
                    nodes[0] = Position;
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalFireBallState(nodes, 1, 0, 0, 1f, false, (IOriginalFireBallProcess)engine));
                    break;
                case ItemCrystalContents.Shadow_Dash_Refill:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new Celeste.Mods.CherryHelper.Entities.Collectibles.Refills.ShadowDashRefill(engine, Position,
                            false));
                    break;
                case ItemCrystalContents.Madeline:
                    Level.SourceScene.QueueSourceEntity(entity = Level.CreateOriginalPlayer(Position,
                        Celeste.Simulation.Gameplay.Player.Presentation.Sprites.PlayerSpriteMode.MadelineNoBackpack));
                    break;
                case ItemCrystalContents.Summit_Confetti:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalSummitCheckpointState.ConfettiRenderer(engine, Position));
                    PlaySound("event:/game/07_summit/checkpoint_confetti");
                    break;
                case ItemCrystalContents.Unlockable_Character:
                    Level.SourceScene.QueueSourceEntity(entity = new OriginalIntroCarState(engine,
                        Level.SolidWorldFactory, Position + new Vector2(0, -24f)));
                    Level.SourceScene.QueueSourceEntity(
                        new OriginalSummitCheckpointState.ConfettiRenderer(engine, Position));
                    PlaySound("event:/char/dialogue/secret_character", Position);
                    break;
                case ItemCrystalContents.Seeker:
                    Vector2[] seekerNodes = new Vector2[1];
                    seekerNodes[0] = Position;
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalSeekerState(Position, seekerNodes, (IOriginalSeekerProcess)engine));
                    break;
                case ItemCrystalContents.Badeline_Chaser:
                    Level.SourceScene.QueueSourceEntity(entity =
                        ((IOriginalBadelineOldsiteEnvironment)engine).BadelineOldsites.Create(
                            new EntityData
                            {
                                Position = Position,
                                Values = new Dictionary<string, object> { ["canChangeMusic"] = false }
                            }, Vector2.Zero, 0, engine));
                    break;
                case ItemCrystalContents.Crystal_Heart:
                    Level.SourceScene.QueueSourceEntity(entity = new OriginalHeartGemState(engine, Position));
                    break;
                case ItemCrystalContents.Cloud:
                    Level.SourceScene.QueueSourceEntity(entity = new OriginalCloudState(engine,
                        Position + new Vector2(0, -24f), false,
                        Level.SolidWorldFactory.CreateJumpThru(Position + new Vector2(0, -24f), false)));
                    break;

                case ItemCrystalContents.Frail_Cloud:
                    Level.SourceScene.QueueSourceEntity(entity = new OriginalCloudState(engine,
                        Position + new Vector2(0, -24f), true,
                        Level.SolidWorldFactory.CreateJumpThru(Position + new Vector2(0, -24f), false)));
                    break;
                case ItemCrystalContents.Green_Booster:
                    Level.SourceScene.QueueSourceEntity(entity = new OriginalBoosterState(engine, Position, false));
                    break;
                case ItemCrystalContents.Red_Booster:
                    Level.SourceScene.QueueSourceEntity(entity = new OriginalBoosterState(engine, Position, true));
                    break;
                case ItemCrystalContents.Core_Toggle:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalCoreModeToggleState(engine, Position, false, false, false));
                    break;
                case ItemCrystalContents.Feather:
                    Level.SourceScene.QueueSourceEntity(entity =
                        new OriginalFlyFeatherState(Position, false, false, engine));
                    break;
                case ItemCrystalContents.Theo_Crystal:
                    Level.SourceScene.QueueSourceEntity(entity = new TheoCrystalState(Position, engine));
                    break;
            }

            Add(new SimulationCoroutine(engine, MoveEntityUp()));
            if (item != ItemCrystalContents.Summit_Confetti && item != ItemCrystalContents.Unlockable_Character)
            {
                PlaySound("event:/cherryhelper/itemcrystal_death", Position);
                CrystalDebrisState.Burst(engine, Position + Vector2.UnitY * -10f, SourceHexColor.HexToColor("ffffff"),
                    false, 32);
            }

            destroyed = true;
            Visible = false;
            Collidable = false;
            Speed = Vector2.Zero;
            yield return 1;
            RemoveSelf();
            yield break;
        }

        private IEnumerator EntityDelay()
        {
            Collidable = false;
            yield return 1.2f;
            Collidable = true;
            yield break;
        }

        public void PlayerRelease()
        {
            RoomPlayerSession player = Hold.Holder;
            if (player != null && player.Holding.Holding != null && player.Holding.Holding == Hold && Hold != null)
            {
                Hold.Holder.Machine.State = 0;
                player.Holding.SetHolding(null);
                Hold.CannotHoldDelay = 10;
                Hold.Release(Vector2.Zero);
                Hold.RemoveSelf();
                player.Components.Get<Sprite>().Update();
                Hold.PickupCollider = null;
            }
        }


        private IEnumerator DestroyWithoutItems()
        {
            if (destroyed)
            {
                yield break;
            }

            base.Collider = new CircleColliderGeometry(((ISourceVectorEnvironment)engine).VectorArithmetic, 5f);
            base.Collider = pushRadius;
            sprite.Play("idle", restart: true);
            level.Displacement.AddBurst(Position, 0.35f, 5f, 30f, 0.15f);
            RoomPlayerSession player = Hold.Holder;
            base.Collider = hitBox;
            Visible = false;
            Collidable = false;
            Vector2 saveSpeedForDeath = Speed;
            Speed = Vector2.Zero;
            PlayerRelease();
            CrystalDebrisState.Burst(engine, Position + Vector2.UnitY * -10f, SourceHexColor.HexToColor("639bff"),
                false, 5);
            destroyed = true;
            for (int j = 0; j < 10; j++)
            {
                PlaySound("event:/game/06_reflection/fall_spike_smash", Position);
            }

            if (dieIfDrop)
            {
                RoomPlayerSession play = Level.SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>();
                if (play != null)
                    play.Die(Vector2.Zero - saveSpeedForDeath, true);
            }
            else
            {
                yield return 2;
                Add(new SimulationCoroutine(engine, Respawn()));
            }

            yield break;
        }

        private IEnumerator Respawn()
        {
            Position = respawnPosition;
            WigglerComponent wiggler;
            Add(wiggler = WigglerComponent.Create(((ISourceWigglerPoolScope)engine).WigglerPool, 0.5f, 4f,
                delegate(float v) { sprite.Scale = (sprite.Scale = Vector2.One * (1f + v * 0.2f)); }, true, false));
            for (int j = 0; j < 5; j++)
            {
                PlaySound("event:/game/general/assist_nonsolid_out", Position);
            }

            Visible = true;
            Collidable = true;
            Speed = Vector2.Zero;
            yield return 0.5f;
            destroyed = false;
            yield break;
        }

        protected override void OnSquish(PixelCollision data)
        {
            if (!TrySquishWiggle(data))
            {
                Add(new SimulationCoroutine(engine, DestroyWithoutItems()));
            }
        }

        private IEnumerator MoveEntityUp()
        {
            yield return null;
            if (item == ItemCrystalContents.Fireball)
            {
                yield break;
            }

            if (item == ItemCrystalContents.Green_Booster || item == ItemCrystalContents.Red_Booster)
            {
                entity.Add(new SimulationCoroutine(engine, EntityDelay()));
            }

            if (item == ItemCrystalContents.Cloud || item == ItemCrystalContents.Frail_Cloud ||
                item == ItemCrystalContents.Unlockable_Character)
            {
                entity.Position = entity.Position + new Vector2(0, 24f);
            }

            Vector2 end = entity.Position + new Vector2(0, -24f);
            SourceTweenState tween = SourceTweenState.Create(engine.TweenPool, SourceTweenState.TweenMode.Oneshot,
                ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeInOut, 1.2f, start: true);
            tween.OnUpdate = delegate(SourceTweenState t)
            {
                entity.Position = Vector2Interpolation.Lerp(entity.Position, end, t.Eased,
                    ((ISourceScalarEnvironment)engine).ScalarArithmetic);
            };
            tween.OnComplete = delegate { };
            Add(tween);
        }
    }
}
