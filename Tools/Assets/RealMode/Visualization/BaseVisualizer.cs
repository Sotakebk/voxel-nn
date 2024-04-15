using UnityEngine;

namespace RealMode.Visualization
{
    public abstract class BaseVisualizer : MonoBehaviour
    {
        [SerializeReference] protected VisualizationService _visualizationService = null!;
        [SerializeReference] protected PaletteService _paletteService = null!;
        [SerializeReference] protected SelectedEntryService _selectedEntryService = null!;

        private bool _shouldRedrawModel;

        protected virtual void Awake()
        {
            _paletteService.OnPaletteChanged += _paletteService_OnPaletteChanged;
            _selectedEntryService.OnSelectedEntryChanged += _selectedEntryService_OnSelectedEntryChanged;
            Hide();
        }

        private void _selectedEntryService_OnSelectedEntryChanged(SelectedEntryService sender)
        {
            MarkAsDirty();
        }

        private void _paletteService_OnPaletteChanged(PaletteService sender)
        {
            MarkAsDirty();
        }

        protected abstract void Clear();

        protected virtual void Unhide()
        {
            gameObject.SetActive(true);
        }

        protected virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        protected abstract bool CanVisualizeEntry(Entry entry);

        protected abstract void VisualizeEntry(Entry entry);

        protected void MarkAsDirty()
        {
            _shouldRedrawModel = true;
            Unhide();
        }

        protected virtual void LateUpdate()
        {
            if (_shouldRedrawModel)
            {
                _shouldRedrawModel = false;
                var currEntry = _selectedEntryService.CurrentEntry;
                if (currEntry != null && CanVisualizeEntry(currEntry))
                {
                    Clear();
                    VisualizeEntry(currEntry);
                }
                else
                {
                    Clear();
                    Hide();
                }
            }
        }
    }
}