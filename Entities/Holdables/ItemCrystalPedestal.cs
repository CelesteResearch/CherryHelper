// SPDX-License-Identifier: MIT

using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Components.Animation;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Mods.CherryHelper.Components.Holdables;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;

namespace Celeste.Mods.CherryHelper.Entities.Holdables
{
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.ItemCrystalPedestal")]
    [CustomEntity("CherryHelper/ItemCrystalPedestal")]
    public class ItemCrystalPedestal : SceneEntityState
    {
        public Sprite sprite;
        private ItemCrystalCollider collid;
        private readonly ISourceEngineContext engine;

        public ItemCrystalPedestal(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset)
        {
            this.engine = engine;
            Position = data.Position + offset;
            Add(sprite =
                ((ISourceGameGraphicsEnvironment)engine).GameGraphics.SpriteFactory.Create("itemCrystalPedestal"));
            Add(collid = new ItemCrystalCollider(OnHoldable, new HitboxColliderGeometry(24f, 24f, -12f, -8f)));
            collid.Visible = true;
            collid.Active = true;
        }

        public void OnHoldable(ItemCrystal crystal)
        {
            if (crystal == null) return;
            crystal.PlayerRelease();
            crystal.PlayerRelease();
            SourceTweenState tween = SourceTweenState.Create(engine.TweenPool, SourceTweenState.TweenMode.Oneshot,
                ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeInOut, 0.4f, true);
            tween.OnUpdate = t =>
            {
                crystal.PlayerRelease();
                crystal.Position = Vector2Interpolation.Lerp(crystal.Position, Position, t.Eased,
                    ((ISourceScalarEnvironment)engine).ScalarArithmetic);
                crystal.sprite.Play("fill");
            };
            tween.OnComplete = t =>
            {
                crystal.Position = Position;
                crystal.Add(new SimulationCoroutine(engine, crystal.DestroySequence()));
            };
            Add(tween);
        }
    }
}
