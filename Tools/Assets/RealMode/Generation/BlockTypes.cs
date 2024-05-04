using System.Collections.Generic;

namespace RealMode.Generation
{
    public enum BlockType : int
    {
        Empty = 0,
    }

    public static class BlockTypeExtensions
    {
        public static string ToFriendlyString(this BlockType blockType)
        {
            return blockType switch
            {
                BlockType.Empty => "empty",
                _ => "UNKNOWN"
            };
        }

        public static int ToId(this BlockType blockType)
        {
            return (int)blockType;
        }
    }

    public static class BlockTypeHelper
    {
        public static Dictionary<int, string> NewCommonBlockTypeDictionary(params BlockType[] blocks)
        {
            var dict = new Dictionary<int, string>();
            foreach (var block in blocks)
            {
                dict.Add(block.ToId(), block.ToFriendlyString());
            }
            return dict;
        }
    }
}