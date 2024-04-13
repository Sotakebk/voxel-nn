using System.ComponentModel;
using UnityEngine;

namespace RealMode.Visualization.Pixels
{
    public class PixelVisualizer : MonoBehaviour
    {
        [SerializeReference] private VisualizationService _visualizationService = null!;
        [SerializeReference] private PaletteManager paletteManager = null!;
        [SerializeReference] private Material _solidMaterial = null!;
        [SerializeReference] private Material _transparentMaterial = null!;
        [SerializeReference] private Transform _basePlaneTransform = null!;
        private PixelMeshElement _solidMeshElement = null!;
        private PixelMeshElement _transparentMeshElement = null!;
        private bool _shouldRedrawModel;

        private void Awake()
        {
            _solidMeshElement = PixelMeshElement.ConstructOnNewGameObject("solid", _solidMaterial, transform);
            _transparentMeshElement = PixelMeshElement.ConstructOnNewGameObject("transparent", _transparentMaterial, transform);
        }

        public void Clear()
        {
            _solidMeshElement.ClearMesh();
            _transparentMeshElement.ClearMesh();
            gameObject.SetActive(false);
        }

        public void Visualize(Entry2D entry, Palette palette)
        {
            Clear();
            var (solidMesh, transparentMesh) = PixelMeshingLogic.GenerateMesh(entry, palette);

            _solidMeshElement.ApplyMesh(solidMesh);
            _transparentMeshElement.ApplyMesh(transparentMesh);

            gameObject.SetActive(true);
            ScaleBaseCube(entry);
        }

        private void _settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _shouldRedrawModel = true;
        }

        private void ScaleBaseCube(Entry2D entry)
        {
            var x = entry.SizeX;
            var y = entry.SizeY;
            _basePlaneTransform.position = new Vector3(x, y, 0) / 2f;
            _basePlaneTransform.localScale = new Vector3(x, y, 0);
        }

        public void VisualizeCurrentEntry()
        {
            _shouldRedrawModel = true;
        }

        private void Update()
        {
            if (_shouldRedrawModel)
            {
                _shouldRedrawModel = false;
                var currEntry = _visualizationService.CurrentEntry;
                if (currEntry.IsEntry2D())
                {
                    Visualize(currEntry.AsEntry2D(), paletteManager.CurrentPalette);
                }
                else
                {
                    Clear();
                }
            }
        }
    }
}