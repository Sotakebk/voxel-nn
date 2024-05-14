using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [RequireComponent(typeof(Camera))]
    public class PixelCameraController : BaseCameraController
    {
        [SerializeReference] private SelectedEntryService _selectedEntryService = null!;
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

            if (ShortcutService.CanUseShortcuts && Input.GetKeyDown(KeyCode.R))
            {
                ResetPositioning();
            }
        }

        public override void ResetPositioning()
        {
            var currentEntry = _selectedEntryService.CurrentEntry;
            if (currentEntry.IsEntry2D())
            {
                var entry = _selectedEntryService.CurrentEntry as Entry2D ?? throw new System.Exception();
                var x = entry.SizeX;
                var y = entry.SizeY;
                _targetPosition = new Vector2(x, y) / 2f;
                _zoom = y * Screen.height / (float)Screen.width * 0.5f + 2f;
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