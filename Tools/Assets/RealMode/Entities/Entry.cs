using RealMode.Data;
using System;
using System.Collections.Generic;

namespace RealMode
{
    [Serializable]
    public abstract class Entry
    {
        public string FriendlyName;
        public string[] Tags;
        public Dictionary<int, string> IndexToNameDict;

        public Entry()
        {
            FriendlyName = "Default friendly name";
            Tags = Array.Empty<string>();
            IndexToNameDict = new Dictionary<int, string>();
        }

        public abstract EntryDTO ToDTO();
    }

    [Serializable]
    public sealed class Entry2D : Entry
    {
        public int SizeX => Blocks.GetLength(0);
        public int SizeY => Blocks.GetLength(1);

        public int[,] Blocks;

        public Entry2D()
        {
            Blocks = new int[0, 0];
        }

        public override EntryDTO ToDTO()
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public sealed class Entry3D : Entry
    {
        public int SizeX => Blocks.GetLength(0);
        public int SizeY => Blocks.GetLength(1);
        public int SizeZ => Blocks.GetLength(2);

        public int[,,] Blocks;

        public Entry3D()
        {
            Blocks = new int[0, 0, 0];
        }

        public override EntryDTO ToDTO()
        {
            throw new NotImplementedException();
        }
    }

    public static class IEntryExtensions
    {
        public static bool IsEntry2D(this Entry entry)
        {
            return entry as Entry2D != null;
        }

        public static bool IsEntry3D(this Entry entry)
        {
            return entry as Entry3D != null;
        }

        public static Entry2D AsEntry2D(this Entry entry)
        {
            return entry as Entry2D ?? throw new InvalidOperationException();
        }

        public static Entry3D AsEntry3D(this Entry entry)
        {
            return entry as Entry3D ?? throw new InvalidOperationException();
        }

        public static int? BlockOrNothing(this Entry2D entry, int x, int y)
        {
            if (x < 0 || x >= entry.SizeX)
                return null;
            if (y < 0 || y >= entry.SizeY)
                return null;

            return entry.Blocks[x, y];
        }

        public static int? BlockOrNothing(this Entry3D entry, int x, int y, int z)
        {
            if (x < 0 || x >= entry.SizeX)
                return null;
            if (y < 0 || y >= entry.SizeY)
                return null;
            if (z < 0 || z >= entry.SizeZ)
                return null;

            return entry.Blocks[x, y, z];
        }
    }
}