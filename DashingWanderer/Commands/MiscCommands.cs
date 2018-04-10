using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DashingWanderer.Data;
using DashingWanderer.Data.Explorers.Items;
using DashingWanderer.Data.Explorers.Moves;
using DashingWanderer.Data.Explorers.Pokedex;
using DashingWanderer.Network;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PokeAPI;
using PortraitsAdder;

namespace DashingWanderer.Commands
{
    public class MiscCommands : BaseCommandModule
    {
        [Command("random")]
        public async Task Random(CommandContext ctx)
        {
            string pokedexJson = Path.Combine(DashingWanderer.Globals.AppPath, "maingamepokedex.json");

            JObject pokedexArray = JObject.Parse(File.ReadAllText(pokedexJson));

            List<KeyValuePair<string, JToken>> allPokedexTokens = new List<KeyValuePair<string, JToken>>();

            foreach (KeyValuePair<string, JToken> keyValuePair in pokedexArray)
            {
                allPokedexTokens.Add(keyValuePair);
            }

            string pokeName = allPokedexTokens[DashingWanderer.Globals.Random.Next(allPokedexTokens.Count - 1)].Key;

            if (!NetworkFileHelper.RemoteFileExists($"https://play.pokemonshowdown.com/sprites/xyani/{pokeName}.gif"))
            {
                await this.Random(ctx);
                return;
            }

            using (WebClient client = new WebClient())
            using (MemoryStream stream = new MemoryStream())
            {
                byte[] gifBytes = client.DownloadData(new Uri($"https://play.pokemonshowdown.com/sprites/xyani/{pokeName}.gif"));

                await stream.WriteAsync(gifBytes, 0, gifBytes.Length);

                stream.Position = 0;

                await ctx.RespondWithFileAsync($"{pokeName}.gif", stream);
            }
        }

        [Command("eval"), RequireOwner, Hidden]
        public async Task Eval2(CommandContext ctx, [RemainingText] string command)
        {
            command = string.Join("\n", command.Split('\n').Skip(1).Take(command.Split('\n').Skip(1).Count() - 1));

            Console.WriteLine(command);

            ScriptRunner<object> script = CSharpScript.Create(command, ScriptOptions.Default
                .WithReferences(typeof(object).GetTypeInfo().Assembly, typeof(Enumerable).GetTypeInfo().Assembly,
                                typeof(PropertyInfo).GetTypeInfo().Assembly, typeof(Decoder).GetTypeInfo().Assembly,
                                typeof(Regex).GetTypeInfo().Assembly, typeof(Task).GetTypeInfo().Assembly, typeof(CommandContext).GetTypeInfo().Assembly,
                                typeof(Command).GetTypeInfo().Assembly, typeof(DiscordMessage).GetTypeInfo().Assembly,
                                typeof(ExplorersItem).GetTypeInfo().Assembly)
                .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Reflection", "System.Text",
                             "System.Text.RegularExpressions", "System.Threading.Tasks", "DSharpPlus.CommandsNext",
                             "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Entities", "DSharpPlus", "DashingWanderer.Data.Explorers.Items", 
                             "DashingWanderer.Data.Explorers.Moves", "DashingWanderer.Data.Explorers.Pokedex"), typeof(Globals))
                .CreateDelegate();                                                                       
            object result;

            try
            {
                result = await script(new Globals
                {
                    Ctx = ctx,
                    ExplorersPokemon = DataBuilder.ExplorersPokemon,
                    ExplorersIQSkills = DataBuilder.ExplorersIQSkills,
                    ExplorersItems = DataBuilder.ExplorersItems,
                    ExplorersMoves = DataBuilder.ExplorersMoves,
                    ExplorersPortraits = DataBuilder.ExplorersPortraits
                });
            }
            catch (Exception e)
            {
                DiscordEmbedBuilder errorbuilder = new DiscordEmbedBuilder();
                errorbuilder.WithTitle("Exception occurred.");
                errorbuilder.AddField("Input", $"```cs\n{command}\n```");
                errorbuilder.AddField("Output", $"```\n[Exception ({(e.InnerException ?? e).GetType().Name})] {e.InnerException?.Message ?? e.Message}\n```");
                errorbuilder.WithColor(DiscordColor.Red);

                await ctx.RespondAsync(null, false, errorbuilder.Build());

                return;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.AddField("Input", $"```cs\n{command}\n```");
            builder.AddField("Output", $"```\n{result.ToString()}\n```");
            builder.WithColor(DiscordColor.Green);

            await ctx.RespondAsync(null, false, builder.Build());
        }

        public async Task TCPI(CommandContext ctx)
        {
        }
    }

    public class Globals
    {
        public CommandContext Ctx { get; set; }
        public List<PortraitEntity> ExplorersPortraits { get; set; }
        public List<ExplorersEnitity> ExplorersPokemon { get; set; }
        public List<ExplorersItem> ExplorersItems { get; set; }
        public List<ExplorersMove> ExplorersMoves { get; set; }
        public List<IQSkill> ExplorersIQSkills { get; set; }
    }
}
