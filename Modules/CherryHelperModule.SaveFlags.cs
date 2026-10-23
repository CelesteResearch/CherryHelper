// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using Celeste.Game.Execution.Areas;
using Celeste.Mods.CherryHelper.Persistence;
using Celeste.Mods.CherryHelper.Runtime.Flags;
using Celeste.Simulation.Runtime.Session;

namespace Celeste.Mods.CherryHelper.Modules
{
    public sealed partial class CherryHelperModule
    {
        private SavedFlagHooks savedFlags;
        public override Type SaveDataType => typeof(CherryHelperSaveData);
        public CherryHelperSaveData SaveData => (CherryHelperSaveData)_SaveData;
        internal void LoadSavedFlags() => savedFlags = new SavedFlagHooks(this, SourceEngine);

        internal void UnloadSavedFlags()
        {
            savedFlags.Dispose();
            savedFlags = null;
        }

        public void SaveFlag(ChapterCompletionSession session, string flag, bool value)
        {
            var keys = ((IOriginalAreaKeyEnvironment)SourceEngine).AreaKeys;
            bool found = false;
            foreach (var data in SaveData.allSavedFlagsData)
            {
                if (keys.GetLevelSet(keys.GetSessionArea(session)) == data.campaignId)
                {
                    if (data.savedFlags.ContainsKey(flag)) data.savedFlags.Remove(flag);
                    data.savedFlags.Add(flag, value);
                    found = true;
                }
            }

            if (!found)
            {
                var data = new SavedFlagsData(keys.GetLevelSet(keys.GetSessionArea(session)),
                    new Dictionary<string, bool>());
                SaveData.allSavedFlagsData.Add(data);
                data.savedFlags.Add(flag, value);
            }
        }
    }
}
