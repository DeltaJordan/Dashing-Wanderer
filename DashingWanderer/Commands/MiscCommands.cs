using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DashingWanderer.Commands
{
    public class MiscCommands
    {
        [Command("eval"), RequireOwner]
        public async Task Eval2(CommandContext ctx, [RemainingText] string command)
        {
            command = string.Join("\n", command.Split('\n').Skip(1).Take(command.Split('\n').Skip(1).Count() - 1));

            Console.WriteLine(command);

            ScriptRunner<object> script = CSharpScript.Create(command, ScriptOptions.Default
                .WithReferences(typeof(object).GetTypeInfo().Assembly, typeof(Enumerable).GetTypeInfo().Assembly,
                                typeof(PropertyInfo).GetTypeInfo().Assembly, typeof(Decoder).GetTypeInfo().Assembly,
                                typeof(Regex).GetTypeInfo().Assembly, typeof(Task).GetTypeInfo().Assembly, typeof(CommandContext).GetTypeInfo().Assembly,
                                typeof(Command).GetTypeInfo().Assembly, typeof(DiscordMessage).GetTypeInfo().Assembly)
                .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Reflection", "System.Text",
                             "System.Text.RegularExpressions", "System.Threading.Tasks", "DSharpPlus.CommandsNext",
                             "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Entities", "DSharpPlus"), typeof(Globals))
                .CreateDelegate();

            object result;

            try
            {
                result = await script(new Globals { Ctx = ctx });
            }
            catch (Exception e)
            {
                DiscordEmbedBuilder errorbuilder = new DiscordEmbedBuilder();
                errorbuilder.WithTitle("Exception occured.");
                errorbuilder.AddField("Input", $"```cs\n{command}\n```");
                errorbuilder.AddField("Output", $"```\n[Exception ({(e.InnerException ?? e).GetType().Name})] {e.InnerException?.Message ?? e.Message}\n```");
                errorbuilder.WithColor(DiscordColor.Red);

                DiscordMessage errorMessage = await ctx.RespondAsync(null, false, errorbuilder.Build());

                await Task.Delay(TimeSpan.FromMinutes(1));

                await errorMessage.DeleteAsync();

                return;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.AddField("Input", $"```cs\n{command}\n```");
            builder.AddField("Output", $"```\n{result.ToString()}\n```");
            builder.WithColor(DiscordColor.Green);

            DiscordMessage resultMessage = await ctx.RespondAsync(null, false, builder.Build());

            await resultMessage.DeleteAsync();
        }

        public async Task TCPI(CommandContext ctx)
        {
        }
    }

    public class Globals
    {
        public CommandContext Ctx { get; set; }
    }
}
