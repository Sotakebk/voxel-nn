using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    [Serializable]
    public class VoxelVisualizerSettings : INotifyPropertyChanged
    {
        [SerializeField] private int _minX, _maxX, _minY, _maxY, _minZ, _maxZ;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? source = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(source));
        }

        public int MinX
        {
            get => _minX;
            set => SetProperty(ref _minX, value);
        }

        public int MaxX
        {
            get => _maxX;
            set => SetProperty(ref _maxX, value);
        }

        public int MinY
        {
            get => _minY;
            set => SetProperty(ref _minY, value);
        }

        public int MaxY
        {
            get => _maxY;
            set => SetProperty(ref _maxY, value);
        }

        public int MinZ
        {
            get => _minZ;
            set => SetProperty(ref _minZ, value);
        }

        public int MaxZ
        {
            get => _maxZ;
            set => SetProperty(ref _maxZ, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class VoxelVisualizer : MonoBehaviour
    {
        [SerializeReference] private Material _solidMaterial = null!;
        [SerializeReference] private Material _transparentMaterial = null!;
        [SerializeReference] private VoxelCameraController _cameraController = null!;
        [SerializeReference] private Transform _baseCubeTransform = null!;
        private VisualizationElement[] _solidVisualizationElements = Array.Empty<VisualizationElement>();
        private VisualizationElement[] _transparentVisualizationElements = Array.Empty<VisualizationElement>();
        [SerializeField] private VoxelVisualizerSettings _settings = new VoxelVisualizerSettings();

        public VoxelVisualizerSettings Settings => _settings;

        private void Awake()
        {
            _solidVisualizationElements = new VisualizationElement[6];
            _transparentVisualizationElements = new VisualizationElement[6];
            for (int i = 0; i < 6; i++)
            {
                _solidVisualizationElements[i] =
                    VisualizationElement.ConstructOnNewGameObject($"Solid {i}", _solidMaterial, transform);
            }

            for (int i = 0; i < 6; i++)
            {
                _transparentVisualizationElements[i] =
                    VisualizationElement.ConstructOnNewGameObject($"Transparent {i}", _transparentMaterial, transform);
            }
        }

        public void Clear()
        {
            foreach (var element in _solidVisualizationElements)
            {
                element.ClearMesh();
            }
            foreach (var element in _transparentVisualizationElements)
            {
                element.ClearMesh();
            }
            gameObject.SetActive(false);
        }

        public void Visualize(Entry3D entry, Palette palette)
        {
            Clear();

            var (solidMeshes, transparentMeshes) = VoxelMeshingLogic.GenerateMesh(entry, palette);

            for (int i = 0; i < 6; i++)
            {
                var mesh = solidMeshes[i];
                var element = _solidVisualizationElements[i];

                element.ApplyMesh(mesh);
            }

            for (int i = 0; i < 6; i++)
            {
                var mesh = transparentMeshes[i];
                var element = _transparentVisualizationElements[i];

                element.ApplyMesh(mesh);
            }

            gameObject.SetActive(true);
            _cameraController.HandleEntryOpened(entry);
            ScaleBaseCube(entry);
        }

        private void ScaleBaseCube(Entry3D entry)
        {
            var x = entry.SizeX;
            var y = entry.SizeY;
            var z = entry.SizeZ;
            _baseCubeTransform.position = new Vector3(x, y, z) / 2f;
            _baseCubeTransform.localScale = new Vector3(x, y, z);
        }
    }
}