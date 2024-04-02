using RealMode.Data;
using System.Collections.Generic;
using UnityEngine;

namespace RealMode
{
    public class Palette
    {
        public Dictionary<string, Color32> Colors { get; set; }

        public Palette(PaletteDTO dto)
        {
            Colors = dto.Colors;
        }

        public PaletteDTO ToDTO()
        {
            return new PaletteDTO()
            {
                Colors = Colors
            };
        }
    }
}
