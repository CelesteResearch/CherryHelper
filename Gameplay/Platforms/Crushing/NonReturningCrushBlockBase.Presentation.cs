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
        private static readonly string sourcePlayerRegistryKey =
            typeof(global::Celeste.Simulation.RoomPlayerSession).FullName;

        public override void Added(object scene)
        {
            base.Added(scene);
            this.level = ((OriginalLevelSceneRoot)Scene);
        }

        public override void Update()
        {
            if (!((OriginalLevelSceneRoot)Scene).IsInBounds(this))
            {
                RemoveSelf();
            }

            base.Update();
            bool flag = this.crushDir == Vector2.Zero;
            if (flag)
            {
                this.face.Position = new Vector2(Width, Height) / 2f;
                bool flag2 = CollideCheck(sourcePlayerRegistryKey, this.Position + new Vector2(-1f, 0f));
                if (flag2)
                {
                    this.face.X -= 1f;
                }
                else
                {
                    bool flag3 = CollideCheck(sourcePlayerRegistryKey, this.Position + new Vector2(1f, 0f));
                    if (flag3)
                    {
                        this.face.X += 1f;
                    }
                    else
                    {
                        bool flag4 = CollideCheck(sourcePlayerRegistryKey, this.Position + new Vector2(0f, -1f));
                        if (flag4)
                        {
                            this.face.Y -= 1f;
                        }
                    }
                }
            }

            bool flag5 = this.currentMoveLoopSfx != null;
            if (flag5)
            {
                this.currentMoveLoopSfx.Param("submerged", (float)(this.Submerged ? 1 : 0));
            }

            bool flag6 = this.returnLoopSfx != null;
            if (flag6)
            {
                this.returnLoopSfx.Param("submerged", (float)(this.Submerged ? 1 : 0));
            }
        }

        public override void Render(ISourceRenderContext context)
        {
            Vector2 position = this.Position;
            this.Position += Shake;
            SourcePrimitiveDrawing.Rect(context, this, X + 2f, Y + 2f, Width - 4f, Height - 4f, this.fill);
            base.Render(context);
            this.Position = position;
        }

        private bool Submerged
        {
            get
            {
                return SourceSceneCollision.CheckRect(Tracker, "Celeste.Water",
                    new IntRect((int)(Center.X - 4f), (int)Center.Y, 8, 4));
            }
        }

        public void AddImage(AtlasEntry idle, int x, int y, int tx, int ty, int borderX = 0, int borderY = 0)
        {
            AtlasEntry subtexture = idle.GetSubtexture(tx * 8, ty * 8, 8, 8);
            Vector2 vector = new Vector2((float)(x * 8), (float)(y * 8));
            bool flag = borderX != 0;
            if (flag)
            {
                Add(new SpriteImage(subtexture)
                {
                    Color = PackedColor.Black,
                    Position = vector + new Vector2((float)borderX, 0f)
                });
            }

            bool flag2 = borderY != 0;
            if (flag2)
            {
                Add(new SpriteImage(subtexture)
                {
                    Color = PackedColor.Black,
                    Position = vector + new Vector2(0f, (float)borderY)
                });
            }

            SpriteImage image = new SpriteImage(subtexture);
            image.Position = vector;
            Add(image);
            this.idleImages.Add(image);
            bool flag3 = borderX != 0 || borderY != 0;
            if (flag3)
            {
                bool flag4 = borderX < 0;
                if (flag4)
                {
                    SpriteImage image2 = new SpriteImage(Graphics.GameAtlas[spriteDirectory + "/lit_left"]
                        .GetSubtexture(0, ty * 8, 8, 8));
                    this.activeLeftImages.Add(image2);
                    image2.Position = vector;
                    image2.Visible = false;
                    Add(image2);
                }
                else
                {
                    bool flag5 = borderX > 0;
                    if (flag5)
                    {
                        SpriteImage image3 = new SpriteImage(Graphics.GameAtlas[spriteDirectory + "/lit_right"]
                            .GetSubtexture(0, ty * 8, 8, 8));
                        this.activeRightImages.Add(image3);
                        image3.Position = vector;
                        image3.Visible = false;
                        Add(image3);
                    }
                }

                bool flag6 = borderY < 0;
                if (flag6)
                {
                    SpriteImage image4 = new SpriteImage(Graphics.GameAtlas[spriteDirectory + "/lit_top"]
                        .GetSubtexture(tx * 8, 0, 8, 8));
                    this.activeTopImages.Add(image4);
                    image4.Position = vector;
                    image4.Visible = false;
                    Add(image4);
                }
                else
                {
                    bool flag7 = borderY > 0;
                    if (flag7)
                    {
                        SpriteImage image5 = new SpriteImage(Graphics.GameAtlas[spriteDirectory + "/lit_bottom"]
                            .GetSubtexture(tx * 8, 0, 8, 8));
                        this.activeBottomImages.Add(image5);
                        image5.Position = vector;
                        image5.Visible = false;
                        Add(image5);
                    }
                }
            }
        }
    }
}
