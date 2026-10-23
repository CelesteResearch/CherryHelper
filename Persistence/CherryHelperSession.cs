using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Modules.Persistence;

namespace Celeste.Mods.CherryHelper.Persistence
{
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.CherryHelperSession")]
    public class CherryHelperSession : EverestModuleSession
    {
        public bool HasShadowDash { get; set; }
        public bool ShadowDashActive { get; set; }
        public bool SafeFromRot { get; set; }
    }
}
