using RealMode.Communication;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation
{
    public class NetworkConfigurationPresenter : BasePresenter
    {
        public override string ViewName => "Network settings";

        private void SetIsValidPort(bool value)
        {
            if (value)
                portTextField.RemoveFromClassList("invalidTextField");
            else
                portTextField.AddToClassList("invalidTextField");
        }

        [SerializeField] private ServerContainer _serverContainer = null!;

        private Button applyButton = null!;
        private TextField portTextField = null!;

        public override void PrepareView()
        {
            applyButton = _view.Q<Button>(name: "applyButton");
            portTextField = _view.Q<TextField>(name: "portTextField");

            applyButton.clicked += ApplyCommand;
            portTextField.RegisterValueChangedCallback(PortTextFieldValueChanged);
        }

        public override void RegenerateView()
        {
        }

        public override void Show()
        {
            base.Show();
            portTextField.value = _serverContainer.Port.ToString();
        }

        private string StripNonNumbers(string text)
        {
            return Regex.Replace(text, "[^0-9]", "");
        }

        private bool ValidatePortFromString(string value, out short port)
        {
            port = 0;
            if (!int.TryParse(value, out int i))
                return false;

            if (i < 0 || i > 65535)
                return false;

            port = (short)i;
            return true;
        }

        private void PortTextFieldValueChanged(ChangeEvent<string> evt)
        {
            portTextField.value = StripNonNumbers(portTextField.value);
            SetIsValidPort(ValidatePortFromString(portTextField.value, out short port));
        }

        private void ApplyCommand()
        {
            if (ValidatePortFromString(portTextField.value, out short port))
            {
                _serverContainer.Port = port;
            }
        }
    }
}