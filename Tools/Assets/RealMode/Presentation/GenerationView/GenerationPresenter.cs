using RealMode.Generation;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.Presentation
{
    public class GenerationPresenter : BasePresenter
    {
        [SerializeReference] private LoadedEntriesService _loadedEntriesService;

        [SerializeReference]
        private BaseGenerator[] Generators;

        private BaseGenerator? _selectedGenerator;

        public override string ViewName => "Generation";
        private VisualElement _parameterContainer = null!;
        private Button _generateButton = null!;
        private Label _generatorNameLabel = null!;

        public override void PrepareView()
        {
            _generateButton = _view.Q<Button>(name: "GenerateButton");
            _parameterContainer = _view.Q(name: "ParameterContainer");
            _generatorNameLabel = _view.Q<Label>(name: "GeneratorName");
            var generatorButtonsContainer = _view.Q(name: "ButtonContainer");

            _generateButton.clicked += Generate;

            foreach (var generator in Generators)
            {
                var button = new Button()
                {
                    text = generator.Name
                };
                button.clicked += () => SelectGenerator(generator);
                generatorButtonsContainer.Add(button);
            }
        }

        private void Generate()
        {
            if (_selectedGenerator != null)
            {
                var result = _selectedGenerator.Generate();
                _loadedEntriesService.AddEntries(result);
            }
        }

        private void SelectGenerator(BaseGenerator generator)
        {
            _selectedGenerator = generator;
            _generatorNameLabel.text = generator.Name;
            _parameterContainer.Clear();

            var props = generator
                .GetType()
                .GetProperties()
                .Where(p => p.GetCustomAttribute<GeneratorPropertyAttribute>() != null)
                .OrderBy(p => p.GetCustomAttribute<IndexAttribute>()?.Index ?? 0);

            foreach (var property in props)
            {
                if (property.PropertyType == typeof(int))
                {
                    AddIntegerControl(generator, property);
                }
                else if (property.PropertyType == typeof(Vector3Int))
                {
                    AddVector3IntControl(generator, property);
                }
                else
                {
                    AddUnknownPropertyTypeControl(generator, property);
                }
                Debug.Log($"Added property {GetPropertyName(property)}");
            }
        }

        private static string GetPropertyName(PropertyInfo property)
        {
            var nameAttrib = property.GetCustomAttribute<NameAttribute>();
            return nameAttrib?.Name ?? property.Name;
        }

        private void AddIntegerControl(BaseGenerator generator, PropertyInfo property)
        {
            var control = new IntegerField(GetPropertyName(property));
            control.value = (int)property.GetValue(generator);
            control.RegisterValueChangedCallback((arg) => property.SetValue(generator, arg.newValue));
            _parameterContainer.Add(control);
        }

        private void AddVector3IntControl(BaseGenerator generator, PropertyInfo property)
        {
            var control = new Vector3IntField(GetPropertyName(property));
            control.value = (Vector3Int)property.GetValue(generator);
            control.RegisterValueChangedCallback((arg) => property.SetValue(generator, arg.newValue));
            _parameterContainer.Add(control);
        }

        private void AddUnknownPropertyTypeControl(BaseGenerator generator, PropertyInfo property)
        {
            var label = new Label();
            label.text = $"Unknown property type for '{GetPropertyName(property)}'";
            _parameterContainer.Add(label);
        }
    }
}