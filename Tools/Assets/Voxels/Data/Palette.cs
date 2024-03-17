using System.Collections.Generic;
using UnityEngine;

namespace Voxels.Data
{
    public class Palette
    {
        public Dictionary<string, Color32> Colors { get; set; }

        public Palette()
        {
            Colors = new Dictionary<string, Color32>();
        }
    }
}