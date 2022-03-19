#if DEBUG
using System;
using Cysharp.Threading.Tasks;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFVault.Commands
{
    [Aliases("vml")]
    [AllowedCaller(Rocket.API.AllowedCaller.Both)]
    [Permissions("vaultmigratelocker")]
    [CommandInfo(Syntax: "/vaultmigratelocker <to: mysql|litedb|json>",
        Help: "Migrate locker database from RFLocker to RFVault.")]
    public class VaultMigrateLockerCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 1)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var to))
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!VaultManager.Ready)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.DATABASE_NOT_READY.ToString()));
                return;
            }

            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_START.ToString(), "RFLocker", to));
            await VaultManager.MigrateLockerAsync(to);
            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_FINISH.ToString()));
        }
    }
}
#endif