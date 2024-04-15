using UnityEngine;

namespace RealMode
{
    public class SelectedEntryService : MonoBehaviour
    {
        public delegate void SelectedEntryChangedEventHandler(SelectedEntryService sender);

        public event SelectedEntryChangedEventHandler? OnSelectedEntryChanged;

        private bool _shouldTriggerEvent = false;
        private readonly object _lock = new object();

        public Entry? CurrentEntry { get; private set; }

        public void SelectEntry(Entry? entry)
        {
            lock (_lock)
            {
                if (CurrentEntry != entry)
                {
                    CurrentEntry = entry;
                    _shouldTriggerEvent = true;
                }
            }
        }

        private void Start()
        {
            /*
var sizeX = 12;
var sizeY = 14;
var sizeZ = 16;
var entry = new Entry3D()
{
    IndexToNameDict = new System.Collections.Generic.Dictionary<int, string>() {
        { 0, "nothing" },
        { 1, "something" }
    },
    FriendlyName = "Test entry",
    Tags = new[] { "test" },
    Blocks = new int[12, 14, 16]
};
for (int x = 0; x < sizeX; x++)
{
    for (int z = 0; z < sizeZ; z++)
    {
        var height = 4 + UnityEngine.Random.Range(-3, 3) + (x + z) / 2;

        for (int y = 0; y < sizeY; y++)
        {
            entry.Blocks[x, y, z] = y < height ? 1 : 0;
        }
    }
}

SelectEntry(entry);
*/
            var entry = new Entry2D()
            {
                IndexToNameDict = new System.Collections.Generic.Dictionary<int, string>() {
                    { 0, "nothing" },
                    { 1, "something" }
                },
                FriendlyName = "Test entry",
                Tags = new[] { "test" },
                Blocks = new int[32, 32]
            };
            for (int x = 0; x < 32; x++)
            {
                for (int y = 0; y < 32; y++)

                {
                    var xoff = x - 16;
                    var yoff = y - 16;
                    entry.Blocks[x, y] = (32 - (xoff * xoff + yoff * yoff)) > 8 ? 1 : 0;
                }
            }
            SelectEntry(entry);
        }

        private void Update()
        {
            if (_shouldTriggerEvent)
            {
                _shouldTriggerEvent = false;
                OnSelectedEntryChanged?.Invoke(this);
            }
        }
    }
}