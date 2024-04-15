using RealMode.Data;
using System.Collections.Generic;

namespace RealMode
{
    public class EntryCollection
    {
        private string? _fileName;
        private Entry[] _entries;
        private string Name;

        public IEnumerable<Entry> Entries => _entries;

        public EntryCollection(EntryCollectionDTO dto, string? fileName)
        {
            Name = dto.Name;
            _fileName = fileName;
        }
    }
}