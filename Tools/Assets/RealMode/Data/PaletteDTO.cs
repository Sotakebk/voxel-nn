using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealMode.Data
{
    [Serializable]
    public class PaletteDTO
    {
        public Dictionary<string, Color32> Colors;

        public PaletteDTO()
        {
            Colors = new Dictionary<string, Color32>();
        }
    }
}