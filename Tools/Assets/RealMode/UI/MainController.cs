using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainController : MonoBehaviour
    {
        [SerializeReference] private GenericController[] _genericControllers = null!;
        [SerializeReference] private BasePanelController[] _panelControllers = null!;

        [SerializeReference] protected VisualTreeAsset _rootViewAsset = null!;
        [SerializeReference] protected VisualTreeAsset _panelContainer = null!;

        private void Awake()
        {
            var document = GetComponent<UIDocument>();
            document.visualTreeAsset = _rootViewAsset;
            var content = document.rootVisualElement.Q(name: "Content");
            var buttonContainer = document.rootVisualElement.Q(name: "ButtonContainer");

            foreach (var genericController in _genericControllers)
            {
                genericController.Initialize(document.rootVisualElement);
            }

            foreach (var panelController in _panelControllers)
            {
                var controller = panelController;
                buttonContainer.Add(new Button(() => controller.Toggle())
                {
                    name = $"generatedButton_{panelController.Name}",
                    text = panelController.Name
                });

                var panelContainer = _panelContainer.Instantiate().Q(name: "PanelContainer");
                content.Add(panelContainer);


                panelContainer.Q<Button>(name: "Close").clicked += () => controller.Hide();
                panelContainer.Q<Label>(name: "Title").text = panelController.Name;
                panelController.Initialize(panelContainer);
                panelController.Hide();
            }
        }
    }
}