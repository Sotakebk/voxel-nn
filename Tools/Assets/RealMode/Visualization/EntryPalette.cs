using System.Collections.Generic;
using UnityEngine;

namespace RealMode.Visualization
{
    public class EntryPalette
    {
        private static readonly Color32 Unkown = Color.magenta;
        private readonly Dictionary<int, Color32> _colors;

        public EntryPalette(Palette palette, Entry3D entry)
        {
            _colors = new Dictionary<int, Color32>();
            foreach (var pair in entry.IndexToNameDict)
            {
                _colors.Add(pair.Key, palette.Colors[pair.Value]);
            }
        }

        public Color32 ColorForIndex(int index)
        {
            if (_colors.TryGetValue(index, out var value))
                return value;

            return Unkown;
        }
    }
}