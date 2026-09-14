using System;

namespace TechInstaller
{
    public class AppItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string DownloadUrl { get; set; }
        public string SilentArgs { get; set; }
        public string CacheFileName { get; set; }
        public string PresetTags { get; set; }
        public int EstimatedSizeMB { get; set; }
        public string SpecialAction { get; set; }
        public bool IsSelected { get; set; }
        public string Status { get; set; }
        public string LocalFilePath { get; set; }
        public int ExitCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsCached { get; set; }

        public AppItem()
        {
            Status = "Pending";
            SilentArgs = "";
            SpecialAction = "";
            PresetTags = "";
            ErrorMessage = "";
        }

        public bool MatchesPreset(string preset)
        {
            if (string.IsNullOrEmpty(PresetTags)) return false;
            string[] tags = PresetTags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string t in tags)
            {
                if (string.Equals(t.Trim(), preset, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Name, Category);
        }
    }
}
