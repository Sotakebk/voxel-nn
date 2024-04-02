using UnityEngine;
using UnityEngine.UIElements;

namespace RealMode.UI
{
    public abstract class GenericController : MonoBehaviour
    {
        public abstract void Initialize(VisualElement root);
    }
}