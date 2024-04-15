using UnityEngine;

namespace RealMode.Visualization.Pixels
{
    public class PixelVisualizer : BaseVisualizer
    {
        [SerializeReference] private Material _solidMaterial = null!;
        [SerializeReference] private Material _transparentMaterial = null!;
        [SerializeReference] private Transform _basePlaneTransform = null!;
        private PixelMeshElement _solidMeshElement = null!;
        private PixelMeshElement _transparentMeshElement = null!;

        protected override void Awake()
        {
            base.Awake();
            _solidMeshElement = PixelMeshElement.ConstructOnNewGameObject("solid", _solidMaterial, transform);
            _transparentMeshElement = PixelMeshElement.ConstructOnNewGameObject("transparent", _transparentMaterial, transform);
        }

        protected override void Clear()
        {
            _solidMeshElement.ClearMesh();
            _transparentMeshElement.ClearMesh();
        }

        private void ScaleBaseCube(Entry2D entry)
        {
            var x = entry.SizeX;
            var y = entry.SizeY;
            _basePlaneTransform.position = new Vector3(x, y, 0) / 2f;
            _basePlaneTransform.localScale = new Vector3(x, y, 0);
        }

        protected override bool CanVisualizeEntry(Entry entry)
        {
            return entry.IsEntry2D();
        }

        protected override void VisualizeEntry(Entry entry)
        {
            var entry2d = entry.AsEntry2D();

            var (solidMesh, transparentMesh) = PixelMeshingLogic.GenerateMesh(entry2d, _paletteService.CurrentPalette);

            _solidMeshElement.ApplyMesh(solidMesh);
            _transparentMeshElement.ApplyMesh(transparentMesh);

            ScaleBaseCube(entry2d);
            Unhide();
        }
    }
}