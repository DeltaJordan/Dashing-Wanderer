extern alias SystemDrawing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SystemDrawing::System.Drawing;
using DashingWanderer.Data.Explorers;
using DashingWanderer.Data.Explorers.Items;
using DashingWanderer.Data.Explorers.Moves;
using DashingWanderer.Data.Explorers.Pokedex;
using DashingWanderer.Data.Explorers.Pokedex.Enums;
using Newtonsoft.Json;
using PortraitsAdder;
using RawDataExplorers.Data;
using RawDataExplorers.Data.Items;
using RawDataExplorers.Data.Pokemon;
using RawDataPMDExplorers.Data.Moves;

namespace DashingWanderer.Data
{
    public static class DataBuilder
    {
        public static List<PortraitEntity> ExplorersPortraits { get; private set; }
        public static List<ExplorersEnitity> ExplorersPokemon { get; private set; }
        public static List<ExplorersItem> ExplorersItems { get; private set; }
        public static List<ExplorersMove> ExplorersMoves { get; private set; }
        public static List<IQSkill> ExplorersIQSkills { get; private set; }

        public static readonly string PortraitFolder = Path.Combine(Globals.AppPath, "Data", "Portraits");
        public static readonly string PokemonDataFolder = Path.Combine(Globals.AppPath, "Data", "Pokemon");
        public static readonly string ItemDataFolder = Path.Combine(Globals.AppPath, "Data", "Items");
        public static readonly string MoveDataFolder = Path.Combine(Globals.AppPath, "Data", "Moves");
        public static readonly string IQDataFolder = Path.Combine(Globals.AppPath, "Data", "IQ");

        public static void GetExplorersData()
        {
            string currentFile = string.Empty;

            try
            {
                ExplorersItems = new List<ExplorersItem>();
                foreach (string file in Directory.GetFiles(ItemDataFolder, "*.json"))
                {
                    currentFile = file;
                    ExplorersItems.Add(JsonConvert.DeserializeObject<ExplorersItem>(File.ReadAllText(file)));
                }

                ExplorersMoves = new List<ExplorersMove>();
                foreach (string file in Directory.GetFiles(MoveDataFolder, "*.json"))
                {
                    currentFile = file;
                    ExplorersMoves.Add(JsonConvert.DeserializeObject<ExplorersMove>(File.ReadAllText(file)));
                }

                ExplorersPokemon = new List<ExplorersEnitity>();
                foreach (string file in Directory.GetFiles(PokemonDataFolder, "*.json"))
                {
                    currentFile = file;
                    ExplorersPokemon.Add(JsonConvert.DeserializeObject<ExplorersEnitity>(File.ReadAllText(file)));
                }

                ExplorersPortraits = new List<PortraitEntity>();
                foreach (string file in Directory.GetFiles(PortraitFolder, "*.json"))
                {
                    currentFile = file;
                    ExplorersPortraits.Add(JsonConvert.DeserializeObject<PortraitEntity>(File.ReadAllText(file)));
                }

                ExplorersIQSkills = new List<IQSkill>();
                foreach (string file in Directory.GetFiles(IQDataFolder, "*.json", SearchOption.AllDirectories))
                {
                    currentFile = file;
                    ExplorersIQSkills.Add(JsonConvert.DeserializeObject<IQSkill>(File.ReadAllText(file)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(currentFile);
                Console.WriteLine(e);
            }
        }

        public static void WriteExplorersPortraitsToFiles()
        {
            foreach (PortraitEntity portraitEntity in ExplorersPortraits)
            {
                foreach (Portrait explorersPortrait in portraitEntity.Portraits)
                {
                    using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(explorersPortrait.PortraitImageBase64)))
                    {
                        Image.FromStream(stream).Save(Path.Combine(
                                Directory.CreateDirectory(Path.Combine(PortraitFolder, $"{portraitEntity.IndexId}")).FullName, 
                                $"{explorersPortrait.PortraitId}.png"));
                    }
                }
            }
        }

        public static void BuildExplorersDataIndexes()
        {
            try
            {
                List<string> itemFileIndex = Directory.EnumerateFiles(ItemDataFolder, "*.json").Select(file => file.Replace(ItemDataFolder, string.Empty)).ToList();
                File.WriteAllText(Path.Combine(ItemDataFolder, "index.dat"), JsonConvert.SerializeObject(itemFileIndex, Formatting.Indented));

                List<string> moveFileIndex = Directory.EnumerateFiles(MoveDataFolder, "*.json").Select(file => file.Replace(MoveDataFolder, string.Empty)).ToList();
                File.WriteAllText(Path.Combine(MoveDataFolder, "index.dat"), JsonConvert.SerializeObject(moveFileIndex, Formatting.Indented));

                List<string> pokemonFileIndex = Directory.EnumerateFiles(PokemonDataFolder, "*.json").Select(file => file.Replace(PokemonDataFolder, string.Empty)).ToList();
                File.WriteAllText(Path.Combine(PokemonDataFolder, "index.dat"), JsonConvert.SerializeObject(pokemonFileIndex, Formatting.Indented));

                File.WriteAllText(Path.Combine(PortraitFolder, "index.dat"), JsonConvert.SerializeObject(Directory
                    .EnumerateFiles(PortraitFolder, "*", SearchOption.AllDirectories)
                    .Where(file => file.ToLower().EndsWith("json") || file.ToLower().EndsWith("png"))
                    .Select(file => file.Replace(PortraitFolder, string.Empty))
                    .ToList(), Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void BuildExplorerMoveData()
        {
            foreach (string file in Directory.GetFiles(MoveDataFolder))
            {
                File.Delete(file);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(Globals.AppPath, "RawMoveData"), "*.json"))
            {
                try
                {
                    RawMoveData rawMove = JsonConvert.DeserializeObject<RawMoveData>(File.ReadAllText(file));

                    if (rawMove == null)
                    {
                        Console.WriteLine($"{file} deserialized to null!");
                        continue;
                    }

                    ExplorersMove moveData = ExplorersMove.FromRawData(rawMove);

                    File.WriteAllText(Path.Combine(MoveDataFolder, $"{moveData.Id}.json"), JsonConvert.SerializeObject(moveData, Formatting.Indented));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed on file {file}.");
                    Console.WriteLine(e);
                }
            }
        }

        public static void BuildExplorerIQSkills()
        {
            string[] iqLines = File.ReadAllLines(Path.Combine(Globals.AppPath, "IQGroups.csv"));

            IQEnum.IQGroup currentIQGroup = IQEnum.IQGroup.A;
            foreach (string iqLine in iqLines)
            {
                string remainingText = iqLine;
                IQSkill skill = new IQSkill();

                if (int.TryParse(Regex.Match(remainingText, @"[0-9]*,").Value.TrimEnd(','), out int result))
                {
                    currentIQGroup = (IQEnum.IQGroup)result;

                    remainingText = remainingText.Substring(remainingText.IndexOf(',') + 1);
                }

                remainingText = remainingText.TrimStart(',');

                skill.Group = currentIQGroup;

                skill.Name = Regex.Match(remainingText, @"[^,]+", RegexOptions.IgnoreCase).Value;
                skill.Id = skill.Name.Replace(" ", "_");

                remainingText = remainingText.Substring(remainingText.IndexOf(',') + 1);

                skill.RequiredIQ = Regex.Match(remainingText, @"[^,]+").Value == "Base Skill" ? 0 : int.Parse(Regex.Match(remainingText, @"[0-9]+").Value);

                remainingText = remainingText.Substring(remainingText.IndexOf(',') + 1);

                skill.Effect = remainingText.Trim(',', '"');

                File.WriteAllText(Path.Combine(Directory.CreateDirectory(Path.Combine(IQDataFolder, $"{skill.Group}")).FullName, $"{skill.Id}.json"), JsonConvert.SerializeObject(skill, Formatting.Indented));
            }
        }

        public static void BuildExplorerItemData()
        {
            foreach (string file in Directory.GetFiles(ItemDataFolder))
            {
                File.Delete(file);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(Globals.AppPath, "RawItemData"), "*.json"))
            {
                try
                {
                    Item rawItem = JsonConvert.DeserializeObject<RawItemData>(File.ReadAllText(file)).Item;

                    if (rawItem == null)
                    {
                        Console.WriteLine($"{file} is null!");
                        continue;
                    }

                    ExplorersItem itemData = ExplorersItem.FromRawData(rawItem);

                    itemData.LongDescription = Regex.Replace(itemData.LongDescription, @"[\s]+Select detail:.+", string.Empty);
                    
                    File.WriteAllText(Path.Combine(ItemDataFolder, $"{itemData.Id}.json"), JsonConvert.SerializeObject(itemData, Formatting.Indented));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed on file {file}.");
                    Console.WriteLine(e);
                }
            }
        }

        public static void BuildExplorerPokemonData()
        {
            List<ExplorersEnitity> pokemonDatas = new List<ExplorersEnitity>();
            List<ExplorersEnitity> evoIndexWaiting = new List<ExplorersEnitity>();
            List<string> duplicatePokes = new List<string>();

            foreach (string file in Directory.GetFiles(PokemonDataFolder))
            {
                File.Delete(file);
            }

            foreach (string file in Directory.GetFiles(Path.Combine(Globals.AppPath, "RawPokemonData"), "*.json"))
            {
                try
                {
                    ExplorersEnitity pokemonData = ExplorersEnitity.FromRawData(JsonConvert.DeserializeObject<Pokemon>(File.ReadAllText(file)));

                    pokemonData.RawIndex = Convert.ToInt32(Regex.Match(Path.GetFileNameWithoutExtension(file), @"[0-9]+").Value.TrimStart('0'));

                    pokemonData.Id = new string(pokemonData.Name.Where(e => !Path.GetInvalidPathChars().Contains(e)).ToArray()).ToLower();

                    if (pokemonData.Category.ToCharArray().Distinct().Count() == 1 && pokemonData.Category.ToCharArray().Distinct().First() == '?')
                    {
                        Console.WriteLine($"Skipped special npc entity {pokemonData.Name}");
                        continue;
                    }

                    if (pokemonData.DexId == 0)
                    {
                        continue;
                    }

                    bool preEvoFound = false;

                    foreach (GenderEntity pokemonDataGenderEnitity in pokemonData.GenderEnitities)
                    {
                        switch (pokemonDataGenderEnitity.Evolution.Method)
                        {
                            case EvoEnum.EvolutionMethod.CannotEvolve:
                                if (pokemonDataGenderEnitity.Evolution.PreEvoDexId > 0)
                                {
                                    Console.WriteLine($"{pokemonData.Name} probably has a special evolution.");
                                }
                                else
                                {
                                    preEvoFound = true;
                                    continue;
                                }
                                break;
                        }

                        ExplorersEnitity preEvoData = pokemonDatas.FirstOrDefault(e => e.RawIndex == (pokemonDataGenderEnitity.Evolution.PreEvoDexId > 600 ? pokemonDataGenderEnitity.Evolution.PreEvoDexId - 600 : pokemonDataGenderEnitity.Evolution.PreEvoDexId));

                        if (preEvoData == null)
                        {
                            evoIndexWaiting.Add(pokemonData);
                            preEvoFound = false;
                            break;
                        }

                        pokemonDataGenderEnitity.Evolution.PreEvoDexId = preEvoData.DexId;

                        preEvoFound = true;
                    }

                    if (!preEvoFound)
                    {
                        continue;
                    }

                    if (pokemonDatas.Any(e => e.DexId == pokemonData.DexId))
                    {
                        ExplorersEnitity similarEntity = pokemonDatas.Find(e => e.DexId == pokemonData.DexId);

                        if (!similarEntity.Id.Contains("-base"))
                        {
                            File.Move(Path.Combine(PokemonDataFolder, $"{similarEntity.Id}.json"), Path.Combine(PokemonDataFolder, $"{similarEntity.Id}-base.json"));

                            similarEntity.Id += "-base";

                            duplicatePokes.Add(similarEntity.Id);
                        }

                        if (pokemonData.PrimaryType == similarEntity.PrimaryType)
                        {
                            if (pokemonData.SecondaryType == similarEntity.SecondaryType)
                            {
                                if (pokemonData.GenderEnitities.Select(e => e.Gender).SequenceEqual(similarEntity.GenderEnitities.Select(e => e.Gender)))
                                {
                                    pokemonData.Id += $"-{pokemonData.RawIndex}";
                                }
                                else
                                {
                                    pokemonData.Id = $"-{pokemonData.GenderEnitities.First().Gender}";
                                }
                            }
                            else
                            {
                                pokemonData.Id += $"-{pokemonData.SecondaryType}";
                            }
                        }
                        else
                        {
                            pokemonData.Id += $"-{pokemonData.PrimaryType}";
                        }

                        pokemonData.Id = pokemonData.Id.ToLower();

                        duplicatePokes.Add(pokemonData.Id);
                    }

                    pokemonDatas.Add(pokemonData);

                    File.WriteAllText(Path.Combine(PokemonDataFolder, $"{pokemonData.Id}.json"), JsonConvert.SerializeObject(pokemonData, Formatting.Indented));
                }
                catch (Exception e)
                {
                    if (e.ToString().Contains("DashingWanderer.Data.Pokedex.RawData.Learn[]"))
                    {
                        File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), @"""Learn"": (?<class>\{[^}]+\})", match => $"\"Learn\": [{match.Groups["class"].Captures[0].Value}]", RegexOptions.Singleline));

                        Console.WriteLine($"Failed on file {file}.\n{e}\nAttempted to implement Learn fix.");

                        continue;
                    }

                    Console.WriteLine($"Failed on file {file}.\n{e}");
                }
            }

            foreach (ExplorersEnitity pokemonData in evoIndexWaiting)
            {
                bool preEvoFound = false;

                foreach (GenderEntity pokemonDataGenderEnitity in pokemonData.GenderEnitities)
                {
                    if (pokemonDataGenderEnitity.Evolution.Method == EvoEnum.EvolutionMethod.CannotEvolve)
                    {
                        if (pokemonDataGenderEnitity.Evolution.PreEvoDexId > 0)
                        {
                            Console.WriteLine($"{pokemonData.Name} probably has a special evolution.");
                        }
                        else
                        {
                            preEvoFound = true;
                            continue;
                        }
                    }

                    ExplorersEnitity preEvoData = pokemonDatas.FirstOrDefault(e => e.RawIndex == (pokemonDataGenderEnitity.Evolution.PreEvoDexId > 600 ? pokemonDataGenderEnitity.Evolution.PreEvoDexId - 600 : pokemonDataGenderEnitity.Evolution.PreEvoDexId));

                    if (preEvoData == null)
                    {
                        Console.WriteLine($"Unable to find preEvo DexId for {pokemonData.Name} with preEvoIndex {pokemonDataGenderEnitity.Evolution.PreEvoDexId}!");
                        preEvoFound = false;
                        break;
                    }

                    pokemonDataGenderEnitity.Evolution.PreEvoDexId = preEvoData.RawIndex;

                    preEvoFound = true;
                }

                if (!preEvoFound)
                {
                    continue;
                }

                if (pokemonDatas.Any(e => e.DexId == pokemonData.DexId))
                {
                    ExplorersEnitity similarEntity = pokemonDatas.Find(e => e.DexId == pokemonData.DexId);

                    if (!similarEntity.Id.Contains("-base"))
                    {
                        File.Move(Path.Combine(PokemonDataFolder, $"{similarEntity.Id}.json"), Path.Combine(PokemonDataFolder, "Data", $"{similarEntity.Id}-base.json"));

                        similarEntity.Id += "-base";

                        duplicatePokes.Add(similarEntity.Id);
                    }

                    if (pokemonData.PrimaryType == similarEntity.PrimaryType)
                    {
                        if (pokemonData.SecondaryType == similarEntity.SecondaryType)
                        {
                            if (pokemonData.GenderEnitities.Select(e => e.Gender).SequenceEqual(similarEntity.GenderEnitities.Select(e => e.Gender)))
                            {
                                pokemonData.Id += $"-{pokemonData.RawIndex}";
                            }
                            else
                            {
                                pokemonData.Id = $"-{pokemonData.GenderEnitities.First().Gender}";
                            }
                        }
                        else
                        {
                            pokemonData.Id += $"-{pokemonData.SecondaryType}";
                        }
                    }
                    else
                    {
                        pokemonData.Id += $"-{pokemonData.PrimaryType}";
                    }

                    pokemonData.Id = pokemonData.Id.ToLower();

                    duplicatePokes.Add(pokemonData.Id);
                }
                else
                {
                    pokemonDatas.Add(pokemonData);
                }

                File.WriteAllText(Path.Combine(PokemonDataFolder, $"{pokemonData.Id}.json"), JsonConvert.SerializeObject(pokemonData, Formatting.Indented));
            }

            File.WriteAllLines(Path.Combine(PokemonDataFolder, "dupes.log"), duplicatePokes);
        }
    }
}
