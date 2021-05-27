using System;
using System.Collections.Generic;
using RFLocker.Enums;
using RFLocker.Models;
using RFLocker.Utils;
using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;

namespace RFLocker.Commands
{
    public class LockerCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "locker";
        public string Help => "Open a virtual locker storage.";
        public string Syntax => "/locker | /locker <lockerName>";
        public List<string> Aliases => new List<string> {"vault"};
        public List<string> Permissions => new List<string> {"locker"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 1)
            {
                caller.SendChat(Plugin.Inst.Translate("rflocker_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (command.Length == 0)
            {
                if (!CheckResponse(player, command, out var responseType))
                    return;
                LockerUtil.OpenVirtualLocker(player, Plugin.SelectedLockerDict[player.CSteamID]);
                if (Plugin.Conf.EnableLogs)
                    Logger.LogWarning(
                        $"[RFLocker] {player.CharacterName} is accessing {Plugin.SelectedLockerDict[player.CSteamID].Name} Locker");
            }

            if (command.Length == 1)
            {
                if (!CheckResponse(player, command, out var responseType))
                    return;
                var locker = LockerModel.Parse(command[0]);
                LockerUtil.OpenVirtualLocker(player, locker);
                if (Plugin.Conf.EnableLogs)
                    Logger.LogWarning(
                        $"[RFLocker] {player.CharacterName} is accessing {locker.Name} Locker");
            }
        }

        private static bool CheckResponse(UnturnedPlayer player, string[] commands, out EResponseType responseType)
        {
            responseType = EResponseType.SUCCESS;
            LockerUtil.LockerChecks(player, commands, out responseType);
            switch (responseType)
            {
                case EResponseType.LOCKER_NO_PERMISSION:
                    player.SendChat(
                        Plugin.Inst.Translate("rflocker_command_locker_no_permission",
                            LockerModel.Parse(commands[0])), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.LOCKER_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("rflocker_command_locker_not_found"), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.LOCKER_NOT_SELECTED:
                    player.SendChat(Plugin.Inst.Translate("rfrflocker_command_locker_not_selected"), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.SUCCESS:
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(responseType), responseType, null);
            }
        }
    }
}