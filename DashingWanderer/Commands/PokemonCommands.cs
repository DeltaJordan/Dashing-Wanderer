using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DashingWanderer.Algorithms;
using DashingWanderer.Data;
using DashingWanderer.Data.Explorers.Enums;
using DashingWanderer.Data.Explorers.Items;
using DashingWanderer.Data.Explorers.Pokedex;
using DashingWanderer.Data.Explorers.Pokedex.Enums;
using DashingWanderer.Exceptions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PortraitsAdder;
using RawDataPMDExplorers.Data.Moves;

namespace DashingWanderer.Commands
{
    [Group("explorers"), 
     Aliases("e"), 
     Description("Targets the commands to the Pokemon Mystery Dungeon: Explorers of Time/Darkness/Sky games. " +
                 "Please note that all data is pulled directly from the game and may be incorrect." +
                 "\nCredits to https://projectpokemon.org/home/forums/topic/31407-pokemon-mystery-dungeon-2-psy_commandos-tools-and-research-notes/ " +
                 "for all the amazing documentation and research put into this game for the data files.")]
    public class PokemonCommands
    {
        [Command("dex"), Description("Retrieves info about the requested Pokemon.")]
        public async Task Dex(CommandContext ctx, [Description("Requested Pokemon name or Dex Id."), RemainingText] string pokemon)
        {
            if (string.IsNullOrWhiteSpace(pokemon))
            {
                await ctx.RespondAsync("Invalid entry.");
                return;
            }

            pokemon = pokemon.ToLower();

            ExplorersEnitity explorersPokemon;

            if (int.TryParse(pokemon, out int result))
            {
                explorersPokemon = DataBuilder.ExplorersPokemon.FirstOrDefault(e => e.DexId == result);
            }
            else
            {
                explorersPokemon = DataBuilder.ExplorersPokemon.FirstOrDefault(e => e.Name.ToLower() == pokemon || e.Id == pokemon);
            }

            if (explorersPokemon == null)
            {
                IEnumerable<string> similarPokemon = DataBuilder.ExplorersPokemon
                    .GroupBy(e =>
                    {
                        int idComparasion = LevenshteinDistance.Compute(e.Id, pokemon);
                        int nameComparasion = LevenshteinDistance.Compute(e.Name, pokemon);
                        return idComparasion < nameComparasion ? idComparasion : nameComparasion;
                    })
                    .OrderBy(e => e.Key)
                    .First()
                    .Select(e => e.Name);

                await ctx.RespondAsync($"Pokemon not found. Did you mean any of the following: `{string.Join("`, `", similarPokemon)}`?");
                return;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            builder.AddField("Types", $"{explorersPokemon.PrimaryType}{(explorersPokemon.SecondaryType == TypeEnum.PokemonType.None ? "" : $", {explorersPokemon.SecondaryType}")}", true);
            builder.AddField("Abilities", $"{explorersPokemon.GenderEnitities.First().PrimaryAbility}{(explorersPokemon.GenderEnitities.First().SecondaryAbility == AbilityEnum.Ability.None ? "" : $", {explorersPokemon.GenderEnitities.First().SecondaryAbility}")}".Replace("_", " "), true);

            List<ExplorersEnitity> preEvos = DataBuilder.ExplorersPokemon.Where(e => explorersPokemon.GenderEnitities.Any(f => f.Evolution.PreEvoRawId == e.RawIndex)).ToList();
            string prepreEvoString = string.Join("/", DataBuilder.ExplorersPokemon.Where(e => preEvos.Select(g => g.GenderEnitities).SelectMany(h => h).Any(i => i.Evolution.PreEvoRawId == e.RawIndex)));
            string preEvoString = string.Join("/", preEvos.Select(e => e.Name));

            List<ExplorersEnitity> postEvos = DataBuilder.ExplorersPokemon.Where(e => e.GenderEnitities.Any(f => f.Evolution.PreEvoRawId == explorersPokemon.RawIndex)).ToList();
            string postEvoString = string.Join("/", postEvos.Select(e => e.Name));
            string postpostEvoString = string.Join("/", DataBuilder.ExplorersPokemon.Where(e => e.GenderEnitities.Any(f => postEvos.Select(g => g.RawIndex).Contains(f.Evolution.PreEvoRawId))).Select(e => e.Name));

            builder.AddField("Evolutionary Line", $"{prepreEvoString} > {preEvoString} > **{explorersPokemon.Name}** > {postEvoString} > {postpostEvoString}".Trim('>', ' '));

            builder.AddField("Base Stats", explorersPokemon.GenderEnitities.First().BaseStats.ToString());

            builder.AddField("IQ Group", explorersPokemon.GenderEnitities.First().IQGroup.ToString(), true);

            builder.AddField("Recruit Rate", $"{explorersPokemon.GenderEnitities.First().RecruitRate.ToString(CultureInfo.InvariantCulture)}%", true);

            builder.AddField("Base EXP Yield", explorersPokemon.GenderEnitities.First().BaseExpYield.ToString(), true);

            List<ExplorersItem> exclusiveItems = new List<ExplorersItem>();

            exclusiveItems.Add(DataBuilder.ExplorersItems.FirstOrDefault(e =>
            {
                int? exclusiveItemsOneStarExclusiveA = explorersPokemon.GenderEnitities.First().ExclusiveItems.OneStarExclusiveA;
                return exclusiveItemsOneStarExclusiveA != null && e.ItemId == exclusiveItemsOneStarExclusiveA;
            }));
            exclusiveItems.Add(DataBuilder.ExplorersItems.FirstOrDefault(e =>
            {
                int? exclusiveItemsOneStarExclusiveB = explorersPokemon.GenderEnitities.First().ExclusiveItems.OneStarExclusiveB;
                return exclusiveItemsOneStarExclusiveB != null && e.ItemId == exclusiveItemsOneStarExclusiveB;
            }));
            exclusiveItems.Add(DataBuilder.ExplorersItems.FirstOrDefault(e =>
            {
                int? exclusiveItemsTwoStarExclusive = explorersPokemon.GenderEnitities.First().ExclusiveItems.TwoStarExclusive;
                return exclusiveItemsTwoStarExclusive != null && e.ItemId == exclusiveItemsTwoStarExclusive;
            }));
            exclusiveItems.Add(DataBuilder.ExplorersItems.FirstOrDefault(e =>
            {
                int? exclusiveItemsThreeStarExclusive = explorersPokemon.GenderEnitities.First().ExclusiveItems.ThreeStarExclusive;
                return exclusiveItemsThreeStarExclusive != null && e.ItemId == exclusiveItemsThreeStarExclusive;
            }));

            exclusiveItems = exclusiveItems.Where(e => e != null).ToList();

            builder.AddField("Exclusive Items", string.Join(", ", exclusiveItems.Select(e => e.Name)));

            builder.WithFooter($"#{explorersPokemon.DexId}", "https://cdn.discordapp.com/emojis/330534908101394432.png");

            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(DataBuilder.ExplorersPortraits.First(e => e.IndexId == explorersPokemon.RawIndex).Portraits.First(e => e.PortraitId == 0).PortraitImageBase64)))
            {
                await ctx.RespondWithFileAsync($"{explorersPokemon.Id}.png", stream);
                await ctx.RespondAsync($"__**{explorersPokemon.Name}, the {explorersPokemon.Category} Pokémon**__", false, builder.Build());
            }
        }

        public async Task LevelStats(CommandContext ctx, string pokemon, int level)
        {

        }

        [Command("portrait"), Description("Retrieves portrait of the requested Pokemon.")]
        public async Task Portrait(CommandContext ctx, [Description("Requested Pokemon name or Dex Id."), ] string pokemon, string name = null)
        {
            if (string.IsNullOrWhiteSpace(pokemon))
            {
                await ctx.RespondAsync("Invalid entry.");
                return;
            }

            pokemon = pokemon.ToLower();

            ExplorersEnitity explorersPokemon;

            if (int.TryParse(pokemon, out int result))
            {
                explorersPokemon = DataBuilder.ExplorersPokemon.FirstOrDefault(e => e.DexId == result);
            }
            else
            {
                explorersPokemon = DataBuilder.ExplorersPokemon.FirstOrDefault(e => e.Name.ToLower() == pokemon || e.Id == pokemon);
            }

            if (explorersPokemon == null)
            {
                IEnumerable<string> similarPokemon = DataBuilder.ExplorersPokemon
                    .GroupBy(e =>
                    {
                        int idComparasion = LevenshteinDistance.Compute(e.Id, pokemon);
                        int nameComparasion = LevenshteinDistance.Compute(e.Name, pokemon);
                        return idComparasion < nameComparasion ? idComparasion : nameComparasion;
                    })
                    .OrderBy(e => e.Key)
                    .First()
                    .Select(e => e.Name);

                await ctx.RespondAsync($"Pokemon not found. Did you mean any of the following: `{string.Join("`, `", similarPokemon)}`?");
                return;
            }

            PortraitEntity portraitEntity = DataBuilder.ExplorersPortraits.First(e => e.IndexId == explorersPokemon.RawIndex);

            if (portraitEntity == null)
            {
                throw new OutputException($"{explorersPokemon.Name} has no portrait in the research database.");
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                Portrait portrait = portraitEntity.Portraits.FirstOrDefault(e => string.Equals(e.PortraitName, name, StringComparison.CurrentCultureIgnoreCase));

                if (portrait == null)
                {
                    IEnumerable<string> similarPortraitTypes = portraitEntity.Portraits
                        .Select(e => e.PortraitName)
                        .Where(e => !string.IsNullOrWhiteSpace(e));

                    await ctx.RespondAsync($"Portrait with name {name} not found. Did you mean any of the following: `{string.Join("`, `", similarPortraitTypes)}`?");
                    return;
                }

                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(portrait.PortraitImageBase64)))
                {
                    await ctx.RespondWithFileAsync($"{explorersPokemon.Name}-{portrait.PortraitName}.png", stream);
                }
            }

            else
            {
                using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(portraitEntity.Portraits.First().PortraitImageBase64)))
                {
                    await ctx.RespondWithFileAsync($"{explorersPokemon.Name}-{portraitEntity.Portraits.First().PortraitName}.png", stream);
                }
            }
        }
    }
}
