using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation
{
    public class EntriesPresenter : BasePresenter
    {
        [SerializeReference] private LoadedEntriesService _loadedEntriesService;
        [SerializeReference] private SelectedEntryService _selectedEntryService;

        public override string ViewName => "Entries";
        [SerializeReference] private VisualTreeAsset _entryTemplate = null!;
        private VisualElement _listContainer = null!;
        private Button _clearButton = null!;

        private void Start()
        {
            _loadedEntriesService.OnCollectionChanged += _loadedEntriesService_OnCollectionChanged;
        }

        private void _loadedEntriesService_OnCollectionChanged(LoadedEntriesService sender)
        {
            RebuildList();
        }

        private void RebuildList()
        {
            _listContainer.Clear();

            var elements = _loadedEntriesService.GetEntries();

            foreach (var element in elements)
            {
                AddListElement(element);
            }
        }

        public override void PrepareView()
        {
            _clearButton = _view.Q<Button>(name: "ClearButton");
            _listContainer = _view.Q(name: "entryListContainer");

            _clearButton.clicked += _clearLogsButton_Clicked;
        }

        private void _clearLogsButton_Clicked()
        {
            _loadedEntriesService.RemoveAllEntries();
        }

        private void AddListElement(Entry entry)
        {
            _listContainer.Add(ConstructListElement(entry));
        }

        private VisualElement ConstructListElement(Entry entry)
        {
            var template = _entryTemplate.CloneTree();
            var nameLabel = template.Q<Label>(name: "NameLabel");
            var dimensionsLabel = template.Q<Label>(name: "DimensionsLabel");
            var tagsLabel = template.Q<Label>(name: "TagsLabel");
            var tagsFoldout = template.Q<Foldout>(name: "TagsFoldout");

            var upButton = template.Q<Button>(name: "Up");
            var downButton = template.Q<Button>(name: "Down");
            var deleteButton = template.Q<Button>(name: "Delete");
            var viewButton = template.Q<Button>(name: "View");

            nameLabel.text = entry.FriendlyName;
            dimensionsLabel.text = entry.IsEntry3D() ? "3D" : "2D";
            tagsLabel.text = string.Join(", ", entry.Tags);
            tagsFoldout.value = false;

            upButton.clicked += () => _loadedEntriesService.MoveEntryUp(entry);
            downButton.clicked += () => _loadedEntriesService.MoveEntryDown(entry);
            deleteButton.clicked += () => _loadedEntriesService.RemoveEntry(entry);
            viewButton.clicked += () => _selectedEntryService.SelectEntry(entry);

            return template;
        }
    }
}