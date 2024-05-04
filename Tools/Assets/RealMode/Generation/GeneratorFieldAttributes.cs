using System;

namespace RealMode.Generation
{
    public class GeneratorPropertyAttribute : Attribute
    {
        public GeneratorPropertyAttribute()
        {
        }
    }

    public class NameAttribute : Attribute
    {
        public string Name { get; private set; }

        public NameAttribute(string name)
        {
            Name = name;
        }
    }
    public class IndexAttribute : Attribute
    {
        public int Index { get; private set; }

        public IndexAttribute(int index)
        {
            Index = index;
        }
    }
}