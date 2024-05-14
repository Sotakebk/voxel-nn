using NoiseTest;
using UnityEngine;

namespace RealMode.Generation
{
    public class Simple2DTerrainGenerator : Generator2D
    {
        public override string Name => "Simple 2D Terrain Generator";

        protected override Entry GenerateOneEntry()
        {
            Debug.Log($"Called with params: instances: {Instances}, size: {Size}");

            var entry = new Entry2D()
            {
                FriendlyName = "EmptyTerrain",
                Blocks = new TerrainBuilder(Size).Build(),
                Tags = new[] { "empty-terrain" },
                IndexToNameDict = BlockTypeHelper.NewCommonBlockTypeDictionary()
            };

            return entry;
        }

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
        }

        private class TerrainBuilder
        {
            private readonly Vector2Int _size;
            private readonly int[,] _blocks;
            private readonly OpenSimplexNoise _osn = new(new System.Random().Next());
            private double MinSoilHeight => _size.y * 0.2f;
            private double MaxSoilHeight => _size.y * 0.8f;

            public TerrainBuilder(Vector2Int size)
            {
                _size = size;
                _blocks = new int[size.x, size.y];
            }

            public int[,] Build()
            {
                ApplyBlocks();
                return _blocks;
            }

            private const double BaseFrequency = 0.08;

            private float GenerateSoilHeight(int x)
            {
                var delta = MaxSoilHeight - MinSoilHeight;
                return (float)(_osn.EvaluateFBM(x, 0, BaseFrequency, 4, 0.5, 0.5).To01Range() * delta + MinSoilHeight);
            }

            private float GenerateStoneHeight(int x)
            {
                var delta = MaxSoilHeight - MinSoilHeight;
                return (float)(_osn.EvaluateFBM(x, 0, BaseFrequency, 7, 0.5, 0.6).To01Range() * delta + MinSoilHeight) - 5;
            }

            private void ApplyBlocks()
            {
                ApplySoilAndStone();
                ApplyCaves();
                SpawnPonds();
                GrowGrass();
                GenerateTrees();

            }

            private void GenerateTrees()
            {
            }

            private void SpawnPonds()
            {
            }

            private void ApplySoilAndStone()
            {
                for (int x = 0; x < _size.x; x++)
                {
                    var soilHeight = GenerateSoilHeight(x);
                    var stoneHeight = GenerateStoneHeight(x);
                    var maxHeightICareAbout = Mathf.CeilToInt(Mathf.Max(soilHeight, stoneHeight));
                    for (int y = 0; y < maxHeightICareAbout; y++)
                    {
                        _blocks[x, y] = BlockType.Stone.ToId();

                        if (y > stoneHeight || y > soilHeight - 5)
                        {
                            _blocks[x, y] = BlockType.Soil.ToId();
                        }
                    }
                }
            }

            private void ApplyCaves()
            {
                for (var x = 0; x < _size.x; x++)
                {
                    var soilHeight = GenerateSoilHeight(x);
                    var maxY = Mathf.CeilToInt(soilHeight);
                    for (var y = 0; y < maxY; y++)
                    {
                        var value = _osn.EvaluateFBM(x, y, 0.1, 4, 0.6, 0.7).To01Range(); // 0 to 1
                        value = 1 - System.Math.Abs(value - 0.5) * 2; // 0 to 1
                        var multiplier = System.Math.Min(((soilHeight) - y) / (double)soilHeight, 0.5f) * 2.0; // 0-1
                        if (value * multiplier > 0.9)
                            _blocks[x, y] = BlockType.Empty.ToId();
                    }
                }
            }

            private void GrowGrass()
            {
                for (var x = 0; x < _size.x; x++)
                {
                    for (var y = _size.y - 1; y > 0; y--)
                    {
                        if (_blocks[x, y] == BlockType.Empty.ToId())
                            continue;
                        else if (_blocks[x, y] == BlockType.Soil.ToId())
                        {
                            _blocks[x, y] = BlockType.Grass.ToId();
                            break;
                        }
                        else
                            break;
                    }
                }
            }
        }
    }
}