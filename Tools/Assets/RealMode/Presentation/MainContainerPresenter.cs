using RealMode.Presentation.General;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation
{
    public class MainContainerPresenter : BasePresenter
    {
        public override string ViewName => "Main UI container";

        [SerializeReference] private UIDocument _uiRoot;
        [SerializeReference] private BasePresenter[] _subViewPresenters;
        [SerializeReference] private ServerStatePresenter _serverStatePresenter;
        [SerializeReference] private LongActionPresenter _longActionPresenter;
        [SerializeReference] private TargetVoxelPresenter _targetVoxelPresenter;

        private VisualElement viewContainer;

        private void Awake()
        {
            InstantiateView(null);
            var buttonContainer = _view.Q(name: "ButtonContainer");
            viewContainer = _view.Q(name: "ViewContainer");

            foreach (var presenter in _subViewPresenters)
            {
                presenter.InstantiateView(viewContainer);

                var showViewButton = new Button();
                showViewButton.text = presenter.ViewName;
                showViewButton.clicked += () => OpenView(presenter);
                buttonContainer.Add(showViewButton);
            }
            OpenView(_subViewPresenters[0]);

            var statusContainer = _view.Q(name: "ServerStatusContainer");
            _serverStatePresenter.InstantiateView(statusContainer);
            var actionContainer = _view.Q(name: "ProgressBarContainer");
            _longActionPresenter.InstantiateView(actionContainer);
            var targetContainer = _view.Q(name: "TargetVoxelContainer");
            _targetVoxelPresenter.InstantiateView(targetContainer);
        }

        public override void InstantiateView(VisualElement rootElement)
        {
            _uiRoot.visualTreeAsset = viewAsset;
            _view = _uiRoot.rootVisualElement;
        }

        public override void Show()
        {
        }

        public override void Hide()
        {
        }

        private void OpenView(BasePresenter presenter)
        {
            foreach (var p in _subViewPresenters)
            {
                if (p == presenter)
                    p.Show();
                else
                    p.Hide();
            }
        }
    }
}
