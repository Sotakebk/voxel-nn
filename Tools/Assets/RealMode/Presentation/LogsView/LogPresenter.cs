using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation
{
    public class LogPresenter : BasePresenter
    {
        public override string ViewName => "Logs";

        private bool SaveStackTrace { get; set; }

        private const int MaxLogCount = 100;

        [SerializeReference] private VisualTreeAsset _logTemplate = null!;
        private Toggle _saveStackTraceToggle = null!;
        private VisualElement _logContainer = null!;
        private Button _clearLogsButton = null!;

        private void Start()
        {
            Application.logMessageReceived += Application_logMessageReceived;
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            AddLogElement(type, condition, stackTrace);
        }

        public override void PrepareView()
        {
            _saveStackTraceToggle = _view.Q<Toggle>(name: "saveStackTraceToggle");
            _clearLogsButton = _view.Q<Button>(name: "ClearLogsButton");
            _logContainer = _view.Q(name: "logsContainer");

            _saveStackTraceToggle.RegisterValueChangedCallback(SaveStackTraceChangedCallback);
            _clearLogsButton.clicked += _clearLogsButton_Clicked;
        }

        private void _clearLogsButton_Clicked()
        {
            _logContainer.Clear();
        }

        private void SaveStackTraceChangedCallback(ChangeEvent<bool> evt)
        {
            SaveStackTrace = evt.newValue;
        }

        private void AddLogElement(LogType type, string message, string stackTrace)
        {
            _logContainer.Add(ConstructLogElement(type, message, stackTrace));

            while (_logContainer.childCount > MaxLogCount)
                _logContainer.RemoveAt(0);
        }

        private VisualElement ConstructLogElement(LogType type, string message, string stackTrace)
        {
            var template = _logTemplate.CloneTree();
            var typeLabel = template.Q<Label>(name: "typeLabel");
            var messageTextField = template.Q<TextField>(name: "messageTextField");
            var stackTraceFoldout = template.Q<Foldout>(name: "stackTraceFoldout");
            var stackTraceTextField = template.Q<TextField>(name: "stackTraceTextField");

            typeLabel.text = type.ToString();
            typeLabel.style.backgroundColor = LogLevelToColor(type);
            messageTextField.value = message;
            if (SaveStackTrace && !string.IsNullOrEmpty(stackTrace))
            {
                stackTraceFoldout.value = false;
                stackTraceTextField.value = stackTrace;
            }
            else
            {
                stackTraceFoldout.parent.Remove(stackTraceFoldout);
            }

            return template;
        }

        private Color LogLevelToColor(LogType logType)
        {
            return logType switch
            {
                LogType.Log => Color.white,
                LogType.Warning => Color.yellow,
                LogType.Error => Color.red,
                LogType.Exception => Color.red,
                _ => Color.white,
            };
        }
    }
}