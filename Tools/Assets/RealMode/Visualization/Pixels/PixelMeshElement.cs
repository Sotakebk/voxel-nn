using UnityEngine;

namespace RealMode.Visualization.Pixels
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class PixelMeshElement : MonoBehaviour
    {
        private Mesh _mesh = null!;

        public void ApplyMesh(Mesh mesh)
        {
            _mesh = mesh;
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public void ClearMesh()
        {
            GetComponent<MeshFilter>().sharedMesh = null;
            GetComponent<MeshCollider>().sharedMesh = null;
            Destroy(_mesh);
        }

        public static PixelMeshElement ConstructOnNewGameObject(string goName, Material material, Transform parent)
        {
            var go = new GameObject(goName,
                typeof(MeshFilter),
                typeof(MeshRenderer),
                typeof(MeshCollider),
                typeof(PixelMeshElement));

            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.transform.parent = parent;
            return go.GetComponent<PixelMeshElement>();
        }
    }
}