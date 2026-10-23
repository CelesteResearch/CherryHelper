// SPDX-License-Identifier: MIT

using System;
using System.Linq;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Runtime;
using Celeste.Mods.CherryHelper.Persistence;
using Celeste.Mods.CherryHelper.Runtime.ShadowDash;
using Celeste.Mods.CherryHelper.Runtime.Flags;

namespace Celeste.Mods.CherryHelper.Modules
{
    public sealed partial class CherryHelperModule : SourceEverestModule
    {
        private ShadowDashProcess shadowDash;
        public override Type SessionType => typeof(CherryHelperSession);
        public CherryHelperSession Session => (CherryHelperSession)_Session;

        public CherryHelperModule(ISourceEngineContext engine) : base(engine)
        {
            SavedFlagModuleBinding.Bind(engine, this);
        }

        internal static CherryHelperModule For(ISourceEngineContext engine) =>
            ((ISourceEverestModulesEnvironment)engine).Modules.OfType<CherryHelperModule>().Single();

        public override void Load()
        {
            shadowDash = new ShadowDashProcess(SourceEngine, this);
            LoadSavedFlags();
            LoadEntityToggleFields();
        }

        public override void Unload()
        {
            shadowDash.Dispose();
            shadowDash = null;
            UnloadSavedFlags();
            UnloadEntityToggleFields();
        }
    }
}
