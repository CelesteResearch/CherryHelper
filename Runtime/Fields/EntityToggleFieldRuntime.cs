using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Runtime;
using Celeste.Everest.Modules.Updating;
using Celeste.Mods.CherryHelper.Entities.Fields;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.CherryHelper.Runtime.Fields
{
    internal sealed class EntityToggleFieldRuntime : IDisposable
    {
        private static readonly ConditionalWeakTable<ISourceEngineContext, EntityToggleFieldRuntime> Instances = new();
        private readonly ISourceEngineContext engine;
        private readonly HashSet<EntityToggleField> fields = new();
        private readonly List<IDisposable> hooks = new();

        private EntityToggleFieldRuntime(ISourceEngineContext engine)
        {
            this.engine = engine;
        }

        internal static EntityToggleFieldRuntime For(ISourceEngineContext engine) =>
            Instances.GetValue(engine, key => new(key));

        internal void Add(EntityToggleField field) => fields.Add(field);
        internal void Remove(EntityToggleField field) => fields.Remove(field);

        internal void Load()
        {
            if (hooks.Count != 0) return;
            try
            {
                hooks.Add(new Hook(
                    typeof(EverestLoaderProcess).GetMethod("ProcessAssembly",
                        new[] { typeof(EverestModuleMetadata), typeof(Assembly), typeof(Type[]) }),
                    (Action<Action<EverestLoaderProcess, EverestModuleMetadata, Assembly, Type[]>, EverestLoaderProcess,
                        EverestModuleMetadata, Assembly, Type[]>)Register));
                hooks.Add(new Hook(typeof(EverestLoaderProcess).GetMethod("ReleaseAssemblyRegistrations"),
                    (Action<Action<EverestLoaderProcess, Assembly>, EverestLoaderProcess, Assembly>)Release));
                Refresh();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void Register(Action<EverestLoaderProcess, EverestModuleMetadata, Assembly, Type[]> original,
            EverestLoaderProcess loader, EverestModuleMetadata metadata, Assembly assembly, Type[] types)
        {
            original(loader, metadata, assembly, types);
            if (ReferenceEquals(loader, ((IEverestLoaderProcessEnvironment)engine).Loader)) Refresh();
        }

        private void Release(Action<EverestLoaderProcess, Assembly> original, EverestLoaderProcess loader,
            Assembly assembly)
        {
            original(loader, assembly);
            if (ReferenceEquals(loader, ((IEverestLoaderProcessEnvironment)engine).Loader)) Refresh();
        }

        private void Refresh()
        {
            foreach (var field in fields) field.ResolveTypes();
        }

        public void Dispose()
        {
            for (int i = hooks.Count - 1; i >= 0; i--) hooks[i].Dispose();
            hooks.Clear();
        }
    }
}
