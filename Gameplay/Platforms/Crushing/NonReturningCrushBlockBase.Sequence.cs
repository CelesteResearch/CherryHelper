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
        private IEnumerator AttackSequence()
        {
            ((IOriginalGameInputProcessEnvironment)engine).InputSource.Rumble(RumbleStrength.Strong,
                RumbleLength.Medium);
            this.StartShaking(0.4f);
            yield return 0.4f;
            bool flag = !this.chillOut && !uninterrupted;
            if (flag)
            {
                this.canActivate = true;
            }

            this.StopPlayerRunIntoAnimation = false;
            bool slowing = false;
            float speed = 0f;
            Action som = null; // = null wasn't there
            for (;;)
            {
                bool flag2 = !this.chillOut;
                if (flag2)
                {
                    speed = ScalarMath.Approach(speed, 240f, 500f * engine.DeltaTime);
                }
                else
                {
                    bool flag3 = slowing ||
                                 this.CollideCheck("Celeste.SolidTiles", this.Position + this.crushDir * 256f);
                    if (flag3)
                    {
                        speed = ScalarMath.Approach(speed, 24f, 500f * engine.DeltaTime * 0.25f);
                        bool flag4 = !slowing;
                        if (flag4)
                        {
                            slowing = true;
                            float duration = 0.5f;
                            Action onComplete;
                            if ((onComplete = som) == null)
                            {
                                onComplete = (som = delegate()
                                {
                                    this.face.Play("hurt", false, false);
                                    this.currentMoveLoopSfx.Stop(true);
                                    this.TurnOffImages();
                                });
                            }

                            SourceAlarmState.Set(engine.AlarmPool, OwnedComponents, duration, onComplete,
                                SourceAlarmState.AlarmMode.Oneshot);
                        }
                    }
                    else
                    {
                        speed = ScalarMath.Approach(speed, 120f, 250f * engine.DeltaTime); // was speed, 240f, 500f
                    }
                }

                bool flag5 = this.crushDir.X != 0f;
                bool hit;
                if (flag5)
                {
                    hit = this.MoveHorizontalCheck(speed * this.crushDir.X * engine.DeltaTime);
                }
                else
                {
                    hit = this.MoveVerticalCheck(speed * this.crushDir.Y * engine.DeltaTime);
                }

                bool flag6 = hit;
                if (flag6)
                {
                    break;
                }

                bool flag7 = ((ISourceScene)Scene).SourceScene.Clock.OnInterval(0.02f, engine);
                if (flag7)
                {
                    bool flag8 = this.crushDir == Vector2.UnitX;
                    Vector2 at;
                    float dir;
                    if (flag8)
                    {
                        at = new Vector2(this.Left + 1f,
                            SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random, this.Top + 3f,
                                this.Bottom - 3f));
                        dir = 3.14159274f;
                    }
                    else
                    {
                        bool flag9 = this.crushDir == -Vector2.UnitX;
                        if (flag9)
                        {
                            at = new Vector2(this.Right - 1f,
                                SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random, this.Top + 3f,
                                    this.Bottom - 3f));
                            dir = 0f;
                        }
                        else
                        {
                            bool flag10 = this.crushDir == Vector2.UnitY;
                            if (flag10)
                            {
                                at = new Vector2(
                                    SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random, this.Left + 3f,
                                        this.Right - 3f), this.Top + 1f);
                                dir = -1.57079637f;
                            }
                            else
                            {
                                at = new Vector2(
                                    SourceRandomMath.Range(((ISourceRandomEnvironment)engine).Random, this.Left + 3f,
                                        this.Right - 3f), this.Bottom - 1f);
                                dir = 1.57079637f;
                            }
                        }
                    }

                    level.Particles.Emit(P_Crushing, at, dir);
                    at = default(Vector2);
                }

                yield return null;
            }

            OriginalFallingBlockState fallingBlock =
                SourceTrackedEntityCollision.FirstAt<OriginalFallingBlockState>(Tracker, "Celeste.FallingBlock", this,
                    this.Position + this.crushDir);
            bool flag11 = fallingBlock != null;
            if (flag11)
            {
                fallingBlock.Triggered = true;
            }

            bool flag12 = this.crushDir == -Vector2.UnitX;
            if (flag12)
            {
                Vector2 add = new Vector2(0f, 2f);
                int i = 0;
                while ((float)i < this.Height / 8f)
                {
                    Vector2 at2 = new Vector2(this.Left - 1f, this.Top + 4f + (float)(i * 8));
                    bool flag13 = !SourceSceneCollision.CheckPoint(Tracker, "Celeste.Water", at2) &&
                                  SourceSceneCollision.CheckPoint(Tracker, "Celeste.Solid", at2);
                    if (flag13)
                    {
                        ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(Process.ParticleTypes.CrushBlock.P_Impact,
                            at2 + add, 0f);
                        ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(Process.ParticleTypes.CrushBlock.P_Impact,
                            at2 - add, 0f);
                    }

                    at2 = default(Vector2);
                    int num = i;
                    i = num + 1;
                }

                add = default(Vector2);
            }
            else
            {
                bool flag14 = this.crushDir == Vector2.UnitX;
                if (flag14)
                {
                    Vector2 add2 = new Vector2(0f, 2f);
                    int j = 0;
                    while ((float)j < this.Height / 8f)
                    {
                        Vector2 at3 = new Vector2(this.Right + 1f, this.Top + 4f + (float)(j * 8));
                        bool flag15 = !SourceSceneCollision.CheckPoint(Tracker, "Celeste.Water", at3) &&
                                      SourceSceneCollision.CheckPoint(Tracker, "Celeste.Solid", at3);
                        if (flag15)
                        {
                            ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(Process.ParticleTypes.CrushBlock.P_Impact,
                                at3 + add2, 3.14159274f);
                            ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(Process.ParticleTypes.CrushBlock.P_Impact,
                                at3 - add2, 3.14159274f);
                        }

                        at3 = default(Vector2);
                        int num = j;
                        j = num + 1;
                    }

                    add2 = default(Vector2);
                }
                else
                {
                    bool flag16 = this.crushDir == -Vector2.UnitY;
                    if (flag16)
                    {
                        Vector2 add3 = new Vector2(2f, 0f);
                        int k = 0;
                        while ((float)k < this.Width / 8f)
                        {
                            Vector2 at4 = new Vector2(this.Left + 4f + (float)(k * 8), this.Top - 1f);
                            bool flag17 = !SourceSceneCollision.CheckPoint(Tracker, "Celeste.Water", at4) &&
                                          SourceSceneCollision.CheckPoint(Tracker, "Celeste.Solid", at4);
                            if (flag17)
                            {
                                ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(
                                    Process.ParticleTypes.CrushBlock.P_Impact, at4 + add3, 1.57079637f);
                                ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(
                                    Process.ParticleTypes.CrushBlock.P_Impact, at4 - add3, 1.57079637f);
                            }

                            at4 = default(Vector2);
                            int num = k;
                            k = num + 1;
                        }

                        add3 = default(Vector2);
                    }
                    else
                    {
                        bool flag18 = this.crushDir == Vector2.UnitY;
                        if (flag18)
                        {
                            Vector2 add4 = new Vector2(2f, 0f);
                            int l = 0;
                            while ((float)l < this.Width / 8f)
                            {
                                Vector2 at5 = new Vector2(this.Left + 4f + (float)(l * 8), this.Bottom + 1f);
                                bool flag19 = !SourceSceneCollision.CheckPoint(Tracker, "Celeste.Water", at5) &&
                                              SourceSceneCollision.CheckPoint(Tracker, "Celeste.Solid", at5);
                                if (flag19)
                                {
                                    ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(
                                        Process.ParticleTypes.CrushBlock.P_Impact, at5 + add4, -1.57079637f);
                                    ((OriginalLevelSceneRoot)Scene).ParticlesFG.Emit(
                                        Process.ParticleTypes.CrushBlock.P_Impact, at5 - add4, -1.57079637f);
                                }

                                at5 = default(Vector2);
                                int num = l;
                                l = num + 1;
                            }

                            add4 = default(Vector2);
                        }
                    }
                }
            }

            Process.AudioSession.Play("event:/game/06_reflection/crushblock_impact", Center, Process.SaveContext);
            this.level.DirectionalShake(this.crushDir, 0.3f);
            ((IOriginalGameInputProcessEnvironment)engine).InputSource.Rumble(RumbleStrength.Medium,
                RumbleLength.Medium);
            this.StartShaking(0.4f);
            this.StopPlayerRunIntoAnimation = true;
            SoundSourceComponentState sfx = this.currentMoveLoopSfx;
            this.currentMoveLoopSfx.Param("end", 1f);
            this.currentMoveLoopSfx = null;
            if (uninterrupted) canActivate = true;
            SourceAlarmState.Set(engine.AlarmPool, OwnedComponents, 0.5f, delegate { sfx.RemoveSelf(); },
                SourceAlarmState.AlarmMode.Oneshot);
            this.crushDir = Vector2.Zero;
            this.TurnOffImages();
            yield return this.face.PlayRoutine("hurt", false);
            this.face.Play("idle", false, false);
            yield break;
        }

        protected abstract bool MoveHorizontalCheck(float amount);
        protected abstract bool MoveVerticalCheck(float amount);
    }
}
