using RealMode.Data;
using System;
using UnityEngine;

namespace RealMode.Visualization.Voxels
{
    public class VoxelVisualizationController : MonoBehaviour
    {
        [SerializeReference] public Material SolidMaterial = null!;
        [SerializeReference] public Material TransparentMaterial = null!;
        private VisualizationElement[] _solidVisualizationElements = Array.Empty<VisualizationElement>();
        private VisualizationElement[] _transparentVisualizationElements = Array.Empty<VisualizationElement>();

        private void Start()
        {
            _solidVisualizationElements = new VisualizationElement[6];
            _transparentVisualizationElements = new VisualizationElement[6];
            for (int i = 0; i < 6; i++)
            {
                _solidVisualizationElements[i] =
                    VisualizationElement.ConstructOnNewGameObject($"Solid {i}", SolidMaterial, transform);
            }

            for (int i = 0; i < 6; i++)
            {
                _transparentVisualizationElements[i] =
                    VisualizationElement.ConstructOnNewGameObject($"Transparent {i}", TransparentMaterial, transform);
            }
        }

        public void Clear()
        {
            foreach (var element in _solidVisualizationElements)
            {
                element.ClearMesh();
            }
            foreach (var element in _transparentVisualizationElements)
            {
                element.ClearMesh();
            }
            gameObject.SetActive(false);
        }

        public void Visualize(Entry entry, Palette palette)
        {
            if (entry.Dimensions.Length != 3)
                throw new ArgumentException("Entry dimensions not equal to 3");

            Clear();

            var (solidMeshes, transparentMeshes) = VoxelMeshingLogic.GenerateMesh(entry, palette);

            for (int i = 0; i < 6; i++)
            {
                var mesh = solidMeshes[i];
                var element = _solidVisualizationElements[i];

                element.ApplyMesh(mesh);
            }

            for (int i = 0; i < 6; i++)
            {
                var mesh = transparentMeshes[i];
                var element = _transparentVisualizationElements[i];

                element.ApplyMesh(mesh);
            }

            gameObject.SetActive(true);
        }
    }
}