using System;
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
        public static Dictionary<int, string> NewCommonBlockTypeDictionary()
        {
            var dict = new Dictionary<int, string>();
            foreach (var value in (BlockType[])Enum.GetValues(typeof(BlockType)))
            {
                dict.Add(value.ToId(), value.ToFriendlyString());
            }
            return dict;
        }
    }
}