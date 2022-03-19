using RFRocketLibrary.Enum;
using RFRocketLibrary.Events;
using RFRocketLibrary.Utils;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RFVault.EventListeners;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RFVault
{
    public class Plugin : RocketPlugin<Configuration>
    {
        private const int Major = 1;
        private const int Minor = 1;
        private const int Patch = 6;
        
        public static Plugin Inst;
        public static Configuration Conf;
        internal static Color MsgColor;

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            if (Conf.Enabled)
            {
                MsgColor = UnturnedChat.GetColorFromName(Conf.MessageColor, Color.green);
                
                if (DependencyUtil.CanBeLoaded(EDependency.Harmony))
                {
                    DependencyUtil.Load(EDependency.Harmony);
                }
                if (DependencyUtil.CanBeLoaded(EDependency.NewtonsoftJson))
                {
                    DependencyUtil.Load(EDependency.NewtonsoftJson);
                    DependencyUtil.Load(EDependency.SystemRuntimeSerialization);
                }
                if (DependencyUtil.CanBeLoaded(EDependency.LiteDB))
                {
                    DependencyUtil.Load(EDependency.LiteDB);
                    DependencyUtil.Load(EDependency.LiteDBAsync);
                }
                if (DependencyUtil.CanBeLoaded(EDependency.Dapper))
                {
                    DependencyUtil.Load(EDependency.Dapper);
                    DependencyUtil.Load(EDependency.MySqlData);
                    DependencyUtil.Load(EDependency.SystemManagement);
                    DependencyUtil.Load(EDependency.UbietyDnsCore);
                }
                
                DatabaseManager.Init();
                VaultVersionManager.Initialize();
                VaultManager.Initialize();
                
                //Load RFRocketLibrary Events
                EventBus.Load();
                
                // UnturnedEvent.OnPlayerChangedEquipment += PlayerEvent.OnEquipmentChanged;
                // UnturnedEvent.OnPlayerChangedGesture += PlayerEvent.OnGestureChanged;
                UnturnedEvent.OnPrePlayerTookItem += PlayerEvent.OnPreItemTook;
                UnturnedEvent.OnPrePlayerDraggedItem += PlayerEvent.OnPreItemDragged;
                UnturnedEvent.OnPrePlayerSwappedItem += PlayerEvent.OnPreItemSwapped;
            }
            else
                Logger.LogError($"[{Name}] Plugin: DISABLED");

            Logger.LogWarning($"[{Name}] Plugin loaded successfully!");
            Logger.LogWarning($"[{Name}] {Name} v{Major}.{Minor}.{Patch}");
            Logger.LogWarning($"[{Name}] Made with 'rice' by RiceField Plugins!");
        }

        protected override void Unload()
        {
            if (Conf.Enabled)
            {
                // UnturnedEvent.OnPlayerChangedEquipment -= PlayerEvent.OnEquipmentChanged;
                // UnturnedEvent.OnPlayerChangedGesture -= PlayerEvent.OnGestureChanged;
                UnturnedEvent.OnPrePlayerTookItem -= PlayerEvent.OnPreItemTook;
                UnturnedEvent.OnPrePlayerDraggedItem -= PlayerEvent.OnPreItemDragged;
                UnturnedEvent.OnPrePlayerSwappedItem -= PlayerEvent.OnPreItemSwapped;
            }

            Inst = null;
            Conf = null;

            Logger.LogWarning($"[{Name}] Plugin unloaded successfully!");
        }

        public override TranslationList DefaultTranslations => new()
        {
            {$"{EResponse.BLACKLIST}", "[RFVault] BLACKLIST: {0} ({1})"},
            {$"{EResponse.INVALID_PARAMETER}", "[RFVault] Invalid parameter! Usage: {0}"},
            {$"{EResponse.IN_VEHICLE}", "[RFVault] Accessing Vault while in a vehicle is not allowed!"},
            {$"{EResponse.NO_PERMISSION}", "[RFVault] You don't have permission to access {0} Vault!"},
            {$"{EResponse.NO_PERMISSION_ALL}", "[RFVault] You don't have permission to access any Vault!"},
            {$"{EResponse.VAULT_NOT_FOUND}", "[RFVault] Vault not found!"},
            {$"{EResponse.VAULT_NOT_SELECTED}", "[RFVault] Please set default Vault first! /vset <vaultName> or /vault <vaultName>"},
            {$"{EResponse.VAULT_PROCESSING}", "[RFVault] Processing vault. Please wait..."},
            {$"{EResponse.VAULTS}", "[RFVault] Available Vaults: {0}"},
            {$"{EResponse.VAULTSET}", "[RFVault] Successfully set {0} Vault as default Vault!"},
            {$"{EResponse.SAME_DATABASE}", "[RFVault] You can't run migrate to the same database!"},
            {$"{EResponse.MIGRATION_START}", "[RFVault] Starting migration from {0} to {1}..."},
            {$"{EResponse.MIGRATION_FINISH}", "[RFVault] Migration finished!"},
            {$"{EResponse.DATABASE_NOT_READY}", "[RFVault] Database is not ready. Please wait..."},
            {$"{EResponse.PLAYER_VAULT_NOT_FOUND}", "[RFVault] {0} doesn't have {1} Vault!"},
            {$"{EResponse.ADMIN_VAULT_CLEAR}", "[RFVault] Successfully cleared {0}'s {1} Vault"},
            {$"{EResponse.VAULT_CLEAR}", "[RFVault] Successfully cleared {0} Vault!"},
            {$"{EResponse.VAULT_BUSY}", "[RFVault] Someone is using this vault! Please wait until they are finished!"},
            {$"{EResponse.PLAYER_NOT_FOUND}", "[RFVault] Can't find player under name {0}!"},
        };
    }
}