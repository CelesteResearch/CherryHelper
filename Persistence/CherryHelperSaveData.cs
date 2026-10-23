// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Modules.Persistence;

namespace Celeste.Mods.CherryHelper.Persistence
{
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.CherryHelperSaveData")]
    public class CherryHelperSaveData : EverestModuleSaveData
    {
        public List<SavedFlagsData> allSavedFlagsData { get; internal set; } = new List<SavedFlagsData>();
    }
}
