using System.Collections.Generic;
using UnityEngine;

namespace RealMode
{
    public class LoadedEntriesService : MonoBehaviour
    {
        private List<Entry3D> _loadedEntries = new List<Entry3D>();
        private object _lock = new object();

        public delegate void LoadedEntriesUpdatedEventHandler(LoadedEntriesService sender);

        public event LoadedEntriesUpdatedEventHandler? OnEntriesUpdated;

        private bool _shouldTriggerEvent = false;


        public IEnumerable<Entry3D> GetLoadedEntries()
        {
            lock (_lock)
                return _loadedEntries.ToArray();
        }

        public bool AddEntry(Entry3D entry)
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

        public bool RemoveEntry(Entry3D entry)
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