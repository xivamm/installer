using System;

namespace TechInstaller
{
    public class CloudAppItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string DriveUrl { get; set; }
        public string EstimatedSize { get; set; }
        public string Version { get; set; }
        public string Notes { get; set; }

        public CloudAppItem()
        {
            EstimatedSize = "N/A";
            Version = "Latest";
            Notes = "";
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", Name, Category);
        }
    }
}
