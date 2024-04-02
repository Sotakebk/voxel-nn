using RealMode.Data;
using UnityEngine;

namespace RealMode.Visualization
{
    public class PaletteManager : MonoBehaviour
    {
        [SerializeReference]
        private Palette _currentPalette = null!;

        public Palette CurrentPalette
        {
            get => _currentPalette;
            set => _currentPalette = value;
        }

        private void Awake()
        {
            var palette = new PaletteDTO();
            palette.Colors["nothing"] = Color.clear;
            palette.Colors["something"] = Color.gray;
            palette.Colors["glass"] = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            _currentPalette = new Palette(palette);
        }
    }
}
