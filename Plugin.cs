using HarmonyLib;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.EventListeners;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RFVault
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Inst;
        public static Configuration Conf;
        internal static Color MsgColor;
        internal DatabaseManager Database;
        private static Harmony m_Harmony;

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {$"{EResponse.BLACKLIST}", "[RFVault] BLACKLIST: {0} ({1})"},
            {$"{EResponse.INVALID_PARAMETER}", "[RFVault] Invalid parameter! Usage: {0}"},
            {$"{EResponse.NO_PERMISSION}", "[RFVault] You don't have permission to access {0} Vault!"},
            {$"{EResponse.NO_PERMISSION}", "[RFVault] You don't have permission to access any Vault!"},
            {$"{EResponse.VAULT_NOT_FOUND}", "[RFVault] Vault not found!"},
            {$"{EResponse.VAULT_NOT_SELECTED}", "[RFVault] Please set default Vault first! /vset <vaultName> or /vault <vaultName>"},
            {$"{EResponse.VAULTS}", "[RFVault] Available Vaults: {0}"},
            {$"{EResponse.VAULTSET}", "[RFVault] Successfully set {0} Locker as default Vault!"},
            {$"{EResponse.SAME_DATABASE}", "[RFVault] You can't run migrate to the same database!"},
            {$"{EResponse.MIGRATION_START}", "[RFVault] Starting migration from {0} to {1}..."},
            {$"{EResponse.MIGRATION_FINISH}", "[RFVault] Migration finished!"},
            {$"{EResponse.DATABASE_NOT_READY}", "[RFVault] Database is not ready. Please wait..."},
        };

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            if (Conf.Enabled)
            {
                MsgColor = UnturnedChat.GetColorFromName(Conf.MessageColor, Color.green);
                Database = new DatabaseManager();
                
                m_Harmony = new Harmony("RFVault.Patches");
                m_Harmony.PatchAll();

                UnturnedPlayerEvents.OnPlayerUpdateGesture += PlayerEvent.OnGesture;
                ItemManager.onTakeItemRequested += PlayerEvent.OnTakeItem;
            }
            else
                Logger.LogError($"[{Name}] RFVault: DISABLED");

            Logger.LogWarning($"[{Name}] Plugin loaded successfully!");
            Logger.LogWarning($"[{Name}] {Name} v1.0.0");
            Logger.LogWarning($"[{Name}] Made with 'rice' by RiceField Plugins!");
        }

        protected override void Unload()
        {
            if (Conf.Enabled)
            {
                m_Harmony.UnpatchAll(m_Harmony.Id);

                UnturnedPlayerEvents.OnPlayerUpdateGesture -= PlayerEvent.OnGesture;
                ItemManager.onTakeItemRequested -= PlayerEvent.OnTakeItem;
            }

            Inst = null;
            Conf = null;

            Logger.LogWarning($"[{Name}] Plugin unloaded successfully!");
        }
    }
}