using RealMode.Presentation.General;
using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [RequireComponent(typeof(Camera))]
    public class VoxelSelector : MonoBehaviour
    {
        [SerializeReference] TargetVoxelPresenter _targetVoxelPresenter = null!;
        [SerializeReference] Transform _selectorCube = null!;
        [SerializeReference] VisualizationService _visualizationService = null!;

        private Camera _camera = null!;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var info))
            {
                var point = info.point - (info.normal / 2f);
                point = new Vector3(Mathf.Floor(point.x), Mathf.Floor(point.y), Mathf.Floor(point.z));

                var currentEntry = _visualizationService.CurrentEntry?.AsEntry3D();
                if (currentEntry == null)
                    throw new System.Exception("Hit, but no entry loaded?");

                var index = currentEntry.BlockOrNothing((int)point.x, (int)point.y, (int)point.z);
                if (index == null)
                    throw new System.Exception("Hit voxel index out of range?");
                var name = currentEntry.IndexToNameDict[index.Value];

                _targetVoxelPresenter.UpdateTargetElement(name, point);
                _selectorCube.position = point + new Vector3(0.5f, 0.5f, 0.5f);
                _selectorCube.gameObject.SetActive(true);
            }
            else
            {
                _targetVoxelPresenter.ClearTargetElement();
                _selectorCube.gameObject.SetActive(false);
            }
        }
    }
}