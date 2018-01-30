namespace DashingWanderer.Data.Explorers.Pokedex.Enums
{
    public static class EvoEnum
    {
        public enum EvolutionMethod
        {
            /// <summary>
            /// Special evolutions are also set to this value for some reason.
            /// <para></para>
            /// Documented specials: Shedinja
            /// </summary>
            CannotEvolve = 0,
            /// <summary>
            /// Param1 = required Level
            /// <para></para>
            /// Param2 = optional required Item id
            /// </summary>
            LevelUp = 1,
            /// <summary>
            /// Param1 = required IQ
            /// <para></para>
            /// Param2 = optional required item id
            /// </summary>
            IQ = 2,
            /// <summary>
            /// Param1 = required item id
            /// <para></para>
            /// Param2 = optional required item id?
            /// </summary>
            Item = 3,
            /// <summary>
            /// In addition to the evolving Pokemon, another Pokemon defined by Param1 must be in the friends list.
            /// </summary>
            AdditionalRecruit = 4,
            LinkCable = 5
        }
    }
}
