using Unity.VisualScripting;
using UnityEngine;

namespace RealMode
{
    public class ActiveEntryService : MonoBehaviour
    {
        public delegate void ActiveEntryChangedEventHandler(ActiveEntryService sender);

        public event ActiveEntryChangedEventHandler? OnEntryChanged;

        public Entry? CurrentEntry;

        public void SelectEntry(Entry entry)
        {
            CurrentEntry = entry;
            OnEntryChanged?.Invoke(this);
        }

        private void Start()
        {
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
                    var height = 4 + Random.Range(-3, 3) + (x + z) / 2;

                    for (int y = 0; y < sizeY; y++)
                    {
                        entry.Blocks[x, y, z] = y < height ? 1 : 0;
                    }
                }
            }

            Debug.Log(entry.Blocks.ToCommaSeparatedString());

            SelectEntry(entry);
        }
    }
}