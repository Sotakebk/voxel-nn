using System;

namespace RealMode.Data
{
    [Serializable]
    public class EntryDTO
    {
        public string FriendlyName;
        public string[] Tags;
        public int[] Dimensions;
        public string[] BlockNames;
        public int[] Blocks;

        public EntryDTO()
        {
            FriendlyName = string.Empty;
            Tags = Array.Empty<string>();
            Dimensions = Array.Empty<int>();
            BlockNames = Array.Empty<string>();
            Blocks = Array.Empty<int>();
        }
    }
}