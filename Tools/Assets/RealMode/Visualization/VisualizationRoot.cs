using RealMode.Data;
using RealMode.Visualization.Voxels;
using System;
using UnityEngine;

namespace RealMode.Visualization
{
    public class VisualizationRoot : MonoBehaviour
    {
        [SerializeReference]
        private PaletteManager paletteManager = null!;

        [SerializeReference]
        private VoxelVisualizer _visualization3DController = null!;


        [SerializeReference]
        private EntryDTO _currentlyVisualizedEntry = null!;

        private void Start()
        {
            var entry = new EntryDTO()
            {
                BlockNames = new[] { "nothing", "something", "glass" },
                Dimensions = new[] { 16, 16, 16 },
                FriendlyName = "Test entry",
                Tags = new[] { "test" },
                Blocks = new int[16 * 16 * 16]
            };
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        entry.Blocks[x * 16 * 16 + y * 16 + z] = UnityEngine.Random.Range(0, 3);
                    }
                }
            }
            var (validationFailed, message) = entry.Validate();
            if (!validationFailed)
                throw new Exception(message);
            Visualize(new Entry(entry));
            _currentlyVisualizedEntry = entry;
        }

        public void Visualize(Entry entry)
        {
            switch (entry.Dimensions.Length)
            {
                case 3:
                    Visualize3D(entry);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), "Only three dimensions are supported.");
            }
        }

        private void Visualize3D(Entry entry)
        {
            _visualization3DController.Clear();
            _visualization3DController.Visualize(entry, paletteManager.CurrentPalette);
        }
    }
}
