using System;
using Cysharp.Threading.Tasks;
using RFVault.DatabaseManagers;
using RFVault.Enums;
using RocketExtensions.Models;
using RocketExtensions.Plugins;

namespace RFVault.Commands
{
    [Aliases("vm")]
    [AllowedCaller(Rocket.API.AllowedCaller.Both)]
    [Permissions("vaultmigrate")]
    [CommandInfo(Syntax: "/vaultmigrate <from: mysql|litedb|json> <to: mysql|litedb|json>",
        Help: "Migrate vault database from one to another.")]
    public class VaultMigrateCommand : RocketCommand
    {
        public override async UniTask Execute(CommandContext context)
        {
            if (context.CommandRawArguments.Length != 2)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (!Enum.TryParse<EDatabase>(context.CommandRawArguments[0], true, out var from) ||
                !Enum.TryParse<EDatabase>(context.CommandRawArguments[1], true, out var to))
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.INVALID_PARAMETER.ToString(), Syntax));
                return;
            }

            if (from == to)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.SAME_DATABASE.ToString()));
                return;
            }

            if (!VaultManager.Ready)
            {
                await context.ReplyAsync(Plugin.Inst.Translate(EResponse.DATABASE_NOT_READY.ToString()));
                return;
            }

            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_START.ToString(), from, to));
            await Plugin.Inst.Database.VaultManager.MigrateAsync(from, to);
            await context.ReplyAsync(Plugin.Inst.Translate(EResponse.MIGRATION_FINISH.ToString()));
        }
    }
}