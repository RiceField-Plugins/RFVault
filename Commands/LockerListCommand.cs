using System.Collections.Generic;
using System.Linq;
using RFLocker.Enums;
using RFLocker.Utils;
using Rocket.API;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace RFLocker.Commands
{
    public class LockerListCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "lockerlist";
        public string Help => "Get a list of available lockers.";
        public string Syntax => "/lockerlist";
        public List<string> Aliases => new List<string> { "llist" };
        public List<string> Permissions => new List<string> { "lockerlist" };
        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length > 1)
            {
                caller.SendChat(Plugin.Inst.Translate("rfrflocker_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) caller;
            switch (command.Length)
            {
                case 0:
                {
                    var list = "None";
                    var lockers = LockerUtil.GetAllLockers(player);
                    if (lockers.Count != 0)
                        list = string.Join(", ", (from t in lockers select $"{t.Name}").ToArray());
                    caller.SendChat(Plugin.Inst.Translate("rflocker_command_llist_success", list), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    return;
                }
                default:
                    caller.SendChat(Plugin.Inst.Translate("rfgarage_command_invalid_parameter"), Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl);
                    break;
            }
        }
    }
}