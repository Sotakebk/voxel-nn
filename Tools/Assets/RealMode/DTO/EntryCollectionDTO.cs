using System;

namespace RealMode.Data

{
    [Serializable]
    public class EntryCollectionDTO
    {
        public string? Name;
        public EntryDTO[] Entries;

        public EntryCollectionDTO()
        {
            Name = null;
            Entries = Array.Empty<EntryDTO>();
        }
    }
}