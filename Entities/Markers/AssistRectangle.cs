// SPDX-License-Identifier: MIT

using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Display.Colors;
using Celeste.Graphics.Rendering;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;

namespace Celeste.Mods.CherryHelper.Entities.Markers
{
    [CustomEntity("CherryHelper/AssistRect")]
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.AssistRectangle")]
    public sealed class AssistRectangle : SceneEntityState
    {
        private readonly ISourceVectorArithmetic vectors;
        public PackedColor Color = PackedColor.White;
        public float SecretAlpha = .25f;

        public static AssistRectangle Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset, OriginalLevelEntityData data)
        {
            return new AssistRectangle(data, offset, level.EngineContext);
        }

        public AssistRectangle(EntityData data, Vector2 offset, ISourceEngineContext engine)
            : this(data.Position + offset, data.Width, data.Height, SourceHexColor.HexToColor(data.Attr("color")),
                engine)
        {
        }

        public AssistRectangle(Vector2 position, int width, int height, PackedColor color, ISourceEngineContext engine)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            vectors = ((ISourceVectorEnvironment)engine).VectorArithmetic;
            Collider = new HitboxColliderGeometry(width, height);
            Color = color;
        }

        public override void Render(ISourceRenderContext context)
        {
            var left = (int)Left;
            var right = (int)Right;
            var top = (int)Top;
            var bottom = (int)Bottom;
            SourcePrimitiveDrawing.Rect(context, this, left + 4, top + 4, Width - 8f, Height - 8f, Color * SecretAlpha);
            for (float x = left; x < right - 3; x += 3f)
            {
                SourcePrimitiveDrawing.Line(context, this, new Vector2(x, top), new Vector2(x + 2f, top), Color,
                    vectors);
                SourcePrimitiveDrawing.Line(context, this, new Vector2(x, bottom - 1), new Vector2(x + 2f, bottom - 1),
                    Color, vectors);
            }

            for (float y = top; y < bottom - 3; y += 3f)
            {
                SourcePrimitiveDrawing.Line(context, this, new Vector2(left + 1, y), new Vector2(left + 1, y + 2f),
                    Color, vectors);
                SourcePrimitiveDrawing.Line(context, this, new Vector2(right, y), new Vector2(right, y + 2f), Color,
                    vectors);
            }

            SourcePrimitiveDrawing.Rect(context, this, left + 1, top, 1f, 2f, Color);
            SourcePrimitiveDrawing.Rect(context, this, right - 2, top, 2f, 2f, Color);
            SourcePrimitiveDrawing.Rect(context, this, left, bottom - 2, 2f, 2f, Color);
            SourcePrimitiveDrawing.Rect(context, this, right - 2, bottom - 2, 2f, 2f, Color);
            base.Render(context);
        }
    }
}
