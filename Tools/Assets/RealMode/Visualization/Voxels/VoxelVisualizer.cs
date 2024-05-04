using System;
using System.ComponentModel;
using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    public class VoxelVisualizer : BaseVisualizer
    {
        [SerializeReference] private Material _solidMaterial = null!;
        [SerializeReference] private Material _transparentMaterial = null!;
        [SerializeReference] private Transform _baseCubeTransform = null!;

        private VoxelMeshElement[] _solidMeshElements = Array.Empty<VoxelMeshElement>();
        private VoxelMeshElement[] _transparentMeshElements = Array.Empty<VoxelMeshElement>();

        private WeakReference<Entry3D> _previouslyVisualizedEntry = new WeakReference<Entry3D>(null);

        public VoxelVisualizerSettings Settings { get; private set; } = new VoxelVisualizerSettings();

        protected override void Awake()
        {
            base.Awake();

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
            Settings.PropertyChanged += _settings_PropertyChanged;
        }

        protected override void Clear()
        {
            foreach (var element in _solidMeshElements)
            {
                element.ClearMesh();
            }
            foreach (var element in _transparentMeshElements)
            {
                element.ClearMesh();
            }
        }

        protected override bool CanVisualizeEntry(Entry entry)
        {
            return entry.IsEntry3D();
        }

        private bool ShouldResetDisplaySettings(Entry newEntry)
        {
            if (_previouslyVisualizedEntry.TryGetTarget(out var oldEntry))
            {
                if (newEntry == oldEntry)
                    return false;
            }
            return true;
        }

        protected override void VisualizeEntry(Entry entry)
        {
            var entry3d = entry.AsEntry3D();

            if (ShouldResetDisplaySettings(entry3d))
                ResetSettings(entry3d);

            var settings = Settings.ToCurrentSettings();

            var (solidMeshes, transparentMeshes) = PixelMeshingLogic.GenerateMesh(entry3d, _paletteService.CurrentPalette, settings);

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

            ScaleBaseCube(entry3d);
            Unhide();
            _previouslyVisualizedEntry = new WeakReference<Entry3D>(entry3d);
        }

        private void _settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MarkAsDirty();
        }

        private void ScaleBaseCube(Entry3D entry)
        {
            var x = entry.SizeX;
            var y = entry.SizeY;
            var z = entry.SizeZ;
            _baseCubeTransform.position = new Vector3(x, y, z) / 2f;
            _baseCubeTransform.localScale = new Vector3(x, y, z);
        }

        private void ResetSettings(Entry3D newEntry)
        {
            var previousSizeX = 0;
            var previousSizeY = 0;
            var previousSizeZ = 0;
            if (_previouslyVisualizedEntry.TryGetTarget(out var oldEntry))
            {
                previousSizeX = oldEntry.SizeX;
                previousSizeY = oldEntry.SizeY;
                previousSizeZ = oldEntry.SizeZ;
            }

            if (Settings.MinX != 0)
                Settings.MinX = Math.Max(0, Math.Min(Settings.MinX, newEntry.SizeX - 1));
            if (Settings.MaxX == previousSizeX)
                Settings.MaxX = newEntry.SizeX;
            else
                Settings.MaxX = Math.Min(Settings.MaxX, newEntry.SizeX);

            if (Settings.MinY != 0)
                Settings.MinY = Math.Max(0, Math.Min(Settings.MinY, newEntry.SizeY - 1));
            if (Settings.MaxY == previousSizeY)
                Settings.MaxY = newEntry.SizeY;
            else
                Settings.MaxY = Math.Min(Settings.MaxY, newEntry.SizeY);

            if (Settings.MinZ != 0)
                Settings.MinZ = Math.Max(0, Math.Min(Settings.MinZ, newEntry.SizeZ - 1));
            if (Settings.MaxZ == previousSizeZ)
                Settings.MaxZ = newEntry.SizeZ;
            else
                Settings.MaxZ = Math.Min(Settings.MaxZ, newEntry.SizeZ);
        }
    }
}