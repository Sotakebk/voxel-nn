using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.UI
{
    public class BaseSubController : MonoBehaviour
    {
        public virtual string ViewName => "Unnamed view";

        protected virtual bool IsVisible => view?.visible ?? false;
        protected virtual bool IsDirty { get; set; }

        // set from Unity
        [SerializeField]
        protected VisualTreeAsset viewAsset = null!;

        protected VisualElement view = null!;

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
            view = viewAsset.CloneTree().Q(className: "viewContainer");
            rootElement.Add(view);
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
            view.visible = true;
            view.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            view.visible = false;
            view.style.display = DisplayStyle.None;
        }
    }
}
