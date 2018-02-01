using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;

namespace DashingWanderer.Extensions
{
    public static class DiscordEmbedBuilderExtensions
    {
        /// <summary>
        /// Adds field to this embed, converting the parameter "<see cref="object"/> value" to a string.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="name">Name of the field to add.</param>
        /// <param name="value">Value of the field to add. Calls object.ToString().</param>
        /// <param name="inLine">Whether the field is to be inline or not.</param>
        /// <returns></returns>
        public static DiscordEmbedBuilder AddField(this DiscordEmbedBuilder builder, string name, object value, bool inLine = false)
        {
            return builder.AddField(name, value.ToString(), inLine);
        }
    }
}
