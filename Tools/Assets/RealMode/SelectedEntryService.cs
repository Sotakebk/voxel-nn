using UnityEngine;

namespace RealMode
{
    public class SelectedEntryService : MonoBehaviour
    {
        [SerializeReference]
        private LoadedEntriesService _loadedEntriesService;

        public delegate void SelectedEntryChangedEventHandler(SelectedEntryService sender);

        public event SelectedEntryChangedEventHandler? OnSelectedEntryChanged;

        private bool _shouldTriggerEvent = false;
        private readonly object _lock = new object();

        public Entry? CurrentEntry { get; private set; }

        private void Start()
        {
            _loadedEntriesService.OnCollectionChanged += _loadedEntriesService_OnCollectionChanged;
        }

        private void _loadedEntriesService_OnCollectionChanged(LoadedEntriesService sender)
        {
            lock (_lock)
            {
                if (CurrentEntry != null)
                {
                    if (!sender.HasEntry(CurrentEntry))
                    {
                        var anyEntry = _loadedEntriesService.GetEntry(0);
                        if (anyEntry != null)
                        {
                            CurrentEntry = anyEntry;
                            _shouldTriggerEvent = true;
                        }
                        else
                        {
                            CurrentEntry = null;
                            _shouldTriggerEvent = true;
                        }
                    }
                }
                else
                {
                    // is null, try to assign anything!
                    var anyEntry = _loadedEntriesService.GetEntry(0);
                    if (anyEntry != null)
                    {
                        CurrentEntry = anyEntry;
                        _shouldTriggerEvent = true;
                    }
                }
            }
        }

        public void SelectEntry(Entry? entry)
        {
            lock (_lock)
            {
                if (CurrentEntry != entry)
                {
                    CurrentEntry = entry;
                    _shouldTriggerEvent = true;
                }
            }
        }

        private void Update()
        {
            if (_shouldTriggerEvent)
            {
                _shouldTriggerEvent = false;
                OnSelectedEntryChanged?.Invoke(this);
            }
        }
    }
}