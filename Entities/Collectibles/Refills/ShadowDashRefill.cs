using System;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Game.Gameplay.Collectibles.Refills;
using Celeste.Game.Presentation.Particles.Definitions.BadelineBoost;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Mods.CherryHelper.Modules;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Components.Oscillation;
using Celeste.Scene.Entities;
using Celeste.Simulation.Gameplay.Player.Collision.Components;
using Celeste.Simulation.Presentation.Effects.Mirrors;
using Celeste.Simulation.Presentation.Lighting;
using Celeste.Simulation.Presentation.PostProcessing.Bloom;

namespace Celeste.Mods.CherryHelper.Entities.Collectibles.Refills
{
    [CustomEntity("CherryHelper/ShadowDashRefill")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.ShadowDashRefill")]
    public partial class ShadowDashRefill : SceneEntityState
    {
        private readonly IOriginalRefillProcess engine;
        private readonly CherryHelperModule module;
        private readonly bool oneUse;
        private readonly Sprite sprite, flash;
        private readonly SpriteImage outline;
        private readonly WigglerComponent wiggler;
        private readonly BloomPointState bloom;
        private readonly VertexLightState light;
        private readonly SineWaveState sine;
        private readonly BadelineBoostParticleDefinitions badelineParticles;
        private readonly Action respawnAction;
        private readonly IRefillPresentationBinding presentation;
        private IOriginalRefillScene level;
        private float respawnTimer;

        public ShadowDashRefill(ISourceEngineContext engine, Vector2 position, bool oneUse)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            this.engine = (IOriginalRefillProcess)engine;
            module = CherryHelperModule.For(engine);
            this.oneUse = oneUse;
            badelineParticles = ((IOriginalGameplayProcess)engine).ParticleTypes.BadelineBoost;
            respawnAction = Respawn;
            Collider = new HitboxColliderGeometry(16, 16, -8, -8);
            Add(new PlayerColliderComponentState(OnPlayer));
            const string path = "objects/shadowDashRefill/";
            var atlas = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas;
            Add(outline = new SpriteImage(atlas[path + "outline"]));
            outline.CenterOrigin();
            outline.Visible = false;
            Add(sprite = new Sprite(engine, atlas, path + "idle"));
            sprite.AddLoop("idle", "", .1f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            Add(flash = new Sprite(engine, atlas, path + "flash"));
            flash.Add("flash", "", .05f);
            flash.OnFinish = _ => flash.Visible = false;
            flash.CenterOrigin();
            Add(wiggler = ((ISourceWigglerPoolScope)engine).WigglerPool.Create(1f, 4f,
                value => sprite.Scale = flash.Scale = Vector2.One * (1f + value * .2f)));
            Add(new MirrorReflectionState());
            Add(bloom = new BloomPointState(.8f, 16f));
            Add(light = new VertexLightState(engine, Vector2.Zero, Celeste.Core.Display.PackedColor.White, 1, 16, 48));
            Add(sine = new SineWaveState(engine, .6f));
            sine.Randomize();
            RefillPresentation.UpdateY(sprite, flash, bloom, sine);
            presentation = RefillPresentation.BindPresentation(this.engine, new PresentationPhases(this));
            Depth = -100;
        }

        public ShadowDashRefill(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : this(engine, data.Position + offset, data.Bool("oneUse"))
        {
        }

        public static SceneEntityState Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset, OriginalLevelEntityData data) => new ShadowDashRefill(level.EngineContext, data, offset);

        public override void Added(object scene)
        {
            base.Added(scene);
            level = (IOriginalRefillScene)scene;
        }

        public override void Update()
        {
            base.Update();
            presentation.Update();
        }

        public override void Render(Celeste.Core.Rendering.ISourceRenderContext context)
        {
            RefillPresentation.RenderOutline(context, sprite);
            base.Render(context);
        }
    }
}
