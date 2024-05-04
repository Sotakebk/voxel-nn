using System;

namespace RealMode.Data
{
    public static class DataHelper
    {
        public static (bool failed, string? message) Validate(EntryDTO entryDto)
        {
            if (entryDto.Dimensions.Length > 3)
                return (true, "Dimensions.Length is greater than 3.");
            if (entryDto.Dimensions.Length < 2)
                return (true, "Dimensions.Length is less than 2.");

            var totalLength = 1;
            foreach (var value in entryDto.Dimensions)
                totalLength *= value;

            if (totalLength != entryDto.Blocks.Length)
                return (true, $"Blocks.Length is not equal to count implied by Lengths ({totalLength}).");

            var maxBlockId = entryDto.BlockNames.Length - 1;
            foreach (var block in entryDto.Blocks)
            {
                if (maxBlockId < block)
                    return (true, $"BlockNames length is less than names array " +
                        $"(max block id is {maxBlockId} but there is a block ID: {block}).");
            }

            return (false, null);
        }

        public static Entry ToEntryObject(EntryDTO entryDTO)
        {
            if (entryDTO.Dimensions.Length == 2)
            {
                return new Entry2D(entryDTO);
            }
            else if (entryDTO.Dimensions.Length == 3)
            {
                return new Entry3D(entryDTO);
            }
            else
            {
                throw new ArgumentException($"Unexpected number of dimensions: {entryDTO.Dimensions.Length}.");
            }
        }
    }
}