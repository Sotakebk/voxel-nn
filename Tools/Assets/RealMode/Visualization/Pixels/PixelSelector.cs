using RealMode.Presentation.General;
using UnityEngine;

namespace RealMode.Visualization.Pixels
{
    [RequireComponent(typeof(Camera))]
    public class PixelSelector : MonoBehaviour
    {
        [SerializeReference] private TargetVoxelPresenter _targetVoxelPresenter = null!;
        [SerializeReference] private Transform _selectorPlane = null!;
        [SerializeReference] private SelectedEntryService _selectedEntryService = null!;

        private Camera _camera = null!;
        private Plane _plane;

        private void Awake()
        {
            _plane = new Plane(Vector3.back, 0);
            _camera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            var currentEntry = _selectedEntryService.CurrentEntry;
            if (currentEntry.IsEntry2D())
            {
                var entry2d = currentEntry.AsEntry2D();

                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                _plane.Raycast(ray, out float enter);
                var point = ray.GetPoint(enter);
                var x = Mathf.FloorToInt(point.x);
                var y = Mathf.FloorToInt(point.y);

                var index = entry2d.BlockOrNothing(x, y);
                if (index == null)
                {
                    ClearTarget();
                }
                else
                {
                    var name = currentEntry.IndexToNameDict[index.Value];
                    SetTarget(name, x, y);
                }
            }
            else
            {
                ClearTarget();
            }
        }

        private void ClearTarget()
        {
            _targetVoxelPresenter.ClearTargetElement();
            _selectorPlane.gameObject.SetActive(false);
        }

        private void SetTarget(string name, int x, int y)
        {
            _targetVoxelPresenter.UpdateTargetElement(name, new Vector2(x, y));
            _selectorPlane.position = new Vector3(x + 0.5f, y + 0.5f, 0);
            _selectorPlane.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            _targetVoxelPresenter.ClearTargetElement();
        }
    }
}