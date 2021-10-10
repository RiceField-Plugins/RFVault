using Cysharp.Threading.Tasks;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities.ShimmyMySherbet.Extensions;

namespace RFVault.Commands
{
    [AllowedCaller(Rocket.API.AllowedCaller.Player)]
    [CommandName("trash")]
    [Permissions("trash")]
    [CommandInfo("Open a trash storage.", "/trash")]
    public class TrashCommand : RocketCommand
    {
        public override async UniTask Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await ThreadTool.RunOnGameThreadAsync(() => ChatHelper.Say(context.Player,
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