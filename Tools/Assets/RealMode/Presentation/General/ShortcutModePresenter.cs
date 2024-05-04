using UnityEngine.UIElements;

namespace RealMode.Presentation.General
{
    public class ShortcutModePresenter : BasePresenter
    {
        public override string ViewName => "shortcut mode view";
        private Label _label = null!;
        private bool _wasEnabled;

        public override void PrepareView()
        {
            _label = _view.Q<Label>(name: "Label");
            _wasEnabled = ShortcutService.CanUseShortcuts;
            UpdateState();
        }

        private void Update()
        {
            if (_wasEnabled != ShortcutService.CanUseShortcuts)
            {
                UpdateState();
            }
        }

        private void UpdateState()
        {
            _wasEnabled = ShortcutService.CanUseShortcuts;
            _label.text = ShortcutService.CanUseShortcuts ? "Shortcut mode" : "";

        }
    }
}