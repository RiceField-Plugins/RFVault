using System;
using System.Collections.Generic;
using RFLocker.Enums;
using RFLocker.Utils;
using Rocket.API;
using Rocket.Unturned.Player;

namespace RFLocker.Commands
{
    public class TrashCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "trash";
        public string Help => "Open a trash storage.";
        public string Syntax => "/trash";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> {"trash"};
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 1)
            {
                caller.SendChat(Plugin.Inst.Translate("locker_command_invalid_parameter", Syntax), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            if (command.Length == 0)
            {
                if (!CheckResponse(player,  out var responseType))
                    return;
                LockerUtil.OpenVirtualTrash(player);
            }
        }
        
        private static bool CheckResponse(UnturnedPlayer player, out EResponseType responseType)
        {
            responseType = EResponseType.SUCCESS;
            if (Plugin.Conf.Trash == null)
                responseType = EResponseType.TRASH_NOT_FOUND;
            switch (responseType)
            {
                case EResponseType.TRASH_NOT_FOUND:
                    player.SendChat(Plugin.Inst.Translate("locker_command_trash_not_found"), Plugin.MsgColor,
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