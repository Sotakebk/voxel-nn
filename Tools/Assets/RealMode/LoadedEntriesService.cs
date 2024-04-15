using System.Collections.Generic;
using UnityEngine;

namespace RealMode
{
    public class LoadedEntriesService : MonoBehaviour
    {
        public delegate void EntryCollectionsChangedEventHandler(LoadedEntriesService sender);

        public event EntryCollectionsChangedEventHandler? OnCollectionsChanged;

        private bool _shouldTriggerEvent = false;
        private readonly object _lock = new object();

        private List<EntryCollection> _entryCollections = new List<EntryCollection>();

        public IEnumerable<EntryCollection> GetCurrentlyLoadedEntryCollections()
        {
            lock (_lock)
                return _entryCollections.ToArray();
        }

        public bool AddEntryCollection(EntryCollection collection)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                if (!_entryCollections.Contains(collection))
                {
                    _entryCollections.Add(collection);
                    valueToReturn = true;
                }
            }
            _shouldTriggerEvent = true;
            return valueToReturn;
        }

        public bool RemoveEntryCollection(EntryCollection collection)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                valueToReturn = _entryCollections.Remove(collection);
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
                OnCollectionsChanged?.Invoke(this);
            }
        }
    }
}