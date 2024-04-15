using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation
{
    public class BasePresenter : MonoBehaviour
    {
        public virtual string ViewName => "Unnamed view";
        protected virtual bool IsVisible => _view?.visible ?? false;
        protected virtual bool IsDirty { get; set; }

        // set from Unity
        [SerializeField] protected VisualTreeAsset viewAsset = null!;

        protected VisualElement _view = null!;

        private void LateUpdate()
        {
            if (IsVisible && IsDirty)
            {
                IsDirty = false;
                RegenerateView();
            }
        }

        public virtual void InstantiateView(VisualElement rootElement)
        {
            _view = viewAsset.CloneTree().Q(className: "viewContainer");
            rootElement.Add(_view);
            PrepareView();
        }

        public virtual void PrepareView()
        {
        }

        public virtual void RegenerateView()
        {
        }

        public virtual void Show()
        {
            _view.visible = true;
            _view.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            _view.visible = false;
            _view.style.display = DisplayStyle.None;
        }
    }
}