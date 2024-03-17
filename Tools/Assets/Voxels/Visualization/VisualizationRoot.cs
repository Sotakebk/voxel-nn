using System;
using UnityEngine;
using Voxels.Data;

namespace Voxels.Visualization
{
    public class VisualizationRoot : MonoBehaviour
    {
        [SerializeReference]
        private Visualization2DController _visualization2DController = null!;

        [SerializeReference]
        private Visualization3DController _visualization3DController = null!;

        public void Visualize(Entry entry)
        {
            switch (entry.Dimensions.Length)
            {
                case 2:
                    Visualize2D(entry);
                    break;

                case 3:
                    Visualize3D(entry);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entry), "Only '2' and '3' are valid dimension numbers.");
            }
        }

        private void Visualize2D(Entry entry)
        {
            _visualization2DController.Clear();
            _visualization3DController.Clear();
            _visualization2DController.Visualize(entry);
        }

        private void Visualize3D(Entry entry)
        {
            _visualization2DController.Clear();
            _visualization3DController.Clear();
            _visualization3DController.Visualize(entry);
        }
    }
}
