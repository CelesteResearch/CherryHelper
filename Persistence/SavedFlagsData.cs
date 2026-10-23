// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Diagnostics.Logging;
using Celeste.Everest;
using Celeste.Simulation.Runtime.Session;

namespace Celeste.Mods.CherryHelper.Persistence
{
    [RuntimeTypeAlias("Celeste.Mod.CherryHelper.SavedFlagsData")]
    public class SavedFlagsData
    {
        public string campaignId;
        public Dictionary<string, bool> savedFlags;

        public SavedFlagsData(string campaignId, Dictionary<string, bool> savedFlags)
        {
            this.campaignId = campaignId;
            this.savedFlags = savedFlags;
        }

        public SavedFlagsData()
        {
        }

        public void handleSavedFlags(ChapterCompletionSession session, ISourceEngineContext engine)
        {
            foreach (string key in savedFlags.Keys)
            {
                savedFlags.TryGetValue(key, out var value);
                session.SetFlag(key, value);
                ((IEverestLoggerProcessEnvironment)engine).Logger.Log(EverestLogLevel.Info,
                    "CherryHelper/SavedFlagsData", $"Flag {key} set to state {value}");
            }
        }
    }
}
