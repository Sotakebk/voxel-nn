using UnityEngine;

namespace RealMode.Generation
{
    public class EmptyTerrainGenerator : Generator3D
    {
        public override string Name => "Empty Terrain Generator";

        protected override Entry GenerateOneEntry()
        {
            Debug.Log($"Called with params: instances: {Instances}, size: {Size}");
            return new Entry3D()
            {
                FriendlyName = "EmptyTerrain",
                Blocks = new int[Size.x, Size.y, Size.z],
                Tags = new[] { "empty-terrain" },
                IndexToNameDict = BlockTypeHelper.NewCommonBlockTypeDictionary()
            };
        }

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
        }
    }
}
