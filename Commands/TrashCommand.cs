using System.Threading.Tasks;
using RFRocketLibrary.Plugins;
using RFRocketLibrary.Utils;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("trash")]
    [Permissions("trash")]
    [CommandInfo("Open a trash storage.", "/trash")]
    public class TrashCommand : RocketCommand
    {
        public override async Task ExecuteAsync(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await ThreadUtil.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
                    Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    Plugin.MsgColor, Plugin.Conf.AnnouncerIconUrl));
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            if (Plugin.Conf.DebugMode)
                Logger.LogWarning($"[RFVault] [DEBUG] {player.CharacterName} is accessing Trash");
            await VaultUtil.OpenVirtualTrashAsync(player);
        }
    }
}