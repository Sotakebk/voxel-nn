using Newtonsoft.Json;
using RealMode.Data;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation.SerializationView
{
    public class SerializationPanelPresenter : BasePresenter
    {
        [SerializeReference]
        private LoadedEntriesService _loadedEntriesService;
        [SerializeReference]
        private SelectedEntryService _selectedEntryService;


        public override string ViewName => "Serialization";

        private Label _errorLabel;
        private Label _clipboardTextLabel;

        public override void PrepareView()
        {
            var deserializeReplaceButton = _view.Q<Button>("DeserializeReplace");
            var deserializeAppendButton = _view.Q<Button>("DeserializeAppend");
            var serializeActiveButton = _view.Q<Button>("SerializeActive");
            var serializeAllButton = _view.Q<Button>("SerializeAll");
            var clearButton = _view.Q<Button>("Clear");
            _errorLabel = _view.Q<Label>("ErrorLabel");
            _clipboardTextLabel = _view.Q<Label>("PastedText");

            deserializeAppendButton.clicked += DeserializeAppendButton_clicked;
            deserializeReplaceButton.clicked += DeserializeReplaceButton_clicked;
            serializeActiveButton.clicked += SerializeActiveButton_clicked;
            serializeAllButton.clicked += SerializeAllButton_clicked;
            clearButton.clicked += ClearButton_clicked;
            ClearButton_clicked();
        }

        private void SetErrorText(Exception exception)
        {
            SetErrorText($"{exception.GetType().Name} {exception.Message}");
        }

        private void SetErrorText(string? text = null)
        {
            if (text == null)
            {
                _errorLabel.text = string.Empty;
                _errorLabel.style.display = DisplayStyle.None;
            }
            else
            {
                _errorLabel.text = text;
                _errorLabel.style.display = DisplayStyle.Flex;
            }
        }

        private void SaveTextToClipboard(string json)
        {
            var forContext = json.Length > 1000 ? json.Substring(0, 1000) + "..." : json;
            _clipboardTextLabel.text = $"Text copied to clipboard! Contains:\n{forContext}";
            GUIUtility.systemCopyBuffer = json;
        }

        private string GetTextFromClipboard()
        {
            var json = GUIUtility.systemCopyBuffer;
            var forContext = json.Length > 1000 ? json.Substring(0, 1000) + "..." : json;
            _clipboardTextLabel.text = $"Text copied from clipboard! Contains:\n{forContext}";
            return json;
        }

        private void ClearButton_clicked()
        {
            SetErrorText();
            _clipboardTextLabel.text = "Clipboard text will be shown here...";
        }

        private void SerializeAllButton_clicked()
        {
            try
            {

                var allEntries = _loadedEntriesService.GetEntries();
                string json = JsonConvert.SerializeObject(allEntries.Select(e => e.ToDTO()).ToArray());
                SaveTextToClipboard(json);
                SetErrorText();
            }
            catch (Exception ex)
            {
                SetErrorText(ex);
                Debug.LogException(ex);
            }
        }

        private void SerializeActiveButton_clicked()
        {
            try
            {
                var currentEntry = _selectedEntryService.CurrentEntry;
                string json = JsonConvert.SerializeObject(currentEntry?.ToDTO());
                SaveTextToClipboard(json);
                SetErrorText();
            }
            catch (Exception ex)
            {
                SetErrorText(ex);
                Debug.LogException(ex);
            }

        }

        private void DeserializeReplaceButton_clicked()
        {
            _loadedEntriesService.RemoveAllEntries();
            DeserializeAndAdd();
        }

        private void DeserializeAppendButton_clicked()
        {
            DeserializeAndAdd();
        }

        private void DeserializeAndAdd()
        {
            var json = GetTextFromClipboard();
            Exception? e = null;
            try
            {
                var array = DeserializeEntryArray(json);
                if (array != null)
                {
                    _loadedEntriesService.AddEntries(array);
                    return;
                }
            }
            catch (Exception ex)
            {
                e = ex;
            }

            try
            {
                var entry = DeserializeEntry(json);
                if (entry != null)
                {
                    _loadedEntriesService.AddEntry(entry);
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(e);
                Debug.LogException(ex);
            }
        }

        private Entry[]? DeserializeEntryArray(string json)
        {
            var dtos = JsonConvert.DeserializeObject<EntryDTO[]>(json);
            foreach (var d in dtos)
            {
                var (failed, message) = DataHelper.Validate(d);
                if (failed)
                {
                    SetErrorText($"Failed to deserialize '{d.FriendlyName}': '{message}'.");
                    return null;
                }
            }

            return dtos.Select(d => DataHelper.ToEntryObject(d)).ToArray();
        }

        private Entry? DeserializeEntry(string json)
        {
            var d = JsonConvert.DeserializeObject<EntryDTO>(json);
            var (failed, message) = DataHelper.Validate(d);
            if (failed)
            {
                SetErrorText($"Failed to deserialize '{d.FriendlyName}': '{message}'.");
                return null;
            }
            return DataHelper.ToEntryObject(d);
        }
    }
}