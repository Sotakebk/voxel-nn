using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RealMode.Visualization
{
    [Serializable]
    public struct NameColorPair
    {
        public string Name;
        public Color32 Color;
    }

    public class PaletteService : MonoBehaviour
    {
        public delegate void PaletteChangedEventHandler(PaletteService sender);

        public event PaletteChangedEventHandler? OnPaletteChanged;

        private bool _shouldTriggerEvent;
        private readonly object _lock = new object();

        private Palette _currentPalette = null!;

        [SerializeField]
        private NameColorPair[] DataToConstructPaletteFrom;

        public Palette CurrentPalette
        {
            get => _currentPalette;
            set => _currentPalette = value;
        }

        public void SetPalette(Palette newPalette)
        {
            CurrentPalette = newPalette;
        }

        private void Start()
        {
            var dict = new Dictionary<string, Color32>();
            foreach (var pair in DataToConstructPaletteFrom)
                dict.Add(pair.Name, pair.Color);

            _currentPalette = new Palette(dict);
            PrintPalette();
        }

        private void PrintPalette()
        {
            var sb = new StringBuilder();
            sb.AppendLine("color_dict = {");

            float FromByte(byte b)
            {
                return ((float)b) / 255f;
            }

            foreach (var pair in DataToConstructPaletteFrom)
            {
                var color = pair.Color;
                sb.AppendLine($"\t\"{pair.Name}\": ({FromByte(color.r)}, {FromByte(color.g)}, {FromByte(color.b)}),");
            }
            sb.AppendLine("}");

            Debug.Log(sb.ToString());
        }

        private void Update()
        {
            var raisingEvent = false;
            lock (_lock)
            {
                if (_shouldTriggerEvent)
                {
                    _shouldTriggerEvent = false;
                    raisingEvent = true;
                }
            }
            if (raisingEvent)
                OnPaletteChanged?.Invoke(this);
        }
    }
}