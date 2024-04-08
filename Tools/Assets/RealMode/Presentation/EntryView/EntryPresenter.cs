using UnityEngine.UIElements;

namespace RealMode.Presentation.EntryView
{
    public class EntryPresenter : BasePresenter
    {
        public override string ViewName => "View";

        private VisualElement _rangeSlidersContainer = null!;

        public override void PrepareView()
        {
            var toggleRangeSlidersButton = _view.Q<Button>(name: "RangeSlidersButton");
            _rangeSlidersContainer = _view.Q<VisualElement>(name: "Sliders");

            var sliderX = _view.Q<MinMaxSlider>(name: "X_Slider");
            var sliderY = _view.Q<MinMaxSlider>(name: "Y_Slider");
            var sliderZ = _view.Q<MinMaxSlider>(name: "Z_Slider");
            HideSliders();
            toggleRangeSlidersButton.clicked += ToggleRangeSlidersButton_clicked;
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