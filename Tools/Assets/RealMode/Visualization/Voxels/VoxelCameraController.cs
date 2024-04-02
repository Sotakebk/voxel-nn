using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [RequireComponent(typeof(VoxelCameraController))]
    public class VoxelCameraController : MonoBehaviour
    {
        private Vector3 _targetCenter;
        private Vector3 _lookingAngles;
        private float _distance;

        private void Update()
        {
            transform.position = _targetCenter;
        }

        public void HandleEntryOpened(Entry entry)
        {

        }
    }
}
