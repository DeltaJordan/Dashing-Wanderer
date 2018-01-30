using System;
using System.Linq;
using DashingWanderer.Data.Explorers.Enums;
using DashingWanderer.Data.Explorers.Pokedex.Enums;
using DashingWanderer.Extensions;
using RawDataExplorers.Data;
using RawDataExplorers.Data.Pokemon;

namespace DashingWanderer.Data.Explorers.Pokedex
{
    public class ExplorersEnitity
    {
        public string Id { get; set; }
        public int DexId { get; set; }
        public int RawIndex { get; set; }
        public string Name { get; set; }
        public GameEnum.GameSeries GameSeries { get; set; }
        public TypeEnum.PokemonType PrimaryType { get; set; }
        public TypeEnum.PokemonType SecondaryType { get; set; }
        public string Category { get; set; }
        public LevelStats[] LevelStats { get; set; }
        public GenderEntity[] GenderEnitities { get; set; }
        public Moveset[] Movesets { get; set; }

        public static ExplorersEnitity FromRawData(Pokemon pokemon)
        {
            ExplorersEnitity data = new ExplorersEnitity
            {
                DexId = Convert.ToInt32(pokemon.Pokemon2.GenderedEntity.First().PokedexNumber),
                RawIndex = Convert.ToInt32(pokemon.Pokemon2.GenderedEntity.First().PokeID),
                Category = pokemon.Pokemon2.Strings.English.Category,
                GameSeries = GameEnum.GameSeries.Explorers,
                PrimaryType = (TypeEnum.PokemonType)Convert.ToInt32(pokemon.Pokemon2.GenderedEntity.First().PrimaryType),
                SecondaryType = (TypeEnum.PokemonType)Convert.ToInt32(pokemon.Pokemon2.GenderedEntity.First().SecondaryType),
                Name = pokemon.Pokemon2.Strings.English.Name,
                GenderEnitities = (from genderedEntity in pokemon.Pokemon2.GenderedEntity
                    where genderedEntity.Gender != "0"
                    select new GenderEntity
                    {
                        BaseExpYield = Convert.ToInt32(genderedEntity.ExpYield),
                        BaseStats = new BaseStats
                        {
                            Attack = Convert.ToInt32(genderedEntity.BaseStats.Attack),
                            Defense = Convert.ToInt32(genderedEntity.BaseStats.Defense),
                            HP = Convert.ToInt32(genderedEntity.BaseStats.HP),
                            SpecialAttack = Convert.ToInt32(genderedEntity.BaseStats.SpAttack),
                            SpecialDefense = Convert.ToInt32(genderedEntity.BaseStats.SpDefense)
                        },
                        Evolution = new EvolutionRequirements
                        {
                            PreEvoDexId = Convert.ToInt32(genderedEntity.EvolutionReq.PreEvoIndex),
                            PreEvoRawId = Convert.ToInt32(genderedEntity.EvolutionReq.PreEvoIndex),
                            Method = (EvoEnum.EvolutionMethod)Convert.ToInt32(genderedEntity.EvolutionReq.Method),
                            Param1 = Convert.ToInt32(genderedEntity.EvolutionReq.Param1).NullIfZero(),
                            Param2 = Convert.ToInt32(genderedEntity.EvolutionReq.Param2).NullIfZero()
                        },
                        ExclusiveItems = new ExclusiveItems
                        {
                            OneStarExclusiveA = Convert.ToInt32(genderedEntity.ExclusiveItems.ItemID[0]).NullIfZero(),
                            OneStarExclusiveB = Convert.ToInt32(genderedEntity.ExclusiveItems.ItemID[1]).NullIfZero(),
                            TwoStarExclusive = Convert.ToInt32(genderedEntity.ExclusiveItems.ItemID[2]).NullIfZero(),
                            ThreeStarExclusive = Convert.ToInt32(genderedEntity.ExclusiveItems.ItemID[3]).NullIfZero()
                        },
                        Gender = (GenderEnum.Gender)Convert.ToInt32(genderedEntity.Gender),
                        IQGroup = (IQEnum.IQGroup)Convert.ToInt32(genderedEntity.IQGroup),
                        PrimaryAbility = (AbilityEnum.Ability)Convert.ToInt32(genderedEntity.PrimaryAbility),
                        SecondaryAbility = (AbilityEnum.Ability)Convert.ToInt32(genderedEntity.SecondaryAbility),
                        RecruitRate = Convert.ToDouble(genderedEntity.RecruitRate2) / 10
                    }).ToArray(),
                LevelStats = pokemon.Pokemon2.StatsGrowth.Level.Select(level => new LevelStats
                {
                    Attack = Convert.ToInt32(level.Attack),
                    Defense = Convert.ToInt32(level.Defense),
                    HP = Convert.ToInt32(level.HP),
                    RequiredExp = Convert.ToInt32(level.RequiredExp),
                    SpecialAttack = Convert.ToInt32(level.SpAttack),
                    SpecialDefense = Convert.ToInt32(level.SpDefense)
                }).ToArray(),
                Movesets = pokemon.Pokemon2.Moveset.Select(moveset => new Moveset
                {
                    EggMoves = moveset.EggMoves == null
                        ? null
                        : new EggMoves
                        {
                            MoveIds = moveset.EggMoves.MoveID.Select(e => Convert.ToInt32(e)).ToArray()
                        },
                    HmTmMoves = moveset.HmTmMoves == null
                        ? null
                        : new HmTmMoves
                        {
                            MoveIds = moveset.HmTmMoves.MoveID.Select(e => Convert.ToInt32(e)).ToArray()
                        },
                    LevelUpMoves = moveset.LevelUpMoves?.Learn.Select(learn => new LevelUpMoves
                    {
                        Level = Convert.ToInt32(learn.Level),
                        MoveId = Convert.ToInt32(learn.MoveID)
                    }).ToArray()
                }).ToArray()
            };

            return data;
        }
    }

    public class GenderEntity
    {
        public EvolutionRequirements Evolution { get; set; }

        public GenderEnum.Gender Gender { get; set; }

        public IQEnum.IQGroup IQGroup { get; set; }

        public AbilityEnum.Ability PrimaryAbility { get; set; }

        public AbilityEnum.Ability SecondaryAbility { get; set; }

        public int BaseExpYield { get; set; }

        // RecruitRate1 from the raw data seems to be unused
        /// <summary>
        /// Recruit rate, represented as a percent.
        /// </summary>
        public double RecruitRate { get; set; }

        public BaseStats BaseStats { get; set; }

        public ExclusiveItems ExclusiveItems { get; set; }
    }

    public class EvolutionRequirements
    {
        public int PreEvoDexId { get; set; }

        /// <summary>
        /// The prevolution id used to find
        /// </summary>
        public int PreEvoRawId { get; set; }

        public EvoEnum.EvolutionMethod Method { get; set; }

        /// <summary>
        /// See EvolutionMethod enums
        /// </summary>
        public int? Param1 { get; set; }

        /// <summary>
        /// See EvolutionMethod enums
        /// </summary>
        public int? Param2 { get; set; }
    }

    public class BaseStats
    {
        public int HP { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int SpecialAttack { get; set; }

        public int SpecialDefense { get; set; }

        /// <summary>
        /// Converts BaseStats to a Discord Embed formatted string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"HP: **{this.HP}**, ATK: **{this.Attack}**, DEF: **{this.Defense}**, SPA: **{this.SpecialAttack}**, SPD: **{this.SpecialDefense}**";
        }
    }

    public class ExclusiveItems
    {
        /// <summary>
        /// The first 1-star exclusive item for this Pokemon. null if none.
        /// </summary>
        public int? OneStarExclusiveA { get; set; }

        /// <summary>
        /// The second 1-star exclusive item for this Pokemon. null if none.
        /// </summary>
        public int? OneStarExclusiveB { get; set; }

        /// <summary>
        /// The 2-star exclusive item for this Pokemon. null if none.
        /// </summary>
        public int? TwoStarExclusive { get; set; }

        /// <summary>
        /// The 3-star exclusive item for this Pokemon. null if none.
        /// </summary>
        public int? ThreeStarExclusive { get; set; }

    }

    public class LevelStats
    {
        public int RequiredExp { get; set; }

        public int HP { get; set; }

        public int Attack { get; set; }

        public int Defense { get; set; }

        public int SpecialAttack { get; set; }

        public int SpecialDefense { get; set; }
    }

    public class Moveset
    {
        public LevelUpMoves[] LevelUpMoves { get; set; }

        public EggMoves EggMoves { get; set; }

        public HmTmMoves HmTmMoves { get; set; }
    }

    public class EggMoves
    {
        public int[] MoveIds { get; set; }
    }

    public class HmTmMoves
    {
        public int[] MoveIds { get; set; }
    }

    public class LevelUpMoves
    {
        public int Level { get; set; }

        public int MoveId { get; set; }
    }
}