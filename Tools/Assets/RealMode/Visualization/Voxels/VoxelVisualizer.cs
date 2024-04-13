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

        public CurrentVisualizationSettings ToCurrentSettings()
        {
            return new CurrentVisualizationSettings()
            {
                MinX = _minX,
                MaxX = _maxX,
                MinY = _minY,
                MaxY = _maxY,
                MinZ = _minZ,
                MaxZ = _maxZ
            };
        }
    }

    public struct CurrentVisualizationSettings
    {
        public int MinX, MaxX, MinY, MaxY, MinZ, MaxZ;
    }

    public class VoxelVisualizer : MonoBehaviour
    {
        [SerializeReference] private VisualizationService _visualizationService = null!;
        [SerializeReference] private PaletteManager paletteManager = null!;
        [SerializeReference] private Material _solidMaterial = null!;
        [SerializeReference] private Material _transparentMaterial = null!;
        [SerializeReference] private Transform _baseCubeTransform = null!;
        private VoxelMeshElement[] _solidMeshElements = Array.Empty<VoxelMeshElement>();
        private VoxelMeshElement[] _transparentMeshElements = Array.Empty<VoxelMeshElement>();
        [SerializeField] private VoxelVisualizerSettings _settings = new VoxelVisualizerSettings();
        private bool _shouldRedrawModel;

        public VoxelVisualizerSettings Settings => _settings;

        private void Awake()
        {
            _solidMeshElements = new VoxelMeshElement[6];
            _transparentMeshElements = new VoxelMeshElement[6];
            for (int i = 0; i < 6; i++)
            {
                _solidMeshElements[i] =
                    VoxelMeshElement.ConstructOnNewGameObject($"Solid {i}", _solidMaterial, transform);
            }

            for (int i = 0; i < 6; i++)
            {
                _transparentMeshElements[i] =
                    VoxelMeshElement.ConstructOnNewGameObject($"Transparent {i}", _transparentMaterial, transform);
            }
            _settings.PropertyChanged += _settings_PropertyChanged;
        }

        public void Clear()
        {
            foreach (var element in _solidMeshElements)
            {
                element.ClearMesh();
            }
            foreach (var element in _transparentMeshElements)
            {
                element.ClearMesh();
            }
            gameObject.SetActive(false);
        }

        public void Visualize(Entry3D entry, Palette palette)
        {
            Clear();
            var settings = _settings.ToCurrentSettings();
            var (solidMeshes, transparentMeshes) = PixelMeshingLogic.GenerateMesh(entry, palette, settings);

            for (int i = 0; i < 6; i++)
            {
                var mesh = solidMeshes[i];
                var element = _solidMeshElements[i];

                element.ApplyMesh(mesh);
            }

            for (int i = 0; i < 6; i++)
            {
                var mesh = transparentMeshes[i];
                var element = _transparentMeshElements[i];

                element.ApplyMesh(mesh);
            }

            gameObject.SetActive(true);
            ScaleBaseCube(entry);
        }

        private void _settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _shouldRedrawModel = true;
        }

        private void ScaleBaseCube(Entry3D entry)
        {
            var x = entry.SizeX;
            var y = entry.SizeY;
            var z = entry.SizeZ;
            _baseCubeTransform.position = new Vector3(x, y, z) / 2f;
            _baseCubeTransform.localScale = new Vector3(x, y, z);
        }

        public void VisualizeCurrentEntry()
        {
            _shouldRedrawModel = true;
            var currEntry = _visualizationService.CurrentEntry;
            if (currEntry.IsEntry3D())
            {
                var entry = _visualizationService.CurrentEntry.AsEntry3D();
                _settings.MinX = 0;
                _settings.MaxX = entry.SizeX;
                _settings.MinY = 0;
                _settings.MaxY = entry.SizeY;
                _settings.MinZ = 0;
                _settings.MaxZ = entry.SizeZ;
            }
        }

        private void Update()
        {
            if (_shouldRedrawModel)
            {
                _shouldRedrawModel = false;
                var currEntry = _visualizationService.CurrentEntry;
                if (currEntry.IsEntry3D())
                {
                    Visualize(currEntry.AsEntry3D(), paletteManager.CurrentPalette);
                }
                else
                {
                    Clear();
                }
            }
        }
    }
}