using RealMode.Visualization;
using RealMode.Visualization.Voxels;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation.EntryView
{
    public class EntryPresenter : BasePresenter
    {
        public override string ViewName => "View";

        [SerializeReference] private VoxelVisualizer _voxelVisualizer = null!;
        [SerializeReference] private VisualizationService _visualizationService = null!;

        private VisualElement _rangeSlidersContainer = null!;
        private Button _toggleRangeSlidersButton;
        private MinMaxSlider sliderX;
        private MinMaxSlider sliderY;
        private MinMaxSlider sliderZ;

        public override void PrepareView()
        {
            _toggleRangeSlidersButton = _view.Q<Button>(name: "RangeSlidersButton");
            _rangeSlidersContainer = _view.Q<VisualElement>(name: "Sliders");

            sliderX = _view.Q<MinMaxSlider>(name: "X_Slider");
            sliderY = _view.Q<MinMaxSlider>(name: "Y_Slider");
            sliderZ = _view.Q<MinMaxSlider>(name: "Z_Slider");
            HideSliders();
            _toggleRangeSlidersButton.clicked += ToggleRangeSlidersButton_clicked;
            _voxelVisualizer.Settings.PropertyChanged += Settings_PropertyChanged;
            sliderX.RegisterValueChangedCallback(sliderX_ValueChanged);
            sliderY.RegisterValueChangedCallback(sliderY_ValueChanged);
            sliderZ.RegisterValueChangedCallback(sliderZ_ValueChanged);
            UpdateSliders();
            _visualizationService.OnEntryChangedOrModified += _visualizationService_OnEntryChangedOrModified;
        }

        private void _visualizationService_OnEntryChangedOrModified(VisualizationService sender)
        {
            ToggleSlidersActive(sender.CurrentEntry.IsEntry3D());
        }

        private void ToggleSlidersActive(bool active)
        {
            _toggleRangeSlidersButton.SetEnabled(active);
            if (!active)
                HideSliders();
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateSliders();
        }

        private void UpdateSliders()
        {
            if (!_visualizationService.CurrentEntry.IsEntry3D())
                return;

            var currentEntry = _visualizationService.CurrentEntry.AsEntry3D();
            var settings = _voxelVisualizer.Settings;
            sliderX.SetValueWithoutNotify(new Vector2(settings.MinX, settings.MaxX));
            sliderX.lowLimit = 0;
            sliderX.highLimit = currentEntry.SizeX;
            sliderX.label = $"X: {settings.MinX}-{settings.MaxX}";

            sliderY.SetValueWithoutNotify(new Vector2(settings.MinY, settings.MaxY));
            sliderY.lowLimit = 0;
            sliderY.highLimit = currentEntry.SizeY;
            sliderY.label = $"Y: {settings.MinY}-{settings.MaxY}";

            sliderZ.SetValueWithoutNotify(new Vector2(settings.MinZ, settings.MaxZ));
            sliderZ.lowLimit = 0;
            sliderZ.highLimit = currentEntry.SizeZ;
            sliderZ.label = $"Z: {settings.MinZ}-{settings.MaxZ}";
        }

        private void sliderX_ValueChanged(ChangeEvent<Vector2> @event)
        {
            _voxelVisualizer.Settings.MinX = Mathf.RoundToInt(@event.newValue.x);
            _voxelVisualizer.Settings.MaxX = Mathf.RoundToInt(@event.newValue.y);
        }

        private void sliderY_ValueChanged(ChangeEvent<Vector2> @event)
        {
            _voxelVisualizer.Settings.MinY = Mathf.RoundToInt(@event.newValue.x);
            _voxelVisualizer.Settings.MaxY = Mathf.RoundToInt(@event.newValue.y);
        }

        private void sliderZ_ValueChanged(ChangeEvent<Vector2> @event)
        {
            _voxelVisualizer.Settings.MinZ = Mathf.RoundToInt(@event.newValue.x);
            _voxelVisualizer.Settings.MaxZ = Mathf.RoundToInt(@event.newValue.y);
        }

        private void ToggleRangeSlidersButton_clicked()
        {
            ToggleSlidersVisibility();
        }

        private bool AreSlidersShown()
        {
            return _rangeSlidersContainer.style.display == DisplayStyle.Flex;
        }

        private void ToggleSlidersVisibility()
        {
            if (AreSlidersShown())
            {
                HideSliders();
            }
            else
            {
                ShowSliders();
            }
        }

        private void HideSliders()
        {
            _rangeSlidersContainer.style.display = DisplayStyle.None;
        }

        private void ShowSliders()
        {
            _rangeSlidersContainer.style.display = DisplayStyle.Flex;
        }
    }
}