using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace DashingWanderer.Exceptions
{
    /// <inheritdoc />
    /// <summary>
    /// Exception that is output for guild memebers to see.
    /// </summary>
    public class DiscordMessageException : Exception
    {
        public bool IsTTS { get; }
        public DiscordEmbed Embed { get; }

        public DiscordMessageException(string message = null, bool isTTS = false, DiscordEmbed embed = null) : base(message)
        {
            this.IsTTS = isTTS;
            this.Embed = embed;
        }
    }
}
