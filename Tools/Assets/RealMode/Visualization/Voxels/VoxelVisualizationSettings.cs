using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RealMode.Visualization.Voxels
{
    public class VoxelVisualizerSettings : INotifyPropertyChanged
    {
        private int _minX, _maxX, _minY, _maxY, _minZ, _maxZ;

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
}