using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace DashingWanderer
{
    public static class Globals
    {
        [ThreadStatic] private static Random local;

        public static Random Random => local ?? (local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));

        public static Settings BotSettings { get; set; }

        public static readonly string AppPath = Directory.GetParent(new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath).FullName;
    }
}
