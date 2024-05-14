using RealMode.Generation;
using System;
using UnityEngine;

namespace RealMode
{
    public class DemoLogic : MonoBehaviour
    {
        [SerializeReference]
        private LoadedEntriesService _loadedEntriesService;

        [SerializeReference]
        private BaseGenerator? _generatorToRunImmediately;

        private void ConstructCommonBlocksDemo()
        {
            var values = (BlockType[])Enum.GetValues(typeof(BlockType));
            var entry = new Entry2D()
            {
                IndexToNameDict = BlockTypeHelper.NewCommonBlockTypeDictionary(),
                FriendlyName = $"Common block type demo",
                Blocks = new int[values.Length, values.Length]
            };
            for (int x = 0; x < values.Length; x++)
            {
                for (int y = 0; y < values.Length; y++)
                {
                    entry.Blocks[x, y] = values[y].ToId();
                }
            }
            _loadedEntriesService.AddEntry(entry);
        }

        private void Start()
        {
            if (_generatorToRunImmediately == null)
                ConstructCommonBlocksDemo();
            else
                _loadedEntriesService.AddEntries(_generatorToRunImmediately.Generate());
        }
    }
}