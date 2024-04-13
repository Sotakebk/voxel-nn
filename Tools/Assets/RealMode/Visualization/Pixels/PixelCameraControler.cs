using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [RequireComponent(typeof(Camera))]
    public class PixelCameraControler : MonoBehaviour
    {
        [SerializeReference] private VisualizationService _visualizationService = null!;
        [SerializeField] private float _zoom = 30;
        private bool _isDragging = false;
        private Vector2 _targetPosition;
        private Vector2 _dragBeginPosition;
        private Plane _plane;
        private Camera _camera = null!;

        private void Awake()
        {
            _plane = new Plane(Vector3.back, 0);
            _camera = GetComponent<Camera>();
            _visualizationService.OnEntryChangedOrModified += _visualizationService_OnEntryChangedOrModified;
        }

        private void _visualizationService_OnEntryChangedOrModified(VisualizationService sender)
        {
            ResetPositioning();
        }

        private void Update()
        {
            bool anythingChanged = false;
            if (Input.mouseScrollDelta.y != 0)
            {
                _zoom = Mathf.Clamp(_zoom + Input.mouseScrollDelta.y, 0.1f, 100f);
                anythingChanged = true;
            }

            if (_isDragging && (Input.GetMouseButtonUp(2) || Input.GetKeyUp(KeyCode.LeftControl)))
            {
                _isDragging = false;
            }

            if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.LeftControl))
            {
                _isDragging = true;
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                _plane.Raycast(ray, out float enter);
                _dragBeginPosition = ray.GetPoint(enter);
            }

            if (_isDragging)
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (_plane.Raycast(ray, out float enter))
                {
                    var delta = _dragBeginPosition - (Vector2)ray.GetPoint(enter);
                    _targetPosition += delta;
                    anythingChanged = true;
                }
            }

            if (anythingChanged)
            {
                UpdatePosition();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPositioning();
            }
        }

        private void ResetPositioning()
        {
            var currentEntry = _visualizationService.CurrentEntry;
            if (currentEntry.IsEntry2D())
            {
                var entry = _visualizationService.CurrentEntry as Entry2D ?? throw new System.Exception();
                var x = entry.SizeX;
                var y = entry.SizeY;
                _targetPosition = new Vector2(x + 1, y + 1) / 2f;
                _zoom = x * Screen.height / Screen.width * 0.5f + 0.5f;
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            transform.position = new Vector3(_targetPosition.x, _targetPosition.y, -10);
            _camera.orthographic = true;
            _camera.orthographicSize = _zoom;
        }
    }
}
