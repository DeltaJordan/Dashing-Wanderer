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
using DashingWanderer.Data.Explorers.Items.Enums;
using DashingWanderer.Data.Explorers.Moves;
using DashingWanderer.Data.Explorers.Pokedex;
using DashingWanderer.Data.Explorers.Pokedex.Enums;
using DashingWanderer.Exceptions;
using DashingWanderer.Extensions;
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
                throw new DiscordMessageException("Invalid entry.");
            }

            ExplorersEnitity explorersPokemon = GetPokemon(pokemon);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();

            builder.AddField("Types", $"{explorersPokemon.PrimaryType}{(explorersPokemon.SecondaryType == TypeEnum.PokemonType.None ? "" : $", {explorersPokemon.SecondaryType}")}", true);
            builder.AddField("Abilities", $"{explorersPokemon.GenderEnitities.First().PrimaryAbility}{(explorersPokemon.GenderEnitities.First().SecondaryAbility == AbilityEnum.Ability.None ? "" : $", {explorersPokemon.GenderEnitities.First().SecondaryAbility}")}".Replace("_", " "), true);

            List<ExplorersEnitity> preEvos = DataBuilder.ExplorersPokemon.Where(e => explorersPokemon.GenderEnitities.Any(f => f.Evolution.PreEvoRawId == e.RawIndex)).ToList();
            string prepreEvoString = string.Join("/", DataBuilder.ExplorersPokemon.Where(e => preEvos.Select(g => g.GenderEnitities).SelectMany(h => h).Any(i => i.Evolution.PreEvoRawId == e.RawIndex)).Select(e => e.Name));
            string preEvoString = string.Join("/", preEvos.Select(e => e.Name));

            List<ExplorersEnitity> postEvos = DataBuilder.ExplorersPokemon.Where(e => e.GenderEnitities.Any(f => f.Evolution.PreEvoRawId == explorersPokemon.RawIndex)).ToList();
            string postEvoString = string.Join("/", postEvos.Select(e => e.Name));
            string postpostEvoString = string.Join("/", DataBuilder.ExplorersPokemon.Where(e => e.GenderEnitities.Any(f => postEvos.Select(g => g.RawIndex).Contains(f.Evolution.PreEvoRawId))).Select(e => e.Name));

            builder.AddField("Evolutionary Line", $"{prepreEvoString} > {preEvoString} > **{explorersPokemon.Name}** > {postEvoString} > {postpostEvoString}".Trim('>', ' '));

            builder.AddField("Base Stats", explorersPokemon.GenderEnitities.First().BaseStats.ToString());

            builder.AddField("IQ Group", explorersPokemon.GenderEnitities.First().IQGroup.ToString(), true);

            builder.AddField("Recruit Rate", $"{explorersPokemon.GenderEnitities.First().RecruitRate.ToString(CultureInfo.InvariantCulture)}%", true);

            builder.AddField("Base EXP Yield", explorersPokemon.GenderEnitities.First().BaseExpYield.ToString(), true);

            List<ExplorersItem> exclusiveItems = new List<ExplorersItem>
            {
                DataBuilder.ExplorersItems.FirstOrDefault(e =>
                {
                    int? exclusiveItemsOneStarExclusiveA = explorersPokemon.GenderEnitities.First().ExclusiveItems.OneStarExclusiveA;
                    return exclusiveItemsOneStarExclusiveA != null && e.ItemId == exclusiveItemsOneStarExclusiveA;
                }),
                DataBuilder.ExplorersItems.FirstOrDefault(e =>
                {
                    int? exclusiveItemsOneStarExclusiveB = explorersPokemon.GenderEnitities.First().ExclusiveItems.OneStarExclusiveB;
                    return exclusiveItemsOneStarExclusiveB != null && e.ItemId == exclusiveItemsOneStarExclusiveB;
                }),
                DataBuilder.ExplorersItems.FirstOrDefault(e =>
                {
                    int? exclusiveItemsTwoStarExclusive = explorersPokemon.GenderEnitities.First().ExclusiveItems.TwoStarExclusive;
                    return exclusiveItemsTwoStarExclusive != null && e.ItemId == exclusiveItemsTwoStarExclusive;
                }),
                DataBuilder.ExplorersItems.FirstOrDefault(e =>
                {
                    int? exclusiveItemsThreeStarExclusive = explorersPokemon.GenderEnitities.First().ExclusiveItems.ThreeStarExclusive;
                    return exclusiveItemsThreeStarExclusive != null && e.ItemId == exclusiveItemsThreeStarExclusive;
                })
            };


            exclusiveItems = exclusiveItems.Where(e => e != null).ToList();

            builder.AddField("Exclusive Items", string.Join(", ", exclusiveItems.Select(e => e.Name)));

            builder.WithFooter($"#{explorersPokemon.DexId}", "https://cdn.discordapp.com/emojis/330534908101394432.png");

            builder.WithColor(new DiscordColor(explorersPokemon.PrimaryType.ToHex()));

            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(DataBuilder.ExplorersPortraits.First(e => e.IndexId == explorersPokemon.RawIndex).Portraits.First(e => e.PortraitId == 0).PortraitImageBase64)))
            {
                await ctx.RespondWithFileAsync($"{explorersPokemon.Id}.png", stream);
                await ctx.RespondAsync($"__**{explorersPokemon.Name}, the {explorersPokemon.Category} Pokémon**__", false, builder.Build());
            }
        }

        [Command("item"), Description("Retrieves info about the requested item.")]
        public async Task Item(CommandContext ctx, [Description("Requested move name or move id."), RemainingText] string item)
        {
            ExplorersItem explorersItem = GetItem(item);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Title = explorersItem.Name
            };

            builder.WithDescription(explorersItem.ShortDescription);

            builder.AddField("More Detail", explorersItem.LongDescription);

            builder.AddField("Buy Price", explorersItem.BuyPrice, true);

            builder.AddField("Sell Price", explorersItem.SellPrice, true);

            switch (explorersItem.ItemCategory)
            {
                case ItemCategoryEnum.ItemCategory.Projectile:
                    builder.AddField("Item Group", "Projectile");
                    break;
                case ItemCategoryEnum.ItemCategory.Arc:
                    builder.AddField("Item Group", "Arc");
                    break;
                case ItemCategoryEnum.ItemCategory.SeedAndDrink:
                    builder.AddField("Item Group", "Seed and Drink");
                    break;
                case ItemCategoryEnum.ItemCategory.FoodAndGummi:
                    builder.AddField("Item Group", "Food and Gummi");
                    break;
                case ItemCategoryEnum.ItemCategory.HeldItem:
                    builder.AddField("Item Group", "Held Item");
                    break;
                case ItemCategoryEnum.ItemCategory.TMHM:
                    builder.AddField("Item Group", "TM/HM");
                    break;
                case ItemCategoryEnum.ItemCategory.Poké:
                    builder.AddField("Item Group", "Poké");
                    break;
                case ItemCategoryEnum.ItemCategory.NotApplicable:
                    break;
                case ItemCategoryEnum.ItemCategory.EvoAndMisc:
                    builder.AddField("Item Group", "Evolution Item and Miscellaneous");
                    break;
                case ItemCategoryEnum.ItemCategory.Orb:
                    builder.AddField("Item Group", "Orb");
                    break;
                case ItemCategoryEnum.ItemCategory.LinkBox:
                    builder.AddField("Item Group", "Link Box");
                    break;
                case ItemCategoryEnum.ItemCategory.UsedTM:
                    builder.AddField("Item Group", "Used TM");
                    break;
                case ItemCategoryEnum.ItemCategory.BoxOne:
                    builder.AddField("Item Group", "Box Group 1");
                    break;
                case ItemCategoryEnum.ItemCategory.BoxTwo:
                    builder.AddField("Item Group", "Box Group 2");
                    break;
                case ItemCategoryEnum.ItemCategory.BoxThree:
                    builder.AddField("Item Group", "Box Group 3");
                    break;
                case ItemCategoryEnum.ItemCategory.SpecificItems:
                    builder.AddField("Item Group", "Exclusive Items");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            switch (explorersItem.Rarity)
            {
                case RarityEnum.ItemRarity.None:
                    break;
                case RarityEnum.ItemRarity.TypeExclusiveOneStarA:
                    builder.AddField("Rarity", "☆");
                    break;
                case RarityEnum.ItemRarity.TypeExclusiveOneStarB:
                    builder.AddField("Rarity", "☆");
                    break;
                case RarityEnum.ItemRarity.TypeExclusiveTwoStar:
                    builder.AddField("Rarity", "☆☆");
                    break;
                case RarityEnum.ItemRarity.TypeExclusiveThreeStar:
                    builder.AddField("Rarity", "☆☆☆");
                    break;
                case RarityEnum.ItemRarity.PokemonExclusiveOneStarA:
                    builder.AddField("Rarity", "☆");
                    break;
                case RarityEnum.ItemRarity.PokemonExclusiveOneStarB:
                    builder.AddField("Rarity", "☆");
                    break;
                case RarityEnum.ItemRarity.PokemonExclusiveTwoStar:
                    builder.AddField("Rarity", "☆☆");
                    break;
                case RarityEnum.ItemRarity.PokemonExclusiveThreeStar:
                    builder.AddField("Rarity", "☆☆☆");
                    break;
                case RarityEnum.ItemRarity.PokemonExclusiveThreeStarHatch:
                    builder.AddField("Rarity", "☆☆☆\nThis pokemon can also hatch with this item!");
                    break;
                case RarityEnum.ItemRarity.PokemonExclusiveThreeStarUnknown:
                    builder.AddField("Rarity", "☆☆☆");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (explorersItem.ItemCategory == ItemCategoryEnum.ItemCategory.TMHM)
            {
                builder.AddField("TM/HM Move", DataBuilder.ExplorersMoves.First(e => e.MoveId == explorersItem.Param1).Name);
            }

            TypeEnum.PokemonType type = (TypeEnum.PokemonType)new Random().Next(1, Enum.GetValues(typeof(TypeEnum.PokemonType)).Length - 2);

            builder.WithColor(new DiscordColor(type.ToHex()));

            await ctx.RespondAsync(string.Empty, false, builder.Build());
        }

        [Command("move"), Description("Retrieves info about the requested move.")]
        public async Task Move(CommandContext ctx, [Description("Requested move name or move id."), RemainingText] string move)
        {
            ExplorersMove explorersMove = GetMove(move);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Title = explorersMove.Name
            };

            builder.AddField("Description", explorersMove.Description);

            builder.AddField("Base PP", explorersMove.BasePP.ToString(), true);

            builder.AddField("Base Power", explorersMove.BasePower.ToString(), true);

            builder.AddField("Accuracy", explorersMove.Accuracy.ToString(), true);

            builder.AddField("Category", explorersMove.Category.ToString(), true);

            builder.AddField("Type", explorersMove.Type.ToString(), true);
            
            builder.WithFooter($"#{explorersMove.MoveId}", "https://cdn.discordapp.com/emojis/330534908101394432.png");

            builder.WithColor(new DiscordColor(explorersMove.Type.ToHex()));

            await ctx.RespondAsync(string.Empty, false, builder.Build());
        }

        [Command("levelup"), Description("Retrieves the stats of a specificed Pokemon at a requested level.")]
        public async Task LevelStats(CommandContext ctx, 
            [Description("Requested Pokemon name or Dex Id. Must be in quotes.")] string pokemon, 
            [Description("Requested Pokemon's level.")] int level)
        {
            if (level > 100 || level < 1 || string.IsNullOrWhiteSpace(pokemon))
            {
                throw new DiscordMessageException("Invalid entry.");
            }

            ExplorersEnitity explorersPokemon = GetPokemon(pokemon);

            Moveset moveset = explorersPokemon.Movesets.First();

            LevelStats levelStats = new LevelStats
            {
                Attack = explorersPokemon.GenderEnitities.First().BaseStats.Attack + explorersPokemon.LevelStats.Take(level).Select(e => e.Attack).Aggregate((e, f) => e + f),
                Defense = explorersPokemon.GenderEnitities.First().BaseStats.Defense + explorersPokemon.LevelStats.Take(level).Select(e => e.Defense).Aggregate((e, f) => e + f),
                SpecialAttack = explorersPokemon.GenderEnitities.First().BaseStats.SpecialAttack + explorersPokemon.LevelStats.Take(level).Select(e => e.SpecialAttack).Aggregate((e, f) => e + f),
                SpecialDefense = explorersPokemon.GenderEnitities.First().BaseStats.SpecialDefense + explorersPokemon.LevelStats.Take(level).Select(e => e.SpecialDefense).Aggregate((e, f) => e + f),
                HP = explorersPokemon.GenderEnitities.First().BaseStats.HP + explorersPokemon.LevelStats.Take(level).Select(e => e.HP).Aggregate((e, f) => e + f),
                RequiredExp = explorersPokemon.LevelStats[level].RequiredExp
            };

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.AddField("Required Exp.", levelStats.RequiredExp.ToString());
            builder.AddField("Stats (Without eating any Gummis)", levelStats.ToString());
            if (moveset.LevelUpMoves.Any(e => e.Level == level))
            {
                builder.AddField("Learns new move(s):", string.Join(", ", DataBuilder.ExplorersMoves.Where(f => moveset.LevelUpMoves.Where(e => e.Level == level).Select(e => e.MoveId).Contains(f.MoveId)).Select(e => e.Name)));
            }

            builder.WithFooter($"#{explorersPokemon.DexId}", "https://cdn.discordapp.com/emojis/330534908101394432.png");

            builder.WithColor(new DiscordColor(explorersPokemon.PrimaryType.ToHex()));

            await ctx.RespondAsync($"**{explorersPokemon.Name} has leveled up! Let's have a look at its stats...**", false, builder.Build());
        }

        [Command("portrait"), Description("Retrieves portrait of the requested Pokemon.")]
        public async Task Portrait(CommandContext ctx, [Description("Requested Pokemon name or Dex Id."), ] string pokemon, string name = null)
        {
            if (string.IsNullOrWhiteSpace(pokemon))
            {
                throw new DiscordMessageException("Invalid entry.");
            }

            ExplorersEnitity explorersPokemon = GetPokemon(pokemon);

            PortraitEntity portraitEntity = DataBuilder.ExplorersPortraits.First(e => e.IndexId == explorersPokemon.RawIndex);

            if (portraitEntity == null)
            {
                throw new DiscordMessageException($"{explorersPokemon.Name} has no portrait in the research database.");
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

        private static ExplorersItem GetItem(string item)
        {
            item = item.ToLower();

            ExplorersItem explorersMove;

            if (int.TryParse(item, out int result))
            {
                explorersMove = DataBuilder.ExplorersItems.FirstOrDefault(e => e.ItemId == result);
            }
            else
            {
                explorersMove = DataBuilder.ExplorersItems.FirstOrDefault(e => e.Name.ToLower() == item || e.Id == item);
            }

            if (explorersMove == null)
            {
                IEnumerable<string> similarPokemon = DataBuilder.ExplorersMoves
                    .GroupBy(e =>
                    {
                        int idComparasion = LevenshteinDistance.Compute(e.Id, item);
                        int nameComparasion = LevenshteinDistance.Compute(e.Name, item);
                        return idComparasion < nameComparasion ? idComparasion : nameComparasion;
                    })
                    .OrderBy(e => e.Key)
                    .First()
                    .Select(e => e.Name);

                throw new DiscordMessageException($"Item not found. Did you mean any of the following: `{string.Join("`, `", similarPokemon)}`?");
            }

            return explorersMove;
        }

        private static ExplorersMove GetMove(string move)
        {
            move = move.ToLower();

            ExplorersMove explorersMove;

            if (int.TryParse(move, out int result))
            {
                explorersMove = DataBuilder.ExplorersMoves.FirstOrDefault(e => e.MoveId == result);
            }
            else
            {
                explorersMove = DataBuilder.ExplorersMoves.FirstOrDefault(e => e.Name.ToLower() == move || e.Id == move);
            }

            if (explorersMove == null)
            {
                IEnumerable<string> similarPokemon = DataBuilder.ExplorersMoves
                    .GroupBy(e =>
                    {
                        int idComparasion = LevenshteinDistance.Compute(e.Id, move);
                        int nameComparasion = LevenshteinDistance.Compute(e.Name, move);
                        return idComparasion < nameComparasion ? idComparasion : nameComparasion;
                    })
                    .OrderBy(e => e.Key)
                    .First()
                    .Select(e => e.Name);

                throw new DiscordMessageException($"Pokemon not found. Did you mean any of the following: `{string.Join("`, `", similarPokemon)}`?");
            }

            return explorersMove;
        }

        private static ExplorersEnitity GetPokemon(string pokemon)
        {
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

                throw new DiscordMessageException($"Pokemon not found. Did you mean any of the following: `{string.Join("`, `", similarPokemon)}`?");
            }

            return explorersPokemon;
        }
    }
}
