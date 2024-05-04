using RealMode.Data;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public Entry2D(EntryDTO entryDTO)
        {
            FriendlyName = entryDTO.FriendlyName;
            Tags = (string[])entryDTO.Tags.Clone();
            Blocks = new int[entryDTO.Dimensions[0], entryDTO.Dimensions[1]];
            IndexToNameDict = new Dictionary<int, string>();
            for (int i = 0; i < entryDTO.BlockNames.Length; i++)
            {
                IndexToNameDict[i] = entryDTO.BlockNames[i];
            }
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    Blocks[x, y] = entryDTO.Blocks[x * SizeY + y];
                }
            }
        }

        public override EntryDTO ToDTO()
        {
            var blockNames = IndexToNameDict.Select(p => p.Value).ToList();
            var map = new Dictionary<int, int>();
            foreach (var pair in IndexToNameDict)
            {
                map.Add(pair.Key, blockNames.IndexOf(pair.Value));
            }

            var blocks = new int[SizeX * SizeY];
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    blocks[x * SizeY + y] = map[Blocks[x, y]];
                }
            }

            return new EntryDTO
            {
                FriendlyName = FriendlyName,
                Tags = (string[])Tags.Clone(),
                Dimensions = new int[] { SizeX, SizeY },
                BlockNames = blockNames.ToArray(),
                Blocks = blocks
            };
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

        public Entry3D(EntryDTO entryDTO)
        {
            FriendlyName = entryDTO.FriendlyName;
            Tags = (string[])entryDTO.Tags.Clone();
            Blocks = new int[entryDTO.Dimensions[0], entryDTO.Dimensions[1], entryDTO.Dimensions[2]];
            IndexToNameDict = new Dictionary<int, string>();
            for (int i = 0; i < entryDTO.BlockNames.Length; i++)
            {
                IndexToNameDict[i] = entryDTO.BlockNames[i];
            }
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        Blocks[x, y, z] = entryDTO.Blocks[x * SizeY * SizeZ + y * SizeZ + z];
                    }
                }
            }
        }
        public override EntryDTO ToDTO()
        {
            var blockNames = IndexToNameDict.Select(p => p.Value).ToList();
            var map = new Dictionary<int, int>();
            foreach (var pair in IndexToNameDict)
            {
                map.Add(pair.Key, blockNames.IndexOf(pair.Value));
            }

            var blocks = new int[SizeX * SizeY * SizeZ];
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    for (int z = 0; z < SizeZ; z++)
                    {
                        blocks[x * SizeY * SizeZ + y * SizeZ + z] = map[Blocks[x, y, z]];
                    }
                }
            }

            return new EntryDTO
            {
                FriendlyName = FriendlyName,
                Tags = (string[])Tags.Clone(),
                Dimensions = new int[] { SizeX, SizeY, SizeZ },
                BlockNames = blockNames.ToArray(),
                Blocks = blocks
            };
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