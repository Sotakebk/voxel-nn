using UnityEngine;

namespace RealMode.Generation
{
    public abstract class Generator2D : BaseGenerator
    {
        [GeneratorProperty, Name("Entry Size"), Index(int.MinValue)]
        public Vector2Int Size { get; set; } = new Vector2Int(64, 64);

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (Size.x < 1 || Size.y < 1)
                ThrowOnInvalidProperty(nameof(Size), "No dimension should be less than 1.");
        }
    }
}
