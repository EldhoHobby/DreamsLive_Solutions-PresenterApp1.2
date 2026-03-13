using System;
using System.IO;

namespace DreamsLive_Solutions_PresenterApp1
{
    public class DatabaseFileInfo
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public string RelativePath { get; set; }
        public string Extension { get; set; }

        public DatabaseFileInfo() { }

        public DatabaseFileInfo(string fullPath, string databaseRoot)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
            Extension = Path.GetExtension(fullPath).ToLowerInvariant();

            if (!string.IsNullOrEmpty(databaseRoot) && fullPath.StartsWith(databaseRoot, StringComparison.OrdinalIgnoreCase))
            {
                RelativePath = fullPath.Substring(databaseRoot.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            else
            {
                RelativePath = Name;
            }
        }
    }
}
