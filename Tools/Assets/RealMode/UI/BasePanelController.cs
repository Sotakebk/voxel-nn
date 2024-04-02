using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.UI
{
    public abstract class BasePanelController : MonoBehaviour
    {
        public abstract string Name { get; }
        protected virtual bool IsVisible => outerPanel?.visible ?? false;
        protected virtual bool IsDirty { get; set; }

        [SerializeField]
        protected VisualTreeAsset viewAsset = null!;

        protected VisualElement outerPanel = null!;
        protected VisualElement innerView = null!;

        public virtual void Toggle()
        {
            if (IsVisible)
                Hide();
            else
                Show();
        }

        public virtual void Initialize(VisualElement panelContainer)
        {
            outerPanel = panelContainer;
            innerView = viewAsset.CloneTree();
            outerPanel.Q(name: "PanelConainerInnerContainer").Add(innerView);
            PrepareView();
        }

        private void LateUpdate()
        {
            if (IsVisible && IsDirty)
            {
                IsDirty = false;
                RegenerateView();
            }
        }

        public virtual void PrepareView()
        {
        }

        public virtual void RegenerateView()
        {
        }

        public virtual void Show()
        {
            outerPanel.visible = true;
            outerPanel.style.display = DisplayStyle.Flex;
        }

        public virtual void Hide()
        {
            outerPanel.visible = false;
            outerPanel.style.display = DisplayStyle.None;
        }
    }
}