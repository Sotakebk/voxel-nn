using System.Collections.Generic;
using UnityEngine;

namespace RealMode
{
    public class LoadedEntriesService : MonoBehaviour
    {
        private List<Entry> _loadedEntries = new List<Entry>();
        private object _lock = new object();

        public delegate void LoadedEntriesUpdatedEventHandler(LoadedEntriesService sender);

        public event LoadedEntriesUpdatedEventHandler? OnEntriesUpdated;

        private bool _shouldTriggerEvent = false;


        public IEnumerable<Entry> GetLoadedEntries()
        {
            lock (_lock)
                return _loadedEntries.ToArray();
        }

        public bool AddEntry(Entry entry)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                if (!_loadedEntries.Contains(entry))
                {
                    _loadedEntries.Add(entry);
                    valueToReturn = true;
                }
            }
            _shouldTriggerEvent = true;
            return valueToReturn;
        }

        public bool RemoveEntry(Entry entry)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                valueToReturn = _loadedEntries.Remove(entry);
            }
            if (valueToReturn)
                _shouldTriggerEvent = true;
            return valueToReturn;
        }

        private void Update()
        {
            if (_shouldTriggerEvent)
            {
                _shouldTriggerEvent = false;
                OnEntriesUpdated?.Invoke(this);
            }
        }
    }
}