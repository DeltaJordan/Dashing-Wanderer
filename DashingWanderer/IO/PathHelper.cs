using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DashingWanderer.IO
{
    public static class PathHelper
    {
        public static void MoveDirectory(string source, string target)
        {
            string sourcePath = source.TrimEnd('\\', ' ');
            string targetPath = target.TrimEnd('\\', ' ');
            IEnumerable<IGrouping<string, string>> files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                .GroupBy(Path.GetDirectoryName);
            foreach (IGrouping<string, string> folder in files)
            {
                string targetFolder = folder.Key.Replace(sourcePath, targetPath);
                Directory.CreateDirectory(targetFolder);
                foreach (string file in folder)
                {
                    string targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    if (File.Exists(targetFile)) File.Delete(targetFile);
                    File.Move(file, targetFile);
                }
            }
            Directory.Delete(source, true);
        }
    }
}
