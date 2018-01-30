using System;
using System.Collections.Generic;
using System.Text;

namespace DashingWanderer.Data.Explorers.Items.Enums
{
    public static class RarityEnum
    {
        public enum ItemRarity
        {
            None,
            TypeExclusiveOneStarA,
            TypeExclusiveOneStarB,
            TypeExclusiveTwoStar,
            TypeExclusiveThreeStar,
            PokemonExclusiveOneStarA,
            PokemonExclusiveOneStarB,
            PokemonExclusiveTwoStar,
            PokemonExclusiveThreeStar,
            /// <summary>
            /// The Pokemon may hatch holding the item.
            /// </summary>
            PokemonExclusiveThreeStarHatch,
            /// <summary>
            /// Exact purpose unknown. Only all the Eeveelutions, and the Tyrogue line have items with this type!
            /// </summary>
            PokemonExclusiveThreeStarUnknown
        }
    }
}
