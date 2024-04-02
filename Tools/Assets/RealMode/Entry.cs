using RealMode.Data;

namespace RealMode
{
    public class Entry
    {
        public string FriendlyName { get; set; }
        public string[] Tags { get; set; }
        public int[] Dimensions { get; set; }
        public string[] BlockNames { get; set; }
        public int[] Blocks { get; set; }

        public Entry(EntryDTO dto)
        {
            FriendlyName = dto.FriendlyName;
            Tags = dto.Tags;
            Dimensions = dto.Dimensions;
            Blocks = dto.Blocks;
            BlockNames = dto.BlockNames;
        }

        public EntryDTO ToDTO()
        {
            return new EntryDTO()
            {
                FriendlyName = FriendlyName,
                Tags = Tags,
                Dimensions = Dimensions,
                Blocks = Blocks,
                BlockNames = BlockNames
            };
        }
    }
}
