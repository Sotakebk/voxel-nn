using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealMode.Data
{
    [Serializable]
    public class Palette
    {
        public Dictionary<string, Color32> Colors { get; set; }

        public Palette()
        {
            Colors = new Dictionary<string, Color32>();
        }
    }
}