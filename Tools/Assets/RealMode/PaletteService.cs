using RealMode.Data;
using UnityEngine;

namespace RealMode.Visualization
{
    public class PaletteService : MonoBehaviour
    {
        public delegate void PaletteChangedEventHandler(PaletteService sender);

        public event PaletteChangedEventHandler? OnPaletteChanged;

        private bool _shouldTriggerEvent;
        private readonly object _lock = new object();

        [SerializeReference] private FilesService _filesService;

        private void Awake()
        {
            _filesService.OnDatasetPathUpdated += _filesService_OnDatasetPathUpdated;
        }

        private void _filesService_OnDatasetPathUpdated(FilesService sender)
        {
            if (_currentPalette == null)
            {
                var newPalette = sender.LoadPalette();
                if (newPalette != null)
                {
                    lock (_lock)
                    {
                        _currentPalette = newPalette;
                        _shouldTriggerEvent = true;
                    }
                }
            }
        }

        [SerializeReference]
        private Palette _currentPalette = null!;

        public Palette CurrentPalette
        {
            get => _currentPalette;
            set => _currentPalette = value;
        }

        public void SetPalette(Palette newPalette)
        {
            CurrentPalette = newPalette;
        }

        private void Start()
        {
            var palette = new PaletteDTO();
            palette.Colors["nothing"] = Color.clear;
            palette.Colors["something"] = Color.gray;
            palette.Colors["glass"] = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            _currentPalette = new Palette(palette);
        }

        private void Update()
        {
            var raisingEvent = false;
            lock (_lock)
            {
                if (_shouldTriggerEvent)
                {
                    _shouldTriggerEvent = false;
                    raisingEvent = true;
                }
            }
            if (raisingEvent)
                OnPaletteChanged?.Invoke(this);
        }
    }
}