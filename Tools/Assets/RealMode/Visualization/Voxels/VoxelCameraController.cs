using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [RequireComponent(typeof(Camera))]
    public class VoxelCameraController : BaseCameraController
    {
        private enum CameraMode
        {
            Perspective,
            Orthographic
        }

        [SerializeReference] private SelectedEntryService _selectedEntryService = null!;
        private CameraMode _currentCameraMode;
        [SerializeField] private float _perspectiveFov = 80;
        [SerializeField] private float _mouseSensitivity = 4;
        [SerializeField] private float _distance = 30;
        private float _horizontal;
        private float _vertical;
        private bool _isDragging = false;
        private Plane _draggingPlane;
        private Vector3 _dragBeginPosition;
        private Camera _camera = null!;
        private Vector3 _targetCenter;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            bool anythingChanged = false;
            if (Input.mouseScrollDelta.y != 0)
            {
                _distance = Mathf.Clamp(_distance + Input.mouseScrollDelta.y, 0.1f, 100f);
                anythingChanged = true;
            }

            if (_isDragging && (Input.GetMouseButtonUp(2) || Input.GetKeyUp(KeyCode.LeftControl)))
            {
                _isDragging = false;
            }

            bool isRotating = Input.GetMouseButton(1);

            if (!isRotating)
            {
                if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.LeftControl))
                {
                    _isDragging = true;
                    (_dragBeginPosition, _draggingPlane) = GetPositionAndPlaneForDragging();
                }
            }
            else
            {
                _isDragging = false;
            }

            // holding right click
            if (Input.GetMouseButton(1))
            {
                _horizontal += Input.GetAxis("Mouse X") * _mouseSensitivity;
                _vertical -= Input.GetAxis("Mouse Y") * _mouseSensitivity;
                _horizontal %= 360f;
                _vertical = Mathf.Clamp(_vertical, -90, 90);

                anythingChanged = true;
            }
            else if (_isDragging)
            {
                var ray = _camera.ScreenPointToRay(Input.mousePosition);
                if (_draggingPlane.Raycast(ray, out float enter))
                {
                    var delta = _dragBeginPosition - ray.GetPoint(enter);
                    _targetCenter += delta;
                    anythingChanged = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                if (_currentCameraMode == CameraMode.Perspective)
                {
                    SetCameraMode(CameraMode.Orthographic);
                }
                else
                {
                    SetCameraMode(CameraMode.Perspective);
                }
                anythingChanged = true;
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

        private void SetCameraMode(CameraMode mode)
        {
            _currentCameraMode = mode;
        }

        public override void ResetPositioning()
        {
            var currentEntry = _selectedEntryService.CurrentEntry;
            if (currentEntry.IsEntry3D())
            {
                var entry = _selectedEntryService.CurrentEntry as Entry3D ?? throw new System.Exception();
                var x = entry.SizeX;
                var y = entry.SizeY;
                var z = entry.SizeZ;
                _targetCenter = new Vector3(x + 1, y + 1, z + 1) / 2f;
                _horizontal = 45f;
                _vertical = 30f;
                _distance = new Vector3(x, y, z).magnitude * 1.2f;
                SetCameraMode(CameraMode.Perspective);
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            if (_currentCameraMode == CameraMode.Perspective)
            {
                var lookingAngles = new Vector3(_vertical, _horizontal, 0);
                var forward = Quaternion.Euler(lookingAngles) * Vector3.forward;

                transform.rotation = Quaternion.Euler(lookingAngles);
                transform.position = _targetCenter + (-forward * _distance);
                _camera.orthographic = false;
                _camera.fieldOfView = _perspectiveFov;
            }
            else
            {
                var lookingAngles = new Vector3(_vertical, _horizontal, 0);
                var forward = Quaternion.Euler(lookingAngles) * Vector3.forward;

                transform.rotation = Quaternion.Euler(lookingAngles);
                transform.position = _targetCenter + (-forward * 400);
                _camera.orthographic = true;
                _camera.orthographicSize = _distance;
            }
        }

        public (Vector3 position, Plane plane) GetPositionAndPlaneForDragging()
        {
            var lookingAngles = new Vector3(_vertical, _horizontal, 0);
            var forward = Quaternion.Euler(lookingAngles) * Vector3.forward;

            var ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hitInfo))
            {
                var pos = hitInfo.point;
                var hitPlane = new Plane(forward, pos);
                return (pos, hitPlane);
            }

            var plane = new Plane(forward, _targetCenter);
            plane.Raycast(ray, out float enter);
            var position = ray.GetPoint(enter);
            return (position, plane);
        }
    }
}