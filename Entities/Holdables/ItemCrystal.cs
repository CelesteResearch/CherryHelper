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
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.ItemCrystal")]
    [CustomEntity("CherryHelper/ItemCrystal")]
    [SceneTracked]
    public partial class ItemCrystal : SourceActorEntityState
    {
        private readonly ISourceEngineContext engine;
        private string itemCrystalColliderKey;
        public HoldableState Hold;
        public Sprite sprite;
        private Vector2 respawnPosition;
        private bool showTutorial;
        private ItemCrystalContents item;
        private SourceCollisionHandler onCollideH;

        private SourceCollisionHandler onCollideV;
        public SceneEntityState entity;

        public Vector2 Speed;

        private float noGravityTimer;

        private Vector2 prevLiftSpeed;

        private float hardVerticalHitSoundCooldown;

        private OriginalLevelSceneRoot Level;

        private Vector2 previousPosition;


        private List<OriginalSpringState> springs;
        private bool destroyed;
        private List<CassetteBlockState> cassetteBlocks;

        private RoomPlayerSession playerEntity;

        private readonly CircleColliderGeometry pushRadius = null;

        private HitboxColliderGeometry hitBox;


        public OriginalSwitchComponent switcher;


        private float frameCount;


        private BirdTutorialGuiState tutorialGui;

        private bool shouldShowTutorial = true;
        private bool destroying;
        private bool dieIfDrop;
        private OriginalLevelSceneRoot level => (OriginalLevelSceneRoot)Scene;

        public ItemCrystal(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(new PixelActor(null, data.Position + offset), engine,
                ((ISourcePlatformTargetMotionEnvironment)engine).TargetMotionPolicy)
        {
            this.engine = engine;
            base.Depth = 100;
            hitBox = new HitboxColliderGeometry(8f, 10f, -4f, -10f);
            base.Collider = hitBox;
            dieIfDrop = data.Bool("dieIfDropped", false);
            Add(sprite = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.SpriteFactory.Create("itemCrystal"));
            Add(Hold = new HoldableState(engine));
            Hold.PickupCollider = new HitboxColliderGeometry(16f, 16f, -8f, -16f);
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            respawnPosition = Position;
            sprite.Position.Y = sprite.Position.Y - 8f;
            showTutorial = data.Bool("showTutorial", false);
            string pain = data.Attr("item");
            destroying = false;
            Enum.TryParse(pain, out item);
            if (item == ItemCrystalContents.Touch_Switch)
            {
                Add(switcher = new OriginalSwitchComponent(false));
            }

            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Hold.SpeedGetter = (() => Speed);
            Add(new VertexLightState(engine, base.Collider.Center, PackedColor.White, 1f, 32, 64));
            Add(new MirrorReflectionState());
        }
    }
}
