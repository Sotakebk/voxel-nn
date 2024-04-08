using RealMode.Communication;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation.General
{
    public class ServerStatePresenter : BasePresenter
    {
        public override string ViewName => "Data source state view";

        [SerializeField] private ServerContainer serverContainer = null!;

        private ServerState? dataProviderState;

        private VisualElement _dot = null!;
        private Button _playButton = null!;
        private Button _pauseButton = null!;

        protected void Update()
        {
            /*ServerState ? newValue = ServerState.Uninitialized;
            if (dataBroadcaster != null)
                newValue = dataBroadcaster.DataProvider?.State;

            if (dataProviderState != newValue)
            {
                dataProviderState = newValue;
                IsDirty = true;
            }*/
        }


        public override void PrepareView()
        {
            _dot = _view.Q(name: "statusDot");
            IsDirty = true;
        }

        public override void RegenerateView()
        {
            _dot.style.backgroundColor = GetDotColor();
        }

        public override void Show()
        {
        }

        public override void Hide()
        {
        }

        private Color GetDotColor()
        {
            return dataProviderState switch
            {
                ServerState.Uninitialized => Color.magenta,
                ServerState.Ready => Color.blue,
                ServerState.NotReady => new Color(75f / 255f, 0, 130f / 255f),
                ServerState.Working => Color.green,
                ServerState.Error => Color.red,
                _ => Color.black,
            };
        }
    }
}
