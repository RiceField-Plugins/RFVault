using System;
using System.Threading.Tasks;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFVault.Commands
{
    [CommandAliases("vm")]
    [CommandActor(Rocket.API.AllowedCaller.Both)]
    [CommandPermissions("vaultmigrate")]
    [CommandInfo("Migrate vault database from one to another.", "/vaultmigrate <from: mysql|litedb|json> <to: mysql|litedb|json>", AllowSimultaneousCalls = false)]
    public class VaultMigrateCommand : RocketCommand
    {
        public override async Task Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var from) ||
                !Enum.TryParse<EDatabase>(context.CommandRawArguments[1], true, out var to))
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (from == to)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.SAME_DATABASE.ToString()));
                return;
            }

            if (!VaultManager.Ready)
            {
                await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.DATABASE_NOT_READY.ToString()));
                return;
            }

            await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.MIGRATION_START.ToString(), from, to));
            await VaultManager.MigrateAsync(from, to);
            await context.ReplyAsync(RFVault.Plugin.Inst.Translate(EResponse.MIGRATION_FINISH.ToString()));
        }
    }
}