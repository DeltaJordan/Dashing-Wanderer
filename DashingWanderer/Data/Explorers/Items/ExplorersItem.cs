using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DashingWanderer.Data.Explorers.Items.Enums;
using DashingWanderer.Extensions;

namespace DashingWanderer.Data.Explorers.Items
{
    public class ExplorersItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public RarityEnum.ItemRarity Rarity { get; set; }
        /// <summary>
        /// If the item's rarity type is set to "exclusive to a Pokemon", the parameter contains that Pokemon's National Pokedex number.
        /// <para></para>
        /// If the item's rarity type is set to "exclusive to a type", the parameter contains the ID of the type. (PokemonType)
        /// </summary>
        public int? RarityParameter { get; set; }
        public ItemCategoryEnum.ItemCategory ItemCategory { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int SpriteId { get; set; }
        public int ItemId { get; set; }
        /// <summary>
        /// If the item is an HM/TM, this value is the move ID it refers to. Its also used the same way by some items, like orbs.
        /// <para></para>
        /// Otherwise, with items of any other category, its role is unknown!
        /// </summary>
        public int Param1 { get; set; }
        public int Param2 { get; set; }
        public int Param3 { get; set; }

        public static ExplorersItem FromRawData(RawDataExplorers.Data.Items.Item rawItem)
        {
            ExplorersItem data = new ExplorersItem
            {
                Id = new string(rawItem.Strings.English.Name?.ToLower().Replace(" ", "-").Where(e => !Path.GetInvalidPathChars().Contains(e)).ToArray()),
                Name = rawItem.Strings.English.Name,
                Rarity = (RarityEnum.ItemRarity)Convert.ToInt32(string.IsNullOrWhiteSpace(rawItem.ExclusiveData?.Type) ? "0" : rawItem.ExclusiveData.Type),
                RarityParameter = Convert.ToInt32(string.IsNullOrWhiteSpace(rawItem.ExclusiveData?.Parameter) ? "0x0" : rawItem.ExclusiveData.Parameter, 16).NullIfZero(),
                ItemCategory = (ItemCategoryEnum.ItemCategory)Convert.ToInt32(rawItem.Data.Category),
                ShortDescription = rawItem.Strings.English.ShortDescription,
                LongDescription = rawItem.Strings.English.LongDescription,
                BuyPrice = Convert.ToInt32(rawItem.Data.BuyPrice),
                SellPrice = Convert.ToInt32(rawItem.Data.SellPrice),
                SpriteId = Convert.ToInt32(rawItem.Data.SpriteID),
                ItemId = Convert.ToInt32(rawItem.Data.ItemID),
                Param1 = Convert.ToInt32(rawItem.Data.Param1, 16),
                Param2 = Convert.ToInt32(rawItem.Data.Param2, 16),
                Param3 = Convert.ToInt32(rawItem.Data.Param3, 16)
            };
            return data;
        }
    }
}
