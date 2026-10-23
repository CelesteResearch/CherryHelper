// SPDX-License-Identifier: MIT

using System;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Display;
using Celeste.Core.Rendering;
using Celeste.Mods.CherryHelper.Entities.Holdables;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking.Attributes;

namespace Celeste.Mods.CherryHelper.Components.Holdables
{
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.ItemCrystalCollider")]
    [SceneTracked]
    public class ItemCrystalCollider : SourceComponentState
    {
        public Action<ItemCrystal> OnCollide;
        public ColliderGeometry Collider;
        public ColliderGeometry FeatherCollider;

        public ItemCrystalCollider(Action<ItemCrystal> onCollide, ColliderGeometry collider = null)
            : base(false, false)
        {
            OnCollide = onCollide;
            Collider = collider;
        }

        public bool Check(ItemCrystal crystal)
        {
            ColliderGeometry collider = Collider;
            var entity = (SceneEntityState)Entity;
            if (collider == null)
            {
                if (SourceEntityCollision.Check(crystal, entity))
                {
                    OnCollide(crystal);
                    return true;
                }

                return false;
            }

            ColliderGeometry previous = entity.Collider;
            entity.Collider = collider;
            bool collided = SourceEntityCollision.Check(crystal, entity);
            entity.Collider = previous;
            if (collided)
            {
                OnCollide(crystal);
                return true;
            }

            return false;
        }

        public override void DebugRender(ISourceRenderContext context, object camera)
        {
            if (Collider != null)
            {
                var entity = (SceneEntityState)Entity;
                ColliderGeometry previous = entity.Collider;
                entity.Collider = Collider;
                Collider.Render(camera, PackedColor.Green, context);
                entity.Collider = previous;
            }
        }
    }
}
