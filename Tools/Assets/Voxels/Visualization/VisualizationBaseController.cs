using UnityEngine;
using Voxels.Data;

namespace Voxels.Visualization
{
    public abstract class VisualizationBaseController : MonoBehaviour
    {
        public abstract void Visualize(Entry entry);

        public abstract void Clear();
    }
}
