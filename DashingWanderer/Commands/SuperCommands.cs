using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;

namespace DashingWanderer.Commands
{
    [Group("super"),
     Aliases("s"),
     Description("Targets the commands to the Pokemon Super Mystery Dungeon game.")]
    public class SuperCommands : BaseCommandModule
    {
        [Description("Displays the requested Pokémon's portrait.")]
        [Command("portrait")]
        public async Task Portrait(CommandContext ctx,
            [Description("Pokemon name. Any Pokémon that have \"'\", spaces, or \".\" will have to be removed. e.g Mr. Mime => MrMime.")]
            string poke,
            [Description("Optionally choose the portrait number (Note: C# indexes start with 0)\nSome Pokémon have only three portraits so also keep that in mind.")]
            int index = 0)
        {
            string indexText = index.ToWords();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(DashingWanderer.Globals.AppPath, "Portraits.dat"));
                using (MemoryStream ms =
                    new MemoryStream(Convert.FromBase64String(doc.SelectNodes($"//{poke}/{indexText}")[0]
                        .InnerText)))
                {
                    await ctx.Channel.SendFileAsync(ms, $"{poke}.png");
                }
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync($"Invalid entry! Remember, Poke is case sensitive.\n{ex.Message}");
            }
        }
    }
}
