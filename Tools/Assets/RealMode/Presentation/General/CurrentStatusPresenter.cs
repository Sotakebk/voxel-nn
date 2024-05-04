using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation.General
{
    public class CurrentStatusPresenter : BasePresenter
    {
        [SerializeReference]
        private SelectedEntryService _selectedEntryService;
        [SerializeReference]
        private LoadedEntriesService _loadedEntriesService;

        public override string ViewName => "current status view";
        private Label _label = null!;
        private bool _isVisible = true;
        private string _text = string.Empty;

        public override void PrepareView()
        {
            _label = _view.Q<Label>(name: "CurrentStatus");
        }

        private void Start()
        {
            _selectedEntryService.OnSelectedEntryChanged += _selectedEntryService_OnSelectedEntryChanged;
            _loadedEntriesService.OnCollectionChanged += _loadedEntriesService_OnCollectionChanged;
        }

        private void _loadedEntriesService_OnCollectionChanged(LoadedEntriesService sender)
        {
            UpdateText();
        }

        private void _selectedEntryService_OnSelectedEntryChanged(SelectedEntryService sender)
        {
            UpdateText();
        }

        private void UpdateText()
        {
            var currentEntry = _selectedEntryService.CurrentEntry;
            if (currentEntry == null)
            {
                _label.text = string.Empty;
                _label.visible = false;
            }
            else
            {
                var currentIndex = _loadedEntriesService.GetIndexOfEntry(currentEntry);
                var count = _loadedEntriesService.GetCount();
                _label.text = $"({currentIndex + 1}/{count}) {currentEntry.FriendlyName}";
                _label.visible = true;
            }
        }
    }
}