using UnityEngine;

namespace RealMode.Visualization
{
    public class EntryPalette
    {
        private static readonly Color32 Unkown = Color.magenta;
        private readonly Color32[] _colors;

        public EntryPalette(Palette palette, Entry entry)
        {
            _colors = new Color32[entry.BlockNames.Length];
            for (int i = 0; i < _colors.Length; i++)
            {
                _colors[i] = palette.Colors[entry.BlockNames[i]];
            }
        }

        public Color32 ColorForIndex(int index)
        {
            if (index < 0 || index >= _colors.Length)
                return Unkown;

            return _colors[index];
        }
    }
}