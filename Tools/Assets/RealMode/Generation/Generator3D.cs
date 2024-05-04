using UnityEngine;

namespace RealMode.Generation
{
    public abstract class Generator3D : BaseGenerator
    {
        [GeneratorProperty, Name("Entry Size"), Index(int.MinValue)]
        public Vector3Int Size { get; set; }

        protected override void ValidateProperties()
        {
            base.ValidateProperties();
            if (Size.x < 1 || Size.y < 1 || Size.z < 1)
                ThrowOnInvalidProperty(nameof(Size), "No dimension should be less than 1.");
        }

    }
}