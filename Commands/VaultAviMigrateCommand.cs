using System;
using System.Threading.Tasks;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFVault.Commands
{
    [CommandAliases("vam")]
    [CommandActor(Rocket.API.AllowedCaller.Both)]
    [CommandPermissions("vaultavimigrate")]
    [CommandInfo("Migrate vault database from avi vault to current rfvault database.",
        "/vaultavimigrate <aviVaultMySQLConnectionString> <aviVaultMySQLTableName>",
        AllowSimultaneousCalls = false)]
    public class VaultAviMigrateCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 3)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!string.IsNullOrWhiteSpace(context.CommandRawArguments[0]) ||
                !string.IsNullOrWhiteSpace(context.CommandRawArguments[1]))
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!VaultManager.Ready)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.DATABASE_NOT_READY.ToString()));
                return;
            }

            await context.ReplyAsync(
                RFVault.Plugin.Inst.Translate(EResponse.MIGRATION_START.ToString(), "AviVault", RFVault.Plugin.Conf.Database));
            await VaultManager.AviMigrateAsync(context.CommandRawArguments[0], context.CommandRawArguments[1]);
            await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.MIGRATION_FINISH.ToString()));
        }
    }
}