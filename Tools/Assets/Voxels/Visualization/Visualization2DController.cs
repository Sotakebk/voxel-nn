using Voxels.Data;

namespace Voxels.Visualization
{
    public class Visualization2DController : VisualizationBaseController
    {
        public override void Clear()
        {
            gameObject.SetActive(false);
        }

        public override void Visualize(Entry entry)
        {
            gameObject.SetActive(true);
        }
    }
}
