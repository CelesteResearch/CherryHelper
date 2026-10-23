using Celeste.Core.Display;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Scheduling;
using Celeste.Core.Rendering;
using Celeste.Core.Geometry;
using Celeste.Core.Audio;
using Celeste.Core.Randomness;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Mathematics.LinearAlgebra;
using Celeste.Core.Components.Timing;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Display;
using Celeste.Graphics.Rendering;
using Celeste.Simulation;
using Celeste.Simulation.Audio.Sources;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.Particles.Definitions;
using Celeste.Simulation.Gameplay.Environment.Water;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Player.Collision.Surfaces;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Formats.Map.Entities.Attributes;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Controls.Source;
using Celeste.Game.Gameplay.Platforms.FallingBlocks;
using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Simulation.Gameplay.Platforms.Crushing;

namespace Celeste.Mods.CherryHelper.Gameplay.Platforms.Crushing
{
    public partial class NonReturningCrushBlockBase
    {
        private void TurnOffImages()
        {
            foreach (SpriteImage image in this.activeLeftImages)
            {
                image.Visible = false;
            }

            foreach (SpriteImage image2 in this.activeRightImages)
            {
                image2.Visible = false;
            }

            foreach (SpriteImage image3 in this.activeTopImages)
            {
                image3.Visible = false;
            }

            foreach (SpriteImage image4 in this.activeBottomImages)
            {
                image4.Visible = false;
            }
        }

        public virtual DashCollisionResult OnDashed(RoomPlayerSession player, Vector2 direction)
        {
            bool flag = this.CanActivate(-direction);
            DashCollisionResult result;
            if (flag)
            {
                this.Attack(-direction);
                result = DashCollisionResult.Rebound;
            }
            else
            {
                result = DashCollisionResult.NormalCollision;
            }

            return result;
        }

        public bool CanActivate(Vector2 direction)
        {
            bool flag = this.giant && direction.X <= 0f;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = this.canActivate && this.crushDir != direction;
                if (flag2)
                {
                    bool flag3 = direction.X != 0f && !this.canMoveHorizontally;
                    if (flag3)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag4 = direction.Y != 0f && !this.canMoveVertically;
                        result = !flag4;
                    }
                }
                else
                {
                    result = false;
                }
            }

            return result;
        }

        public void Attack(Vector2 direction)
        {
            Process.AudioSession.Play("event:/game/06_reflection/crushblock_activate", Center, Process.SaveContext);
            bool flag = this.currentMoveLoopSfx != null;
            if (flag)
            {
                this.currentMoveLoopSfx.Param("end", 1f);
                SoundSourceComponentState sfx = this.currentMoveLoopSfx;
                SourceAlarmState.Set(engine.AlarmPool, OwnedComponents, 0.5f, delegate { sfx.RemoveSelf(); },
                    SourceAlarmState.AlarmMode.Oneshot);
            }

            Add(this.currentMoveLoopSfx = new SoundSourceComponentState(engine));
            this.currentMoveLoopSfx.Position = new Vector2(Width, Height) / 2f;
            this.currentMoveLoopSfx.Play("event:/game/06_reflection/crushblock_move_loop", null, 0f);
            this.face.Play("hit", false, false);
            this.crushDir = direction;
            this.canActivate = false;
            this.attackCoroutine.Replace(this.AttackSequence());
            ClearRemainder();
            this.TurnOffImages();
            ActivateParticles(this.crushDir);
            bool flag2 = this.crushDir.X < 0f;
            if (flag2)
            {
                foreach (SpriteImage image in this.activeLeftImages)
                {
                    image.Visible = true;
                }

                this.nextFaceDirection = "left";
            }
            else
            {
                bool flag3 = this.crushDir.X > 0f;
                if (flag3)
                {
                    foreach (SpriteImage image2 in this.activeRightImages)
                    {
                        image2.Visible = true;
                    }

                    this.nextFaceDirection = "right";
                }
                else
                {
                    bool flag4 = this.crushDir.Y < 0f;
                    if (flag4)
                    {
                        foreach (SpriteImage image3 in this.activeTopImages)
                        {
                            image3.Visible = true;
                        }

                        this.nextFaceDirection = "up";
                    }
                    else
                    {
                        bool flag5 = this.crushDir.Y > 0f;
                        if (flag5)
                        {
                            foreach (SpriteImage image4 in this.activeBottomImages)
                            {
                                image4.Visible = true;
                            }

                            this.nextFaceDirection = "down";
                        }
                    }
                }
            }

            bool flag6 = true;
            bool flag7 = this.returnStack.Count > 0;
            if (flag7)
            {
                NonReturningCrushBlockBase.MoveState moveState = this.returnStack[this.returnStack.Count - 1];
                bool flag8 = moveState.Direction == direction || moveState.Direction == -direction;
                if (flag8)
                {
                    flag6 = false;
                }
            }

            bool flag9 = flag6;
            if (flag9)
            {
                this.returnStack.Add(new NonReturningCrushBlockBase.MoveState(this.Position, this.crushDir));
            }
        }

        private void ActivateParticles(Vector2 dir)
        {
            bool flag = dir == Vector2.UnitX;
            float direction;
            Vector2 position;
            Vector2 positionRange;
            int num;
            if (flag)
            {
                direction = 0f;
                position = CenterRight - Vector2.UnitX;
                positionRange = Vector2.UnitY * (Height - 2f) * 0.5f;
                num = (int)(Height / 8f) * 4;
            }
            else
            {
                bool flag2 = dir == -Vector2.UnitX;
                if (flag2)
                {
                    direction = 3.14159274f;
                    position = CenterLeft + Vector2.UnitX;
                    positionRange = Vector2.UnitY * (Height - 2f) * 0.5f;
                    num = (int)(Height / 8f) * 4;
                }
                else
                {
                    bool flag3 = dir == Vector2.UnitY;
                    if (flag3)
                    {
                        direction = 1.57079637f;
                        position = BottomCenter - Vector2.UnitY;
                        positionRange = Vector2.UnitX * (Width - 2f) * 0.5f;
                        num = (int)(Width / 8f) * 4;
                    }
                    else
                    {
                        direction = -1.57079637f;
                        position = TopCenter + Vector2.UnitY;
                        positionRange = Vector2.UnitX * (Width - 2f) * 0.5f;
                        num = (int)(Width / 8f) * 4;
                    }
                }
            }

            num += 2;
            this.level.Particles.Emit(P_Activate, num, position, positionRange, direction);
        }
    }
}
