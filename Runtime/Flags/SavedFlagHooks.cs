// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Core.Mathematics;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Areas;
using Celeste.Game.Execution.Session;
using Celeste.Mods.CherryHelper.Modules;
using Celeste.Mods.CherryHelper.Persistence;
using Celeste.Simulation.Runtime.Session;
using Celeste.Simulation.Runtime.Session.Maps;
using Celeste.Simulation.Save.Progress;
using MonoMod.RuntimeDetour;

namespace Celeste.Mods.CherryHelper.Runtime.Flags
{
    public sealed class SavedFlagHooks : IDisposable
    {
        private delegate void ConstructorOriginal(ChapterCompletionSession self, IChapterSessionData maps,
            IChapterSessionFactory factory, ISourceVectorEnvironment vectors);

        private delegate ChapterCompletionSession RestartOriginal(OriginalSessionFactory self,
            ChapterCompletionSession session, string checkpoint, ChapterAreaProgress stats);

        private readonly SavedFlagModuleBinding binding;
        private readonly ISourceEngineContext engine;
        private readonly IChapterSessionFactory ownerFactory;
        private readonly IOriginalAreaKeyPolicy keys;
        private Hook constructor;
        private Hook restart;

        public SavedFlagHooks(CherryHelperModule module, ISourceEngineContext engine)
        {
            binding = SavedFlagModuleBinding.For(engine);
            this.engine = engine;
            ownerFactory = ((IOriginalSessionEnvironment)engine).Sessions;
            keys = ((IOriginalAreaKeyEnvironment)engine).AreaKeys;
            try
            {
                constructor = new Hook(
                    typeof(ChapterCompletionSession).GetConstructor(new[]
                    {
                        typeof(IChapterSessionData), typeof(IChapterSessionFactory), typeof(ISourceVectorEnvironment)
                    }),
                    (Action<ConstructorOriginal, ChapterCompletionSession, IChapterSessionData, IChapterSessionFactory,
                        ISourceVectorEnvironment>)Construct);
                restart = new Hook(typeof(OriginalSessionFactory).GetMethod(nameof(OriginalSessionFactory.Restart)),
                    (Func<RestartOriginal, OriginalSessionFactory, ChapterCompletionSession, string, ChapterAreaProgress
                        , ChapterCompletionSession>)Restart);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        private void Construct(ConstructorOriginal original, ChapterCompletionSession self, IChapterSessionData maps,
            IChapterSessionFactory factory, ISourceVectorEnvironment vectors)
        {
            original(self, maps, factory, vectors);
            if (ReferenceEquals(factory, ownerFactory)) Restore(self);
        }

        private ChapterCompletionSession Restart(RestartOriginal original, OriginalSessionFactory self,
            ChapterCompletionSession session, string checkpoint, ChapterAreaProgress stats)
        {
            var next = original(self, session, checkpoint, stats);
            if (ReferenceEquals(self, ownerFactory)) Restore(next);
            return next;
        }

        private void Restore(ChapterCompletionSession session)
        {
            if (binding.Module.SaveData == null) return;
            bool found = false;
            foreach (var data in binding.Module.SaveData.allSavedFlagsData)
            {
                if (keys.GetLevelSet(keys.GetSessionArea(session)) == data.campaignId)
                {
                    data.handleSavedFlags(session, engine);
                    found = true;
                }
            }

            if (!found)
                binding.Module.SaveData.allSavedFlagsData.Add(new SavedFlagsData(
                    keys.GetLevelSet(keys.GetSessionArea(session)),
                    new Dictionary<string, bool>()));
        }

        public void Dispose()
        {
            restart?.Dispose();
            restart = null;
            constructor?.Dispose();
            constructor = null;
        }
    }
}
