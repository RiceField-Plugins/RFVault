using System.Threading.Tasks;
using RFVault.Enums;
using RFVault.Helpers;
using RFVault.Utils;
using Rocket.Core.Logging;
using Rocket.Unturned.Player;
using RocketExtensions.Models;
using RocketExtensions.Plugins;
using RocketExtensions.Utilities;
using SDG.Unturned;

namespace RFVault.Commands
{
    [CommandActor(Rocket.API.AllowedCaller.Player)]
    [CommandPermissions("trash")]
    [CommandInfo("Open a trash storage.", "/trash")]
    public class TrashCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 0)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax),
                    RFVault.Plugin.MsgColor, RFVault.Plugin.Conf.AnnouncerIconUrl);
                return;
            }

            var player = (UnturnedPlayer) context.Player;
            if (RFVault.Plugin.Conf.DebugMode)
                Logger.LogWarning($"[RFVault] [DEBUG] {player.CharacterName} is accessing Trash");
            
            await ThreadTool.RunOnGameThreadAsync(() => VaultUtil.OpenVirtualTrash(player));
        }
    }
}