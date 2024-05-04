using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealMode
{
    public class LoadedEntriesService : MonoBehaviour
    {
        public delegate void EntryCollectionsChangedEventHandler(LoadedEntriesService sender);

        public event EntryCollectionsChangedEventHandler? OnCollectionChanged;

        private bool _shouldTriggerEvent = false;
        private readonly object _lock = new object();

        private List<Entry> _entries = new List<Entry>();

        public IEnumerable<Entry> GetEntries()
        {
            lock (_lock)
                return _entries.ToArray();
        }

        public bool HasEntry(Entry entry)
        {
            return _entries.Contains(entry);
        }

        public bool AddEntry(Entry entry)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                if (!_entries.Contains(entry))
                {
                    _entries.Add(entry);
                    valueToReturn = true;
                }
            }
            _shouldTriggerEvent = true;
            return valueToReturn;
        }

        public bool AddEntries(IEnumerable<Entry> entries)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                foreach (var entry in entries)
                {
                    if (!_entries.Contains(entry))
                    {
                        _entries.Add(entry);
                        valueToReturn = true;
                    }
                }
            }
            _shouldTriggerEvent = true;
            return valueToReturn;
        }

        public void MoveEntryUp(Entry entry)
        {
            lock (_lock)
            {
                if (!_entries.Contains(entry))
                    return;

                var index = _entries.IndexOf(entry);
                if (index == 0)
                    return;
                _entries.RemoveAt(index);
                _entries.Insert(Math.Max(0, index - 1), entry);
                _shouldTriggerEvent = true;
            }
        }

        public void MoveEntryDown(Entry entry)
        {
            lock (_lock)
            {
                if (!_entries.Contains(entry))
                    return;

                var index = _entries.IndexOf(entry);
                if (index == _entries.Count)
                    return;

                _entries.RemoveAt(index);
                _entries.Insert(Math.Min(_entries.Count, index + 1), entry);
                _shouldTriggerEvent = true;
            }
        }

        public void RemoveAllEntries()
        {
            lock (_lock)
            {
                if (_entries.Any())
                {
                    _entries.Clear();
                    _shouldTriggerEvent = true;
                }
            }
        }

        public int? GetIndexOfEntry(Entry entry)
        {
            lock (_lock)
            {
                if (!_entries.Contains(entry))
                    return null;

                return _entries.IndexOf(entry);
            }
        }

        public bool RemoveEntry(Entry entry)
        {
            var valueToReturn = false;
            lock (_lock)
            {
                valueToReturn = _entries.Remove(entry);
            }
            if (valueToReturn)
                _shouldTriggerEvent = true;
            return valueToReturn;
        }

        public Entry? GetEntry(int index)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _entries.Count)
                    return _entries[index];
            }
            return null;
        }

        public int GetCount() => _entries.Count;

        private void Update()
        {
            if (_shouldTriggerEvent)
            {
                _shouldTriggerEvent = false;
                OnCollectionChanged?.Invoke(this);
            }
        }
    }
}