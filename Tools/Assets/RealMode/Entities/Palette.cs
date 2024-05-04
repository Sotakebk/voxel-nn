using System.Collections.Generic;
using UnityEngine;

namespace RealMode
{
    // keep this immutable
    public class Palette
    {
        private readonly Color32 DefaultColor = Color.magenta;
        private Dictionary<string, Color32> _colors { get; set; }

        public Color32 GetColor(string name)
        {
            if (_colors.TryGetValue(name, out var value))
                return value;
            return DefaultColor;
        }

        public Palette(Dictionary<string, Color32> colors)
        {
            _colors = new Dictionary<string, Color32>(colors);
        }
    }
}