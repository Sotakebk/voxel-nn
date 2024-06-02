using NoiseTest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealMode.Generation
{
    public class Simple2DTerrainGenerator : Generator2D
    {
        public override string Name => "Simple 2D Terrain Generator";

        protected override Entry GenerateOneEntry()
        {
            Debug.Log($"Called with params: instances: {Instances}, size: {Size}");

            var (blocks, tags) = new TerrainBuilder(Size).Build();
            var entry = new Entry2D()
            {
                FriendlyName = "2D Terrain",
                Blocks = blocks,
                Tags = tags,
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
            private readonly List<string> _tags;
            private readonly System.Random _random = new System.Random();
            private readonly OpenSimplexNoise _osn;
            private double MinSoilHeight => _size.y * 0.2f;
            private double MaxSoilHeight => _size.y * 0.8f;
            private Vector2 _randomOffset;
            private double TerrainFrequency;

            private bool UpdateOrIgnore(BlockType blockType, int x, int y, bool onlyIfEmpty = false)
            {
                return UpdateOrIgnore((int)blockType, x, y, onlyIfEmpty);
            }
            private bool UpdateOrIgnore(int blockId, int x, int y, bool onlyIfEmpty = false)
            {
                if (x < 0 || x >= _size.x)
                    return false;
                if (y < 0 || y >= _size.y)
                    return false;

                if (onlyIfEmpty && _blocks[x, y] != (int)BlockType.Empty)
                    return false;

                _blocks[x, y] = blockId;
                return true;
            }

            public TerrainBuilder(Vector2Int size)
            {
                _size = size;
                _blocks = new int[size.x, size.y];
                _tags = new List<string>();
                _osn = new(_random.Next());
                _randomOffset = new Vector2(
                    (float)_random.NextDouble(),
                    (float)_random.NextDouble()) * 100f;

                var tFreq = _random.Next(3);
                if (tFreq == 0)
                {
                    _tags.Add("terrain-freq-low");
                    TerrainFrequency = 0.05;
                }
                else if (tFreq == 1)
                {
                    _tags.Add("terrain-freq-high");
                    TerrainFrequency = 0.10;
                }
                else
                {
                    _tags.Add("terrain-freq-mid");
                    TerrainFrequency = 0.075;
                }
            }

            public (int[,] blocks, string[] tags) Build()
            {
                ApplyBlocks();
                return (_blocks, _tags.ToArray());
            }


            private float GenerateSoilHeight(int x)
            {
                var delta = MaxSoilHeight - MinSoilHeight;
                return (float)(_osn.EvaluateFBM(x + _randomOffset.x, 0, TerrainFrequency, 4, 0.5, 0.5).To01Range() * delta + MinSoilHeight);
            }

            private float GenerateStoneHeight(int x)
            {
                var delta = MaxSoilHeight - MinSoilHeight;
                return (float)(_osn.EvaluateFBM(x + _randomOffset.x, 0, TerrainFrequency, 7, 0.5, 0.6).To01Range() * delta + MinSoilHeight) - 5;
            }

            private void ApplyBlocks()
            {
                ApplySoilAndStone();
                if (_random.NextDouble() < 0.6f)
                {
                    SpawnPonds();
                }
                if (_random.NextDouble() < 0.7f)
                {
                    _tags.Add("caves");
                    ApplyCaves();
                }
                GrowGrass();
                if (_random.NextDouble() < 0.7f)
                {
                    GenerateTrees();
                }

            }

            private void GeneratePineTree(int x, int y)
            {
                var height = _random.Next(2, 12);

                for (int h = 0; h < height; h++)
                {
                    UpdateOrIgnore(BlockType.WoodDark, x, y + h, onlyIfEmpty: true);
                }

                var width = _random.Next(2, System.Math.Max(3, (int)(height / 2.0f)));

                var leaf_offset = _random.Next(2);
                for (int h = _random.Next(2); h <= height + 1; h++)
                {
                    var leaf_w = (height - h) / 2f + 1;
                    if (leaf_w > 2)
                        leaf_w -= (h + leaf_offset) % 2;
                    for (var w = -width; w <= width; w++)
                    {
                        if (Mathf.Abs(w) < leaf_w)
                            UpdateOrIgnore(BlockType.Leaves, x + w, y + h + 1, onlyIfEmpty: true);
                    }
                }
            }

            private void GenerateLeafyTree(int x, int y)
            {
                var height = _random.Next(2, 12);

                for (int h = 0; h < height; h++)
                {
                    UpdateOrIgnore(BlockType.WoodLight, x, y + h, onlyIfEmpty: true);
                }

                var lwidth = _random.Next(3, System.Math.Max(3, (int)(height / 2.0f)));
                var lheight = _random.Next(3, System.Math.Max(3, (int)(height / 1.5f)));
                var hoffset = _random.Next(2);

                for (var w = -lwidth; w <= lwidth; w++)
                {
                    for (int h = -lheight; h <= lheight; h++)
                    {
                        var dir = new Vector3(w, h).normalized;
                        var r = (w * w) / (float)(lwidth * lwidth) + (h * h) / (float)(lheight * lheight);
                        if (r < _osn.EvaluateFBM(dir.x + x, dir.y + y, 0.2, 2, 0.5, 0.5).To01Range() * 0.3f + 0.5f)
                        {
                            UpdateOrIgnore(BlockType.Leaves, x + w, y + h + height - hoffset, onlyIfEmpty: true);
                        }
                    }
                }
            }

            private void GenerateTrees()
            {
                int? TryGetFirstGrassPosition(int x)
                {
                    for (var y = _size.y - 1; y > 0; y--)
                    {
                        if (_blocks[x, y] == BlockType.Empty.ToId())
                            continue;
                        else if (_blocks[x, y] == BlockType.Grass.ToId())
                        {
                            return y;
                        }
                        else
                            return null;
                    }
                    return null;
                }

                var tree_count = _random.Next(1, (int)Mathf.Sqrt(_size.x));

                var addedAnyTrees = false;
                var addedPineTrees = false;
                var addedLeafyTrees = false;
                for (int i = 0; i < tree_count; i++)
                {
                    Vector2Int? targetPos = null;

                    for (int attempts = 0; attempts < (int)Mathf.Sqrt(_size.x); attempts++)
                    {
                        var pos_x = _random.Next(0, _size.x);
                        var pos_y = TryGetFirstGrassPosition(pos_x);
                        if (pos_y != null)
                        {
                            targetPos = new Vector2Int(pos_x, pos_y.Value);
                            break;
                        }
                    }
                    if (targetPos == null)
                        continue;

                    var random = _random.Next(2);
                    switch (random)
                    {
                        case 0:
                            GeneratePineTree(targetPos.Value.x, targetPos.Value.y + 1);
                            addedPineTrees = true;
                            break;
                        case 1:
                            GenerateLeafyTree(targetPos.Value.x, targetPos.Value.y + 1);
                            addedLeafyTrees = true;
                            break;
                        default:
                            throw new System.Exception();
                    }
                    addedAnyTrees = true;
                }

                if (addedAnyTrees)
                    _tags.Add("trees");
                if (addedPineTrees)
                    _tags.Add("pine-trees");
                if (addedLeafyTrees)
                    _tags.Add("leafy-trees");
            }

            private void SpawnPonds()
            {
                var lowest_y = _size.y;
                int lowest_x = 0;
                var foundPoint = false;
                int[] random_x = Enumerable.Range(0, _size.x).OrderBy(_ => _random.Next()).ToArray();
                for (int i = 0; i < _size.x; i++)
                {
                    var x = random_x[i];
                    for (int y = _size.x - 1; y > 0; y--)
                    {
                        if (_blocks[x, y] == (int)BlockType.Soil)
                        {
                            // save position ABOVE lowest point
                            if (y + 1 < lowest_y)
                            {
                                lowest_x = x;
                                lowest_y = y + 1;
                                foundPoint = true;
                            }
                            break;
                        }
                    }
                }

                if (!foundPoint)
                    return;

                var addedAnyWater = false;
                var addedWater = 0;
                var maxWater = (int)(Mathf.Sqrt(_size.x * _size.y) * 0.25f) * _random.Next(3, 6);
                var maxWaterOnLevel = Mathf.Sqrt(2f * _size.x);
                var current_y = lowest_y;
                do
                {
                    var max_left = lowest_x;
                    var max_right = lowest_x;
                    var shouldBreakLoopImmediately = false;

                    while (max_left > 0 && _blocks[max_left - 1, current_y] == (int)BlockType.Empty)
                    {
                        // do not risk overhangs
                        if (_blocks[max_left - 1, current_y - 1] == (int)BlockType.Empty)
                        {
                            shouldBreakLoopImmediately = true;
                            break;
                        }
                        else
                        {
                            max_left--;
                        }
                    }

                    while (max_right < _size.x - 1 && _blocks[max_right + 1, current_y] == (int)BlockType.Empty)
                    {
                        // do not risk overhangs
                        if (_blocks[max_right + 1, current_y - 1] == (int)BlockType.Empty)
                        {
                            shouldBreakLoopImmediately = true;
                            break;
                        }
                        else
                        {
                            max_right++;
                        }
                    }
                    if (shouldBreakLoopImmediately)
                        break;

                    if (addedWater + max_right - max_left > maxWater || max_right - max_left > maxWaterOnLevel)
                        break;

                    for (var i = max_left; i <= max_right; i++)
                    {
                        _blocks[i, current_y] = (int)BlockType.Water;
                        if (_blocks[i, current_y - 1] != (int)BlockType.Water)
                            UpdateOrIgnore(BlockType.Sand, i, current_y - 1);
                        if (_blocks[i, current_y - 2] != (int)BlockType.Water)
                            UpdateOrIgnore(BlockType.Sand, i, current_y - 2);
                    }
                    UpdateOrIgnore(BlockType.Sand, max_left - 1, current_y);
                    UpdateOrIgnore(BlockType.Sand, max_left - 1, current_y - 1);
                    UpdateOrIgnore(BlockType.Sand, max_right + 1, current_y);
                    UpdateOrIgnore(BlockType.Sand, max_right + 1, current_y - 1);

                    addedWater += max_right - max_left;
                    current_y++;
                    addedAnyWater = true;
                } while (addedWater < maxWater);


                if (addedAnyWater)
                    _tags.Add("pond");
                else
                    return;
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
                        var value = _osn.EvaluateFBM(x + _randomOffset.y, y + _randomOffset.x, 0.1, 4, 0.6, 0.7).To01Range(); // 0 to 1
                        value = 1 - System.Math.Abs(value - 0.5) * 2; // 0 to 1
                        var multiplier = System.Math.Min(((soilHeight) - y) / (double)soilHeight, 0.5f) * 2.0; // 0-1
                        if (value * multiplier > 0.92)
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