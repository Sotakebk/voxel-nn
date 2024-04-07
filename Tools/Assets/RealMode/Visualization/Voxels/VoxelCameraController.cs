using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [RequireComponent(typeof(Camera))]
    public class VoxelCameraController : MonoBehaviour
    {
        [SerializeField] private Vector3 _targetCenter;
        [SerializeField] private float _distance = 30;
        [SerializeField] private float _horizontal;
        [SerializeField] private float _vertical;
        [SerializeField] private float _mouseSensitivity = 4;
        [SerializeField] private bool _isDragging = false;
        [SerializeField] private Plane _draggingPlane;
        [SerializeField] private Vector3 _dragBeginPosition;
        private Camera _camera = null!;

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

            if (anythingChanged)
            {
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            var lookingAngles = new Vector3(_vertical, _horizontal, 0);
            var forward = Quaternion.Euler(lookingAngles) * Vector3.forward;

            transform.rotation = Quaternion.Euler(lookingAngles);
            transform.position = _targetCenter + (-forward * _distance);
        }

        public void HandleEntryOpened(Entry3D entry)
        {
            var x = entry.SizeX;
            var y = entry.SizeY;
            var z = entry.SizeZ;
            _targetCenter = new Vector3(x + 1, y + 1, z + 1) / 2f;
            _horizontal = 45f;
            _vertical = 30f;
            _distance = new Vector3(x, y, z).magnitude * 1.2f;
            UpdatePosition();
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
