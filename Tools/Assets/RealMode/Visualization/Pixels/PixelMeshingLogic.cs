using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealMode.Visualization.Pixels
{
    public class PixelMeshingLogic
    {
        public static (Mesh solid, Mesh transparent) GenerateMesh(Entry2D entry, Palette palette)
        {
            var context = new PixelMeshingContext(entry, palette);

            return context.GenerateMeshes();
        }
    }

    internal class PixelMeshingContext
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

        private readonly Entry2D _entry;
        private readonly EntryPalette _palette;

        public PixelMeshingContext(Entry2D entry, Palette palette)
        {
            _palette = new EntryPalette(palette, entry);
            _entry = entry;
        }

        public (Mesh solid, Mesh transparent) GenerateMeshes()
        {
            var (solidMeshData, transparentMeshData) = BuildFaces();
            var solidMesh = ConstructMeshFromMeshData(solidMeshData);
            var transparenMesh = ConstructMeshFromMeshData(transparentMeshData);

            return (solidMesh, transparenMesh);
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

        private (MeshData solidMeshData, MeshData transparentMeshData) BuildFaces()
        {
            return GoThroughPixels();
        }

        private (MeshData solid, MeshData transparent) GoThroughPixels()
        {
            var solid = MeshData.GetNew();
            var transparent = MeshData.GetNew();
            for (int x = 0; x < _entry.SizeX; x++)
            {
                for (int y = 0; y < _entry.SizeY; y++)
                {
                    var currentVoxel = GetBlockId(x, y);

                    var (type, color) = GetFaceTypeAndColor(currentVoxel);

                    if (type == FaceType.Solid)
                    {
                        AddFace(solid, x, y, color);
                    }
                    else if (type == FaceType.Transparent)
                    {
                        AddFace(transparent, x, y, color);
                    }
                    else
                    {
                        // continue
                    }
                }
            }
            return (solid, transparent);
        }

        private int GetBlockId(int x, int y)
        {
            return _entry.BlockOrNothing(x, y) ?? throw new ArgumentException();
        }

        private enum FaceType
        {
            None,
            Transparent,
            Solid
        }

        private (FaceType type, Color32 color) GetFaceTypeAndColor(int voxel)
        {
            var color = _palette.ColorForIndex(voxel);
            if (color.a == 0) // don't draw fully transparent faces
                return (FaceType.None, color);

            if (color.a != 255)
                return (FaceType.Transparent, color);

            return (FaceType.Solid, color);
        }

        private void AddFace(MeshData data, int x, int y, Color color)
        {
            var a = new Vector3(x, y, 0);
            var b = new Vector3(x, y + 1, 0);
            var c = new Vector3(x + 1, y + 1, 0);
            var d = new Vector3(x + 1, y, 0);
            var normal = new Vector3(0, 0, -1);
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