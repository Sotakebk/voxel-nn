using System.Collections.Generic;
using UnityEngine;

namespace RealMode.Visualization
{
    public class PaletteForEntry
    {
        private readonly Dictionary<int, Color32> _colors;

        public PaletteForEntry(Palette palette, Entry entry)
        {
            _colors = new Dictionary<int, Color32>();
            foreach (var pair in entry.IndexToNameDict)
            {
                _colors.Add(pair.Key, palette.GetColor(pair.Value));
            }
        }

        public Color32 ColorForIndex(int index)
        {
            return _colors[index];
        }
    }
}