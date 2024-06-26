using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation.General
{
    public class TargetVoxelPresenter : BasePresenter
    {
        public override string ViewName => "target voxel view";
        private Label _label = null!;
        private bool _isVisible = true;
        private string _text = string.Empty;

        public override void PrepareView()
        {
            _label = _view.Q<Label>(name: "TargetVoxel");
            ClearTargetElement();
        }

        public void UpdateTargetElement(string name, Vector3 position)
        {
            var interpolated = $"{name} ({position.x}, {position.y}, {position.z})";
            if (_text == interpolated)
                return;
            _label.visible = true;
            _text = interpolated;
            _label.text = _text;
            _isVisible = true;
        }

        public void UpdateTargetElement(string name, Vector2 position)
        {
            var interpolated = $"{name} ({position.x}, {position.y})";
            if (_text == interpolated)
                return;
            _label.visible = true;
            _text = interpolated;
            _label.text = _text;
            _isVisible = true;
        }

        public void ClearTargetElement()
        {
            if (!_isVisible)
                return;

            _label.visible = false;
            _label.text = string.Empty;
            _isVisible = false;
            _text = string.Empty;
        }
    }
}