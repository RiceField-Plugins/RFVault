using System;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.LowLevel;

namespace RFVault.Core
{
    internal static class UniTaskSetup
    {
        private static bool m_Initialized { get; set; }

        internal static void CheckInit()
        {
            if (m_Initialized)
                return;
            m_Initialized = true;
            Init();
        }

        private static void Init()
        {
            // Original from https://github.com/openmod/openmod/blob/main/unityengine/OpenMod.UnityEngine/UnityHostLifetime.cs
            // Original Author: Trojaner

            if (PlayerLoopHelper.IsInjectedUniTaskPlayerLoop())
                return;
            var unitySynchronizationContextField = typeof(PlayerLoopHelper).GetField("unitySynchronizationContext",
                BindingFlags.Static | BindingFlags.NonPublic);

            unitySynchronizationContextField?.SetValue(null, SynchronizationContext.Current);

            var mainThreadIdField =
                typeof(PlayerLoopHelper).GetField("mainThreadId", BindingFlags.Static | BindingFlags.NonPublic) ??
                throw new Exception("Could not find PlayerLoopHelper.mainThreadId field");
            mainThreadIdField.SetValue(null, Thread.CurrentThread.ManagedThreadId);

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);
        }
    }
}