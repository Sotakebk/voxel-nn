using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.UI
{
    public class LongActionController : GenericController
    {
        [SerializeReference] private LongRunningActionService _actionService = null!;

        private ProgressBar _progressBar = null!;

        public override void Initialize(VisualElement root)
        {
            _progressBar = root.Q<ProgressBar>(name: "ProgressBar");
            _actionService.OnActionsUpdated += _actionService_OnActionsUpdated;
            UpdateProgressBar(_actionService);
        }

        private void _actionService_OnActionsUpdated(LongRunningActionService sender)
        {
            UpdateProgressBar(sender);
        }

        private void UpdateProgressBar(LongRunningActionService service)
        {
            var topAction = service.GetTopRunningAction();
            if (topAction == null)
            {
                _progressBar.visible = false;
            }
            else
            {
                _progressBar.visible = true;
                var (completeness, name) = topAction.Value;
                _progressBar.value = completeness;
                _progressBar.title = name;
            }
        }
    }
}