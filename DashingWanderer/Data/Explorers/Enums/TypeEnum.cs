extern alias SystemDrawing;
using SystemDrawing::System.Drawing;


namespace DashingWanderer.Data.Explorers.Enums
{
    public static class TypeEnum
    {
        public enum PokemonType
        {
            None = 0x00,
            Normal = 0x01,
            Fire = 0x02,
            Water = 0x03,
            Grass = 0x04,
            Electric = 0x05,
            Ice = 0x06,
            Fighting = 0x07,
            Poison = 0x08,
            Ground = 0x09,
            Flying = 0x0A,
            Psychic = 0x0B,
            Bug = 0x0C,
            Rock = 0x0D,
            Ghost = 0x0E,
            Dragon = 0x0F,
            Dark = 0x10,
            Steel = 0x11,
            Neutral = 0x12
        }

        public static string ToHex(this PokemonType type)
        {
            string colorHex = "#000000";
            switch (type)
            {
                case PokemonType.Normal:
                    colorHex = "#A8A878";
                    break;
                case PokemonType.Fighting:
                    colorHex = "#C03028";
                    break;
                case PokemonType.Flying:
                    colorHex = "#A890F0";
                    break;
                case PokemonType.Poison:
                    colorHex = "#A040A0";
                    break;
                case PokemonType.Ground:
                    colorHex = "#E0C068";
                    break;
                case PokemonType.Rock:
                    colorHex = "#B8A038";
                    break;
                case PokemonType.Bug:
                    colorHex = "#A8B820";
                    break;
                case PokemonType.Ghost:
                    colorHex = "#705898";
                    break;
                case PokemonType.Steel:
                    colorHex = "#B8B8D0";
                    break;
                case PokemonType.Fire:
                    colorHex = "#F08030";
                    break;
                case PokemonType.Water:
                    colorHex = "#6890F0";
                    break;
                case PokemonType.Grass:
                    colorHex = "#78C850";
                    break;
                case PokemonType.Electric:
                    colorHex = "#F8D030";
                    break;
                case PokemonType.Psychic:
                    colorHex = "#F85888";
                    break;
                case PokemonType.Ice:
                    colorHex = "#98D8D8";
                    break;
                case PokemonType.Dragon:
                    colorHex = "#7038F8";
                    break;
                case PokemonType.Dark:
                    colorHex = "#705848";
                    break;
            }

            return colorHex;
        }
    }
}
