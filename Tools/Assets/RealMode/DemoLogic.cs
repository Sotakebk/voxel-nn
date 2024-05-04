using UnityEngine;

namespace RealMode
{
    public class DemoLogic : MonoBehaviour
    {
        [SerializeReference]
        private LoadedEntriesService _loadedEntriesService;

        private void AddEntry(Entry entry)
        {
            _loadedEntriesService.AddEntry(entry);
        }

        private void GenerateEntry3D()
        {
            var sizeX = 12;
            var sizeY = 14;
            var sizeZ = 16;
            var entry = new Entry3D()
            {
                IndexToNameDict = new System.Collections.Generic.Dictionary<int, string>() {
                    { 0, "nothing" },
                    { 1, "something" }
            },
                FriendlyName = $"Test entry 3d {UnityEngine.Random.Range(0, 1000)}",
                Tags = new[] { "tag1", "tag2", "tag3", "tag4" },
                Blocks = new int[12, 14, 16]
            };
            for (int x = 0; x < sizeX; x++)
            {
                for (int z = 0; z < sizeZ; z++)
                {
                    var height = 4 + UnityEngine.Random.Range(-3, 3) + (x + z) / 2;

                    for (int y = 0; y < sizeY; y++)
                    {
                        entry.Blocks[x, y, z] = y < height ? 1 : 0;
                    }
                }
            }

            AddEntry(entry);
        }

        private void GenerateEntry2D()
        {
            var entry = new Entry2D()
            {
                IndexToNameDict = new System.Collections.Generic.Dictionary<int, string>() {
                                { 0, "nothing" },
                                { 1, "something" }
                            },
                FriendlyName = $"Test entry 2d {UnityEngine.Random.Range(0, 1000)}",
                Tags = new[] { "tag1", "tag2", "tag3", "tag4" },
                Blocks = new int[32, 32]
            };
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)

                {
                    var xoff = x - 16;
                    var yoff = y - 16;
                    entry.Blocks[x, y] = (32 - (xoff * xoff + yoff * yoff)) > 8 ? 1 : 0;
                }
            }
            AddEntry(entry);
        }

        private void Start()
        {
            for (int i = 0; i < 10; i++)
                GenerateEntry3D();
            for (int i = 0; i < 10; i++)
                GenerateEntry2D();
        }
    }
}