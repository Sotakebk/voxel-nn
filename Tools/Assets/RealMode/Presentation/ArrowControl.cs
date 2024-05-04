using System;
using UnityEngine;

namespace RealMode.Presentation
{

    public class ArrowControl : MonoBehaviour
    {
        [SerializeReference]
        private SelectedEntryService _selectedEntryService;
        [SerializeReference]
        private LoadedEntriesService _loadedEntriesService;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveVertical(false);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveVertical(true);
            }
        }

        private void MoveVertical(bool up)
        {
            var count = _loadedEntriesService.GetCount();
            var currentEntry = _selectedEntryService.CurrentEntry;
            if (currentEntry == null)
            {
                if (count != 0)
                {
                    var newEntry = _loadedEntriesService.GetEntry(0);
                    _selectedEntryService.SelectEntry(newEntry);
                    Debug.Log($"Selecting from nothing to something! new: {newEntry}");
                }
            }
            else
            {
                var currentIndex = _loadedEntriesService.GetIndexOfEntry(currentEntry) ?? 0;
                var targetIndex = currentIndex + (up ? 1 : -1);
                targetIndex = Math.Clamp(targetIndex, 0, count - 1);
                if (targetIndex != currentIndex)
                    _selectedEntryService.SelectEntry(_loadedEntriesService.GetEntry(targetIndex));
            }
        }
    }
}