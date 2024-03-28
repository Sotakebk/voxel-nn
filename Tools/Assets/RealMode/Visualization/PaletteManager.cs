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
            _currentPalette = new Palette();
            _currentPalette.Colors["nothing"] = Color.clear;
            _currentPalette.Colors["something"] = Color.gray;
            _currentPalette.Colors["glass"] = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        }
    }
}
