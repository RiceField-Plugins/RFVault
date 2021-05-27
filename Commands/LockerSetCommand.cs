using System;
using System.Collections.Generic;
using RFLocker.Enums;
using RFLocker.Models;
using RFLocker.Utils;
using Rocket.API;
using Rocket.Unturned.Player;

namespace RFLocker.Commands
{
    public class LockerSetCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "lockerset";
        public string Help => "Set player's default locker.";
        public string Syntax => "/lockerset <lockerName>";
        public List<string> Aliases => new List<string> {"lset"};
        public List<string> Permissions => new List<string> {"lockerset"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                caller.SendChat(Plugin.Inst.Translate("rflocker_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
            }

            var player = (UnturnedPlayer) caller;
            if (!CheckResponse(player, command, out var responseType))
                return;
            var locker = LockerModel.Parse(command[0]);
            if (Plugin.SelectedLockerDict.ContainsKey(player.CSteamID))
                Plugin.SelectedLockerDict[player.CSteamID] = LockerModel.Parse(command[0]);
            player.SendChat(Plugin.Inst.Translate("rflocker_command_lset_success", locker.Name), Plugin.MsgColor,
                Plugin.Conf.AnnouncerIconUrl);
        }
        
        private static bool CheckResponse(UnturnedPlayer player, string[] commands, out EResponseType responseType)
        {
            responseType = EResponseType.SUCCESS;
            LockerUtil.LockerSetChecks(player, commands, out responseType);
            switch (responseType)
            {
                case EResponseType.LOCKER_NO_PERMISSION:
                    player.SendChat(Plugin.Inst.Translate("rflocker_command_locker_no_permission"), Plugin.MsgColor,
                        Plugin.Conf.AnnouncerIconUrl);
                    return false;
                case EResponseType.LOCKER_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("rflocker_command_locker_not_found"), Plugin.MsgColor,
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