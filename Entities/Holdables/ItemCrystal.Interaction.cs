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
        public override void Added(object scene)
        {
            base.Added(scene);
            Level = Scene as OriginalLevelSceneRoot;
            if (level != null)
            {
            }
        }

        public void CheckAgainstColliders()
        {
            foreach (ItemCrystalCollider component in ((ISourceScene)Scene).SourceScene.Components.GetTrackedComponents(
                         itemCrystalColliderKey))
            {
                if (component.Check(this))
                {
                    component.OnCollide(this);
                }
            }
        }

        private void OnCollideSpring(OriginalSpringState spring)
        {
            Sprite sprite = spring.OwnedComponents.Get<Sprite>();
            SpringOrientation springOrientation = (sprite.Rotation == (float)Math.PI / 2f)
                ? SpringOrientation.WallLeft
                : ((sprite.Rotation == -(float)Math.PI / 2f) ? SpringOrientation.WallRight : SpringOrientation.Floor);
            PlaySound("event:/game/general/spring", spring.BottomCenter);
            spring.OwnedComponents.Get<SourceStaticMoverState>().TriggerPlatform();
            spring.OwnedComponents.Get<WigglerComponent>().Start();
            spring.OwnedComponents.Get<Sprite>().Play("bounce", restart: true);
            HitSpring(springOrientation);
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            itemCrystalColliderKey = ((ISourceScene)scene).SourceScene.Components.Catalog
                .Find(typeof(ItemCrystalCollider)).Name;
            springs = ((ISourceScene)scene).SourceScene.Entities.Entities.OfType<OriginalSpringState>().ToList();
            cassetteBlocks = ((ISourceScene)scene).SourceScene.Entities.GetTrackedEntities("Celeste.CassetteBlock")
                .Cast<CassetteBlockState>().ToList();
            playerEntity = ((ISourceScene)scene).SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>();
            if (showTutorial)
            {
                tutorialGui = new BirdTutorialGuiState(engine, ((IOriginalGameTagsEnvironment)engine).Tags.HUD.Value,
                    (ITheoCrystalTutorialResources)engine, this, new Vector2(0f, -24f),
                    ((ITheoCrystalTutorialResources)engine).CleanDialog("tutorial_carry"),
                    ((ITheoCrystalTutorialResources)engine).CleanDialog("tutorial_hold"), BirdTutorialButton.Grab);
                tutorialGui.Open = false;
                shouldShowTutorial = true;
                ((ISourceScene)Scene).SourceScene.QueueSourceEntity(tutorialGui);
            }
        }

        private void OnPickup()
        {
            if (tutorialGui != null)
            {
                shouldShowTutorial = false;
            }

            Speed = Vector2.Zero;
        }

        public void HitSpring(SpringOrientation springOrientation)
        {
            if (!Hold.IsHeld)
            {
                if (springOrientation == SpringOrientation.Floor)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                }
                else
                {
                    Speed.X = 240 * ((springOrientation == SpringOrientation.WallLeft) ? 1 : (-1));
                    Speed.Y = -140f;
                    noGravityTimer = 0.15f;
                }
            }
        }

        private void OnRelease(Vector2 force)
        {
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }

            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
        }

        private void PlaySound(string path, Vector2? position = null, string parameter = null, float value = 0f)
        {
            var process = (IOriginalGameplayProcess)engine;
            if (!position.HasValue) process.AudioSession.Play(path);
            else if (parameter == null) process.AudioSession.Play(path, position.Value, process.SaveContext);
            else process.AudioSession.Play(path, position.Value, process.SaveContext, parameter, value);
        }
    }
}
