// Uncomment to build data
// #define DATA

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DashingWanderer.Data;
using DashingWanderer.Data.Explorers.Pokedex;
using DashingWanderer.Exceptions;
using DashingWanderer.IO;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.VoiceNext;
using DSharpPlus.VoiceNext.Codec;
using Newtonsoft.Json;

namespace DashingWanderer
{
    public static class Program
    {
        private static DiscordClient discord;
        private static CommandsNextExtension commands;
        private static InteractivityExtension interactivity;
        private static VoiceNextExtension voice;

        public static async Task Main(string[] args)
        {
#if DATA
            DataBuilder.BuildExplorerPokemonData();
            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
#else
            try
            {
                using (WebClient client = new WebClient())
                {
                    Console.WriteLine("Downloading Data Repo...");
                    client.Headers.Add("user-agent", "DashingWanderer");
                    byte[] zipBytes = client.DownloadData("https://api.github.com/repos/JordanZeotni/Explorers-Data/zipball");

                    Console.WriteLine("Extracting...");
                    using (MemoryStream stream = new MemoryStream(zipBytes))
                    using (ZipArchive archive = new ZipArchive(stream))
                    {
                        archive.ExtractToDirectory(Globals.AppPath, true);
                    }

                    Console.WriteLine("Moving Data folder...");
                    string dataRepo = Directory.GetDirectories(Globals.AppPath).FirstOrDefault(e => Directory.CreateDirectory(e).Name.StartsWith("JordanZeotni"));

                    PathHelper.MoveDirectory(Path.Combine(Globals.AppPath, dataRepo, "Data"), Path.Combine(Globals.AppPath, "Data"));

                    Console.WriteLine("Deleting repo folder...");
                    Directory.Delete(dataRepo, true);

                    Console.WriteLine("Starting bot...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey(true);
                throw;
            }

            Globals.BotSettings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Path.Combine(Globals.AppPath, "config.json")));

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = Globals.BotSettings.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,
                AutoReconnect = true
            });

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new []{"d."},
                EnableMentionPrefix = true
            });

            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;

            interactivity = discord.UseInteractivity(new InteractivityConfiguration{ });

            voice = discord.UseVoiceNext(new VoiceNextConfiguration
            {
                VoiceApplication = VoiceApplication.Music
            });

            discord.MessageCreated += Discord_MessageCreated;

            await discord.ConnectAsync();

            DataBuilder.GetExplorersData();

            await Task.Delay(-1);
#endif
        }

        private static async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception is DiscordMessageException outputException)
            {
                await e.Context.RespondAsync(outputException.Message, outputException.IsTTS, outputException.Embed);

                e.Context.Client.DebugLogger.LogMessage(LogLevel.Warning, 
                    Assembly.GetExecutingAssembly().FullName, 
                    $"DiscordMessageException output to channel {e.Context.Channel.Name}. Arguments: {string.Join(", ", e.Command.Arguments)}", 
                    e.Context.Message.Timestamp.DateTime);
            }
            else
            {
                e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, Assembly.GetExecutingAssembly().FullName, e.Exception.ToString(), DateTime.Now);
            }
        }

        private static Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            return Task.CompletedTask;
        }

        private static async Task Discord_MessageCreated(MessageCreateEventArgs e)
        {
        }
    }
}
