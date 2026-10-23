// SPDX-License-Identifier: MIT

using System.Runtime.CompilerServices;
using Celeste.Core.Scheduling;
using Celeste.Mods.CherryHelper.Modules;

namespace Celeste.Mods.CherryHelper.Runtime.Flags
{
    internal sealed class SavedFlagModuleBinding
    {
        private static readonly ConditionalWeakTable<ISourceEngineContext, SavedFlagModuleBinding> owners =
            new ConditionalWeakTable<ISourceEngineContext, SavedFlagModuleBinding>();

        public CherryHelperModule Module;

        public static SavedFlagModuleBinding For(ISourceEngineContext engine) =>
            owners.GetValue(engine, _ => new SavedFlagModuleBinding());

        public static void Bind(ISourceEngineContext engine, CherryHelperModule module) => For(engine).Module = module;
    }
}
