using System;

namespace ViralPartyPrototypeLab.Data
{
    [Serializable]
    public sealed class PrototypeCatalogData
    {
        public string labName = "Viral Party Prototype Lab";
        public string buildVersion = "p00-foundation";
        public PrototypeEntry[] prototypes = new PrototypeEntry[0];
    }

    [Serializable]
    public sealed class PrototypeEntry
    {
        public string id = string.Empty;
        public string displayName = string.Empty;
        public string englishName = string.Empty;
        public string hook = string.Empty;
        public string priority = string.Empty;
        public string status = "Not Implemented";
        public bool implemented;
        public string sceneName = string.Empty;
        public string scenePath = string.Empty;
        public string notesPath = string.Empty;
    }
}
