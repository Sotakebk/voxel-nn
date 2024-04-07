using RealMode.Visualization.Voxels;
using System;
using UnityEngine;

namespace RealMode.Visualization
{
    public class VisualizationRoot : MonoBehaviour
    {
        [SerializeReference] private PaletteManager paletteManager = null!;
        [SerializeReference] private VoxelVisualizer _visualization3DController = null!;
        [SerializeReference] private ActiveEntryService SelectedEntryService = null!;

        private void Awake()
        {
            SelectedEntryService.OnEntryChanged += SelectedEntryService_OnEntryChanged;
        }

        private void SelectedEntryService_OnEntryChanged(ActiveEntryService sender)
        {
            Visualize(sender.CurrentEntry);
        }

        public void Visualize(Entry? entry)
        {
            if (entry == null)
            {
                _visualization3DController.Clear();
            }
            else if (entry.IsEntry2D())
            {
                //Visualize2D(entry.AsEntry2D());
            }
            else if (entry.IsEntry3D())
            {
                Visualize3D(entry.AsEntry3D());
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(entry), entry.GetType(), $"name: {entry.FriendlyName}");
            }
        }

        private void Visualize3D(Entry3D entry)
        {
            _visualization3DController.Clear();
            _visualization3DController.Visualize(entry, paletteManager.CurrentPalette);
        }
    }
}