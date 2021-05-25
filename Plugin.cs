using System.Collections.Generic;
using HarmonyLib;
using RFLocker.DatabaseManagers;
using RFLocker.EventListeners;
using RFLocker.Models;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RFLocker
{
    public class Plugin : RocketPlugin<Configuration>
    {
        internal static Dictionary<CSteamID, LockerModel> SelectedLockerDict;
        internal static Dictionary<CSteamID, bool> IsAccessingLocker;
        internal static Harmony Harmony;
        public static Plugin Inst;
        public static Configuration Conf;
        public static MySqlDb DbManager;
        public static Color MsgColor;

        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"locker_blacklist_item", "[RFLocker] BLACKLIST: [ID] {0} [Name] {1}"},
            {"locker_command_invalid_parameter", "[RFLocker] Invalid parameter! Usage: {0}"},
            {"locker_command_locker_no_permission", "[RFLocker] You don't have permission to access {0} Locker!"},
            {"locker_command_locker_not_found", "[RFLocker] Locker not found!"},
            {"locker_command_locker_not_selected", "[RFLocker] Please set default Locker first! /lset <lockerName>"},
            {"locker_command_lockerset_success", "[RFLocker] Successfully set {0} Locker as default Locker!"},
            {"locker_command_trash_not_found", "[RFLocker] Trash not found!"},
            {"locker_command_locker_failed_retrieving_items", "[RFLocker] Failed in opening Locker! Try again later"}
        };

        protected override void Load()
        {
            Inst = this;
            Conf = Configuration.Instance;
            if (!Configuration.Instance.Enabled)
            {
                Logger.LogWarning($"[{Name}] RFLocker: DISABLED");
                Unload();
                return;
            }

            DbManager = new MySqlDb(Conf.DatabaseAddress, Conf.DatabasePort, Conf.DatabaseUsername,
                Conf.DatabasePassword, Conf.DatabaseName, Conf.DatabaseTableName, MySqlDb.CreateTableQuery);
            MsgColor = UnturnedChat.GetColorFromName(Conf.MessageColor, Color.green);
            SelectedLockerDict = new Dictionary<CSteamID, LockerModel>();
            IsAccessingLocker = new Dictionary<CSteamID, bool>();

            Harmony = new Harmony("RFLocker.Patches");
            Harmony.PatchAll();

            U.Events.OnPlayerConnected += PlayerEvent.OnConnected;
            U.Events.OnPlayerDisconnected += PlayerEvent.OnDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateGesture += PlayerEvent.OnGesture;
            ItemManager.onTakeItemRequested += PlayerEvent.OnTakeItem;

            Logger.LogWarning($"[{Name}] Plugin loaded successfully!");
            Logger.LogWarning($"[{Name}] {Name} v0.9.0");
            Logger.LogWarning($"[{Name}] Made with 'rice' by RiceField Plugins!");
        }

        protected override void Unload()
        {
            Inst = null;
            Conf = null;
            DbManager = null;
            SelectedLockerDict.Clear();
            IsAccessingLocker.Clear();
            Harmony.UnpatchAll("RFLocker.Patches");

            U.Events.OnPlayerConnected -= PlayerEvent.OnConnected;
            U.Events.OnPlayerDisconnected -= PlayerEvent.OnDisconnected;
            UnturnedPlayerEvents.OnPlayerUpdateGesture -= PlayerEvent.OnGesture;

            Logger.LogWarning($"[{Name}] Plugin unloaded successfully!");
        }
    }
}