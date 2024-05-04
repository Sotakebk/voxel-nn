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
        [SerializeReference] private LongActionPresenter _longActionPresenter;
        [SerializeReference] private TargetVoxelPresenter _targetVoxelPresenter;
        [SerializeReference] private CurrentStatusPresenter _currentStatusPresenter;

        private KeyCode[] KeyCodes;

        private VisualElement viewContainer;

        private void Awake()
        {
            InstantiateView(null);
            var buttonContainer = _view.Q(name: "ButtonContainer");
            viewContainer = _view.Q(name: "ViewContainer");

            var i = 1;
            foreach (var presenter in _subViewPresenters)
            {
                presenter.InstantiateView(viewContainer);

                var showViewButton = new Button();
                showViewButton.text = $"({i}) {presenter.ViewName}";
                showViewButton.clicked += () => OpenView(presenter);
                buttonContainer.Add(showViewButton);
                i++;
            }
            OpenView(_subViewPresenters[0]);

            var actionContainer = _view.Q(name: "ProgressBarContainer");
            _longActionPresenter.InstantiateView(actionContainer);
            var targetContainer = _view.Q(name: "TargetVoxelContainer");
            _targetVoxelPresenter.InstantiateView(targetContainer);
            var statusContainer = _view.Q(name: "CurrentStatusContainer");
            _currentStatusPresenter.InstantiateView(statusContainer);
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                OnNumKeyPressed(1);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                OnNumKeyPressed(2);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                OnNumKeyPressed(3);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                OnNumKeyPressed(4);
            if (Input.GetKeyDown(KeyCode.Alpha5))
                OnNumKeyPressed(5);
            if (Input.GetKeyDown(KeyCode.Alpha6))
                OnNumKeyPressed(6);
            if (Input.GetKeyDown(KeyCode.Alpha7))
                OnNumKeyPressed(7);
            if (Input.GetKeyDown(KeyCode.Alpha8))
                OnNumKeyPressed(8);
            if (Input.GetKeyDown(KeyCode.Alpha9))
                OnNumKeyPressed(9);
        }

        private void OnNumKeyPressed(int i)
        {
            i--;
            if (i >= 0 && i < _subViewPresenters.Length)
            {
                var view = _subViewPresenters[i];
                OpenView(view);
            }
        }
    }
}