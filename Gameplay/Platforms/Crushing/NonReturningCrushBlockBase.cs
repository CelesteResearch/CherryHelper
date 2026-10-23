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
    public abstract partial class NonReturningCrushBlockBase : SourceSolidEntityState,
        ICrushingSolidCollisionCallbackSource
    {
        private string spriteDirectory;
        private readonly bool uninterrupted;
        public Action<Vector2, Vector2, ISourcePlatformEntity> CrushingCollisionCallback { get; set; }
        private readonly ISourceEngineContext engine;
        private IOriginalGameplayProcess Process => (IOriginalGameplayProcess)engine;

        private OriginalGameGraphicsResources Graphics =>
            (OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics;

        private SceneEntityRegistry Tracker => ((ISourceScene)Scene).SourceScene.Entities;

        protected NonReturningCrushBlockBase(ISourceEngineContext engine, EntityData data, Vector2 offset,
            ISourceSolidWorld world, string defaultSpriteDirectory, bool uninterrupted)
            : base(data.Position + offset, data.Width, data.Height, false, world)
        {
            this.engine = engine;
            this.uninterrupted = uninterrupted;
            P_Crushing = Process.ParticleTypes.CrushBlock.P_Crushing;
            P_Activate = Process.ParticleTypes.CrushBlock.P_Activate;
            axes = data.Enum("axes", Axes.Both);
            chillOut = data.Bool("chillOut", false);

            if (data.Bool("reskinnable"))
            {
                P_Crushing = new ParticleTypePresentation(P_Crushing)
                {
                    Color = PackedColor.FromHex(data.Attr("crushParticleColor1", "ff66e2")),
                    Color2 = PackedColor.FromHex(data.Attr("crushParticleColor2", "68fcff")),
                };

                P_Activate = new ParticleTypePresentation(P_Activate)
                {
                    Color = PackedColor.FromHex(data.Attr("activateParticleColor1", "5fcde4")),
                    Color2 = PackedColor.FromHex(data.Attr("activateParticleColor2", "ffffff")),
                };

                spriteDirectory = data.Attr("spriteDirectory", defaultSpriteDirectory);
                fill = PackedColor.FromHex(data.Attr("fillColor", "242262"));
            }
            else if (data.Bool("altTexture", true))
            {
                spriteDirectory = data.Attr("spriteDirectory", defaultSpriteDirectory);
                fill = PackedColor.FromHex(data.Attr("fillColor", "242262"));
            }
            else
            {
                spriteDirectory = "objects/crushblock";
                fill = PackedColor.FromHex("62222b");
            }

            this.idleImages = new List<SpriteImage>();
            this.activeTopImages = new List<SpriteImage>();
            this.activeRightImages = new List<SpriteImage>();
            this.activeLeftImages = new List<SpriteImage>();
            this.activeBottomImages = new List<SpriteImage>();
            this.OnDashCollide = OnDashed;
            this.returnStack = new List<NonReturningCrushBlockBase.MoveState>();
            this.giant = (Width >= 48f && Height >= 48f && chillOut);
            this.canActivate = true;
            this.attackCoroutine = new SimulationCoroutine(engine);
            this.attackCoroutine.RemoveOnComplete = false;
            Add(this.attackCoroutine);
            List<AtlasEntry> atlasSubtextures = Graphics.GameAtlas.GetAtlasSubtextures(spriteDirectory + "/block");
            AtlasEntry idle;
            switch (axes)
            {
                default:
                    idle = atlasSubtextures[3];
                    this.canMoveHorizontally = (this.canMoveVertically = true);
                    break;
                case Axes.Horizontal:
                    idle = atlasSubtextures[1];
                    this.canMoveHorizontally = true;
                    this.canMoveVertically = false;
                    break;
                case Axes.Vertical:
                    idle = atlasSubtextures[2];
                    this.canMoveHorizontally = false;
                    this.canMoveVertically = true;
                    break;
            }

            Add(face = new Sprite(engine, Graphics.GameAtlas, spriteDirectory + "/"));
            face.Position = new Vector2(Width, Height) / 2f;
            if (giant)
            {
                /*
                  <Loop id="idle" path="giant_block" frames="0" delay="0.08"/>
                  <Anim id="hurt"  path="giant_block" frames="8-12" delay="0.08" goto="idle"/>
                  <Anim id="hit" path="giant_block" frames="0-5" delay="0.08"/>
                  <Loop id="right" path="giant_block" frames="6,7"  delay="0.08"/>
                */
                face.AddLoop("idle", "giant_block", 0.08f, 0);
                face.Add("hurt", "giant_block", 0.08f, "idle", 8, 9, 10, 11, 12);
                face.Add("hit", "giant_block", 0.08f, 0, 1, 2, 3, 4, 5);
                face.AddLoop("right", "giant_block", 0.08f, 6, 7);
            }
            else
            {
                /*
                  <Loop id="idle" path="idle_face" delay="0.08"/>
                  <Anim id="hurt" path="hurt" frames="3-12" delay="0.08" goto="idle"/>
                  <Anim id="hit" path="hit" delay="0.08"/>
                  <Loop id="left" path="hit_left" delay="0.08"/>
                  <Loop id="right" path="hit_right" delay="0.08"/>
                  <Loop id="up" path="hit_up" delay="0.08"/>
                  <Loop id="down" path="hit_down" delay="0.08"/>
                */

                face.AddLoop("idle", "idle_face", 0.08f);
                face.Add("hurt", "hurt", 0.08f, "idle", 3, 4, 5, 6, 7, 8, 9, 10, 11, 12);
                face.Add("hit", "hit", 0.08f);
                face.AddLoop("left", "hit_left", 0.08f);
                face.AddLoop("right", "hit_right", 0.08f);
                face.AddLoop("up", "hit_up", 0.08f);
                face.AddLoop("down", "hit_down", 0.08f);
            }

            face.CenterOrigin();
            face.Play("idle");
            this.face.OnLastFrame = delegate(string f)
            {
                bool flag = f == "hit";
                if (flag)
                {
                    this.face.Play(this.nextFaceDirection, false, false);
                }
            };
            int num = (int)(Width / 8f) - 1;
            int num2 = (int)(Height / 8f) - 1;
            this.AddImage(idle, 0, 0, 0, 0, -1, -1);
            this.AddImage(idle, num, 0, 3, 0, 1, -1);
            this.AddImage(idle, 0, num2, 0, 3, -1, 1);
            this.AddImage(idle, num, num2, 3, 3, 1, 1);
            for (int i = 1; i < num; i++)
            {
                this.AddImage(idle, i, 0, ((ISourceRandomEnvironment)engine).Random.Choose(1, 2), 0, 0, -1);
                this.AddImage(idle, i, num2, ((ISourceRandomEnvironment)engine).Random.Choose(1, 2), 3, 0, 1);
            }

            for (int j = 1; j < num2; j++)
            {
                this.AddImage(idle, 0, j, 0, ((ISourceRandomEnvironment)engine).Random.Choose(1, 2), -1, 0);
                this.AddImage(idle, num, j, 3, ((ISourceRandomEnvironment)engine).Random.Choose(1, 2), 1, 0);
            }

            Add(new LightOccludeState(0.2f));
            Add(this.returnLoopSfx = new SoundSourceComponentState(engine));
            Add(((IWaterInteractionConstructionEnvironment)engine).WaterInteractions.Create(() =>
                this.crushDir != Vector2.Zero));
        }

        public ParticleTypePresentation P_Crushing;
        public ParticleTypePresentation P_Activate;
        private const float CrushSpeed = 120f; // was 240f
        private const float CrushAccel = 250f; // was 500f
        private const float ReturnSpeed = 30f; // was 60f
        private const float ReturnAccel = 80f; // was 160f
        public PackedColor fill;
        public Axes axes;
        protected OriginalLevelSceneRoot level;
        private bool canActivate;
        private Vector2 crushDir;
        private List<NonReturningCrushBlockBase.MoveState> returnStack;
        private SimulationCoroutine attackCoroutine;
        public bool canMoveVertically;
        public bool canMoveHorizontally;
        private bool chillOut;
        public bool giant;
        public Sprite face;
        private string nextFaceDirection;
        public List<SpriteImage> idleImages;
        public List<SpriteImage> activeTopImages;
        public List<SpriteImage> activeRightImages;
        public List<SpriteImage> activeLeftImages;
        public List<SpriteImage> activeBottomImages;
        private SoundSourceComponentState currentMoveLoopSfx;
        private SoundSourceComponentState returnLoopSfx;

        public enum Axes
        {
            Both,
            Horizontal,
            Vertical
        }

        public List<SpriteImage> ImgList;

        private struct MoveState
        {
            public MoveState(Vector2 from, Vector2 direction)
            {
                this.From = from;
                this.Direction = direction;
            }

            public Vector2 From;
            public Vector2 Direction;
        }
    }
}
