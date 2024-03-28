using System;

namespace RealMode.Data
{
    [Serializable]
    public class Entry
    {
        private readonly string DefaultName = "new entry";

        public string FriendlyName { get; set; }

        public string[] Tags { get; set; }

        public int[] Dimensions { get; set; }

        public string[] BlockNames { get; set; }

        public int[] Blocks { get; set; }

        public (bool failed, string? message) Validate()
        {
            if (Dimensions.Length > 3)
                return (true, "Dimensions.Length is greater than 3.");
            if (Dimensions.Length > 2)
                return (true, "Dimensions.Length is lesser than 2.");

            var totalLength = 1;
            foreach (var value in Dimensions)
                totalLength *= value;

            if (totalLength != Blocks.Length)
                return (true, $"Blocks.Length is not equal to count implied by Lengths ({totalLength}).");

            var maxBlockId = BlockNames.Length - 1;
            foreach (var block in Blocks)
            {
                if (maxBlockId <= block)
                    return (true, $"BlockNames length is less than names array " +
                        $"(max block id is {maxBlockId} but there is a block ID: {block}).");
            }

            return (false, null);
        }

        public Entry()
        {
            FriendlyName = DefaultName;
            Tags = Array.Empty<string>();
            Dimensions = Array.Empty<int>();
            BlockNames = Array.Empty<string>();
            Blocks = Array.Empty<int>();
        }
    }
}