using RealMode.Visualization.Pixels;
using RealMode.Visualization.Voxels;
using System;
using UnityEngine;

namespace RealMode.Visualization
{
    public class VisualizationService : MonoBehaviour
    {
        [SerializeReference] private VoxelVisualizer _visualization3DController = null!;
        [SerializeReference] private PixelVisualizer _visualization2DController = null!;

        public delegate void CurrentlyVisualizedEntryChangedOrModified(VisualizationService sender);

        public event CurrentlyVisualizedEntryChangedOrModified? OnEntryChangedOrModified;

        public Entry? CurrentEntry;

        private void Start()
        {
            /*
            var sizeX = 12;
            var sizeY = 14;
            var sizeZ = 16;
            var entry = new Entry3D()
            {
                IndexToNameDict = new System.Collections.Generic.Dictionary<int, string>() {
                    { 0, "nothing" },
                    { 1, "something" }
                },
                FriendlyName = "Test entry",
                Tags = new[] { "test" },
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

            SelectEntry(entry);
        */
            var entry = new Entry2D()
            {
                IndexToNameDict = new System.Collections.Generic.Dictionary<int, string>() {
                    { 0, "nothing" },
                    { 1, "something" }
                },
                FriendlyName = "Test entry",
                Tags = new[] { "test" },
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
            SelectEntry(entry);
        }

        public void SelectEntry(Entry? entry)
        {
            CurrentEntry = entry;
            OnEntryChangedOrModified?.Invoke(this);
            Visualize(CurrentEntry);
        }

        private void Visualize(Entry? entry)
        {
            if (entry == null)
            {
                _visualization2DController.Clear();
                _visualization3DController.Clear();
            }
            else if (entry.IsEntry2D())
            {
                _visualization3DController.Clear();
                _visualization2DController.VisualizeCurrentEntry();
            }
            else if (entry.IsEntry3D())
            {
                _visualization2DController.Clear();
                _visualization3DController.VisualizeCurrentEntry();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(entry), entry.GetType(), $"name: {entry.FriendlyName}");
            }
        }
    }
}