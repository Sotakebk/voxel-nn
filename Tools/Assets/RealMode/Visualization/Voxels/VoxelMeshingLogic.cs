using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    public class PixelMeshingLogic
    {
        public static (Mesh[] solid, Mesh[] transparent) GenerateMesh(Entry3D entry, Palette palette, CurrentVisualizationSettings settings)
        {
            var context = new VoxelMeshingContext(entry, palette, settings);

            return context.GenerateMeshes();
        }
    }

    internal class VoxelMeshingContext
    {
        private struct MeshData
        {
            public List<Vector3> Vertices;
            public List<int> Indices;
            public List<Color32> Colors;
            public List<Vector3> Normals;
            public List<Vector2> Uvs;

            public static MeshData GetNew()
            {
                return new MeshData()
                {
                    Vertices = new List<Vector3>(),
                    Indices = new List<int>(),
                    Colors = new List<Color32>(),
                    Normals = new List<Vector3>(),
                    Uvs = new List<Vector2>()
                };
            }
        }

        private readonly Entry3D _entry;
        private readonly EntryPalette _palette;
        private readonly CurrentVisualizationSettings _settings;

        public VoxelMeshingContext(Entry3D entry, Palette palette, CurrentVisualizationSettings settings)
        {
            _palette = new EntryPalette(palette, entry);
            _entry = entry;
            _settings = settings;
        }

        public (Mesh[] solid, Mesh[] transparent) GenerateMeshes()
        {
            var (solidMeshes, transparentMeshes) = BuildFaces();
            List<Mesh> solid = new List<Mesh>(6);
            List<Mesh> transparent = new List<Mesh>(6);

            foreach (var m in solidMeshes)
            {
                solid.Add(ConstructMeshFromMeshData(m));
            }
            foreach (var m in transparentMeshes)
            {
                transparent.Add(ConstructMeshFromMeshData(m));
            }

            return (solid.ToArray(), transparent.ToArray());
        }

        private Mesh ConstructMeshFromMeshData(MeshData data)
        {
            var mesh = new Mesh();
            mesh.SetIndexBufferParams(data.Indices.Count, UnityEngine.Rendering.IndexFormat.UInt32);
            mesh.SetVertices(data.Vertices);
            mesh.SetIndices(data.Indices, MeshTopology.Triangles, 0, true, 0);
            mesh.SetColors(data.Colors);
            mesh.SetNormals(data.Normals);
            mesh.SetUVs(0, data.Uvs);

            mesh.RecalculateBounds();
            mesh.Optimize();
            mesh.UploadMeshData(false);

            return mesh;
        }

        private (MeshData[] solidMeshData, MeshData[] transparentMeshData) BuildFaces()
        {
            var negx = Task.Run(GoAlongAxisNegativeX);
            var negy = Task.Run(GoAlongAxisNegativeY);
            var negz = Task.Run(GoAlongAxisNegativeZ);
            var posx = Task.Run(GoAlongAxisPositiveX);
            var posy = Task.Run(GoAlongAxisPositiveY);
            var posz = Task.Run(GoAlongAxisPositiveZ);

            //Task.WaitAll(negx, negy, negz, posx, posy, posz);
            Task.WaitAll(negx, posx);

            return (new[]
            {
                negx.Result.solid,
                negy.Result.solid,
                negz.Result.solid,
                posx.Result.solid,
                posy.Result.solid,
                posz.Result.solid
            },
            new[]
            {
                negx.Result.transparent,
                negy.Result.transparent,
                negz.Result.transparent,
                posx.Result.transparent,
                posy.Result.transparent,
                posz.Result.transparent
            });
        }

        private (MeshData solid, MeshData transparent) GoAlongAxisPositiveX()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();

            for (int x = _settings.MinX; x < _settings.MaxX; x++)
            {
                for (int y = _settings.MinY; y < _settings.MaxY; y++)
                {
                    for (int z = _settings.MinZ; z < _settings.MaxZ; z++)
                    {
                        var currentVoxel = BlockOrNothing(x, y, z) ?? throw new($"x: {x}, y: {y}, z: {z}");
                        var forwardVoxel = BlockOrNothing(x + 1, y, z);
                        var behindVoxel = BlockOrNothing(x - 1, y, z);

                        if (ShouldDrawSolidFace(currentVoxel, forwardVoxel, out var color))
                        {
                            AddFacePosX(solid, x, y, z, color);
                        }

                        if (ShouldDrawTransparentFace(currentVoxel, behindVoxel, out var transparentColor))
                        {
                            AddFaceNegX(transparent, x, y, z, transparentColor);
                        }
                    }
                }
            }

            return (solid, transparent);
        }

        private (MeshData solid, MeshData transparent) GoAlongAxisNegativeX()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();

            for (int x = (_settings.MaxX - 1); x >= _settings.MinX; x--)
            {
                for (int y = _settings.MinY; y < _settings.MaxY; y++)
                {
                    for (int z = _settings.MinZ; z < _settings.MaxZ; z++)
                    {
                        var currentVoxel = BlockOrNothing(x, y, z) ?? throw new($"x: {x}, y: {y}, z: {z}");
                        var forwardVoxel = BlockOrNothing(x - 1, y, z);
                        var behindVoxel = BlockOrNothing(x + 1, y, z);

                        if (ShouldDrawSolidFace(currentVoxel, forwardVoxel, out var color))
                        {
                            AddFaceNegX(solid, x, y, z, color);
                        }

                        if (ShouldDrawTransparentFace(currentVoxel, behindVoxel, out var transparentColor))
                        {
                            AddFacePosX(transparent, x, y, z, transparentColor);
                        }
                    }
                }
            }

            return (solid, transparent);
        }

        private (MeshData solid, MeshData transparent) GoAlongAxisPositiveY()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();

            for (int y = _settings.MinY; y < _settings.MaxY; y++)
            {
                for (int x = _settings.MinX; x < _settings.MaxX; x++)
                {
                    for (int z = _settings.MinZ; z < _settings.MaxZ; z++)
                    {
                        var currentVoxel = BlockOrNothing(x, y, z) ?? throw new($"x: {x}, y: {y}, z: {z}");
                        var forwardVoxel = BlockOrNothing(x, y + 1, z);
                        var behindVoxel = BlockOrNothing(x, y - 1, z);

                        if (ShouldDrawSolidFace(currentVoxel, forwardVoxel, out var color))
                        {
                            AddFacePosY(solid, x, y, z, color);
                        }

                        if (ShouldDrawTransparentFace(currentVoxel, behindVoxel, out var transparentColor))
                        {
                            AddFaceNegY(transparent, x, y, z, transparentColor);
                        }
                    }
                }
            }

            return (solid, transparent);
        }

        private (MeshData solid, MeshData transparent) GoAlongAxisNegativeY()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();

            for (int y = (_settings.MaxY - 1); y >= _settings.MinY; y--)
            {
                for (int x = _settings.MinX; x < _settings.MaxX; x++)
                {
                    for (int z = _settings.MinZ; z < _settings.MaxZ; z++)
                    {
                        var currentVoxel = BlockOrNothing(x, y, z) ?? throw new($"x: {x}, y: {y}, z: {z}");
                        var forwardVoxel = BlockOrNothing(x, y - 1, z);
                        var behindVoxel = BlockOrNothing(x, y + 1, z);

                        if (ShouldDrawSolidFace(currentVoxel, forwardVoxel, out var color))
                        {
                            AddFaceNegY(solid, x, y, z, color);
                        }

                        if (ShouldDrawTransparentFace(currentVoxel, behindVoxel, out var transparentColor))
                        {
                            AddFacePosY(transparent, x, y, z, transparentColor);
                        }
                    }
                }
            }

            return (solid, transparent);
        }

        private (MeshData solid, MeshData transparent) GoAlongAxisPositiveZ()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();

            for (int z = _settings.MinZ; z < _settings.MaxZ; z++)
            {
                for (int x = _settings.MinX; x < _settings.MaxX; x++)
                {
                    for (int y = _settings.MinY; y < _settings.MaxY; y++)
                    {
                        var currentVoxel = BlockOrNothing(x, y, z) ?? throw new($"x: {x}, y: {y}, z: {z}");
                        var forwardVoxel = BlockOrNothing(x, y, z + 1);
                        var behindVoxel = BlockOrNothing(x, y, z - 1);

                        if (ShouldDrawSolidFace(currentVoxel, forwardVoxel, out var color))
                        {
                            AddFacePosZ(solid, x, y, z, color);
                        }

                        if (ShouldDrawTransparentFace(currentVoxel, behindVoxel, out var transparentColor))
                        {
                            AddFaceNegZ(transparent, x, y, z, transparentColor);
                        }
                    }
                }
            }

            return (solid, transparent);
        }

        private (MeshData solid, MeshData transparent) GoAlongAxisNegativeZ()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();

            for (int z = (_settings.MaxZ - 1); z >= _settings.MinZ; z--)
            {
                for (int x = _settings.MinX; x < _settings.MaxX; x++)
                {
                    for (int y = _settings.MinY; y < _settings.MaxY; y++)
                    {
                        var currentVoxel = BlockOrNothing(x, y, z) ?? throw new($"x: {x}, y: {y}, z: {z}");
                        var forwardVoxel = BlockOrNothing(x, y, z - 1);
                        var behindVoxel = BlockOrNothing(x, y, z + 1);

                        if (ShouldDrawSolidFace(currentVoxel, forwardVoxel, out var color))
                        {
                            AddFaceNegZ(solid, x, y, z, color);
                        }

                        if (ShouldDrawTransparentFace(currentVoxel, behindVoxel, out var transparentColor))
                        {
                            AddFacePosZ(transparent, x, y, z, transparentColor);
                        }
                    }
                }
            }

            return (solid, transparent);
        }

        private int? BlockOrNothing(int x, int y, int z)
        {
            if (x < _settings.MinX || x >= _settings.MaxX)
                return null;
            if (y < _settings.MinY || y >= _settings.MaxY)
                return null;
            if (z < _settings.MinZ || z >= _settings.MaxZ)
                return null;
            return _entry.BlockOrNothing(x, y, z);
        }

        private bool ShouldDrawSolidFace(int voxel, int? other, out Color32 color)
        {
            color = _palette.ColorForIndex(voxel);
            if (color.a != 255) // don't draw transparent faces
                return false;

            // other is nothing, draw it
            if (other == null)
                return true;

            // don't draw faces between blocks of same type
            if (voxel == other.Value)
                return false;

            var otherColor = _palette.ColorForIndex(other.Value);

            // draw faces towards other blocks with non-opaque colors
            if (otherColor.a != 255)
                return true;

            return false;
        }

        private bool ShouldDrawTransparentFace(int voxel, int? other, out Color32 color)
        {
            color = _palette.ColorForIndex(voxel);
            if (color.a == 0) // don't draw fully transparent faces
                return false;

            if (color.a == 255) // don't draw solid faces
                return false;

            // other is nothing, draw it
            if (other == null)
                return true;

            // don't draw faces between blocks of same type
            if (voxel == other.Value)
                return false;

            var otherColor = _palette.ColorForIndex(other.Value);

            // draw faces towards other blocks with non-opaque colors
            if (otherColor.a != 255)
                return true;

            return false;
        }

        private void AddFacePosX(MeshData data, int x, int y, int z, Color color)
        {
            var d = new Vector3(x + 1, y, z);
            var c = new Vector3(x + 1, y, z + 1);
            var b = new Vector3(x + 1, y + 1, z + 1);
            var a = new Vector3(x + 1, y + 1, z);
            var normal = new Vector3(1, 0, 0);
            AddFace(data, a, b, c, d, normal, color);
        }

        private void AddFaceNegX(MeshData data, int x, int y, int z, Color color)
        {
            var a = new Vector3(x, y, z);
            var b = new Vector3(x, y, z + 1);
            var c = new Vector3(x, y + 1, z + 1);
            var d = new Vector3(x, y + 1, z);
            var normal = new Vector3(-1, 0, 0);
            AddFace(data, a, b, c, d, normal, color);
        }

        private void AddFacePosZ(MeshData data, int x, int y, int z, Color color)
        {
            var a = new Vector3(x, y, z + 1);
            var b = new Vector3(x + 1, y, z + 1);
            var c = new Vector3(x + 1, y + 1, z + 1);
            var d = new Vector3(x, y + 1, z + 1);
            var normal = new Vector3(0, 0, 1);
            AddFace(data, a, b, c, d, normal, color);
        }

        private void AddFaceNegZ(MeshData data, int x, int y, int z, Color color)
        {
            var d = new Vector3(x, y, z);
            var c = new Vector3(x + 1, y, z);
            var b = new Vector3(x + 1, y + 1, z);
            var a = new Vector3(x, y + 1, z);
            var normal = new Vector3(0, 0, 1);
            AddFace(data, a, b, c, d, normal, color);
        }

        private void AddFacePosY(MeshData data, int x, int y, int z, Color color)
        {
            var d = new Vector3(x, y + 1, z);
            var c = new Vector3(x + 1, y + 1, z);
            var b = new Vector3(x + 1, y + 1, z + 1);
            var a = new Vector3(x, y + 1, z + 1);
            var normal = new Vector3(0, 1, 0);
            AddFace(data, a, b, c, d, normal, color);
        }

        private void AddFaceNegY(MeshData data, int x, int y, int z, Color color)
        {
            var a = new Vector3(x, y, z);
            var b = new Vector3(x + 1, y, z);
            var c = new Vector3(x + 1, y, z + 1);
            var d = new Vector3(x, y, z + 1);
            var normal = new Vector3(0, -1, 0);
            AddFace(data, a, b, c, d, normal, color);
        }

        private void AddFace(MeshData data, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal, Color color)
        {
            var baseIndex = data.Vertices.Count;
            data.Vertices.Add(a);
            data.Vertices.Add(b);
            data.Vertices.Add(c);
            data.Vertices.Add(d);

            data.Indices.Add(baseIndex + 0);
            data.Indices.Add(baseIndex + 1);
            data.Indices.Add(baseIndex + 2);
            data.Indices.Add(baseIndex + 2);
            data.Indices.Add(baseIndex + 3);
            data.Indices.Add(baseIndex + 0);

            data.Normals.Add(normal);
            data.Normals.Add(normal);
            data.Normals.Add(normal);
            data.Normals.Add(normal);

            data.Colors.Add(color);
            data.Colors.Add(color);
            data.Colors.Add(color);
            data.Colors.Add(color);

            data.Uvs.Add(new Vector2(0, 0));
            data.Uvs.Add(new Vector2(1, 0));
            data.Uvs.Add(new Vector2(1, 1));
            data.Uvs.Add(new Vector2(0, 1));
        }
    }
}