using Celeste.Game.Execution.Levels;
using Celeste.Mods.CherryHelper.Entities.Fields;
using Celeste.Mods.CherryHelper.Runtime.Fields;

namespace Celeste.Mods.CherryHelper.Modules
{
    [Celeste.Core.Reflection.Runtime.RuntimeTypeAlias("Celeste.Mod.CherryHelper.CherryHelper")]
    public sealed partial class CherryHelperModule
    {
        private void LoadEntityToggleFields() => EntityToggleFieldRuntime.For(SourceEngine).Load();
        private void UnloadEntityToggleFields() => EntityToggleFieldRuntime.For(SourceEngine).Dispose();

        public static void ChangeFields(string nightId, bool Enable) =>
            ChangeFields(Celeste.Game.Execution.EngineRuntime.Instance.Process, nightId, Enable);

        public static void ChangeFields(Celeste.Core.Scheduling.ISourceEngineContext engine, string nightId,
            bool Enable)
        {
            var level = engine.Frame.Scenes.CurrentScene as OriginalLevelSceneRoot;
            foreach (var field in EntityToggleField.FindById(level, nightId)) field.on = Enable;
        }
    }
}
