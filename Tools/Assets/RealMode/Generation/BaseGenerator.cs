using System.Collections.Generic;
using UnityEngine;

namespace RealMode.Generation
{
    public abstract class BaseGenerator : MonoBehaviour
    {
        [GeneratorProperty, Name("Entries to generate"), Index(int.MinValue)]
        public int Instances { get; set; }

        public abstract string Name { get; }

        public IEnumerable<Entry> Generate()
        {
            var list = new List<Entry>();
            for (int i = 0; i < Instances; i++)
            {
                list.Add(GenerateOneEntry());
            }
            return list;
        }

        protected abstract Entry GenerateOneEntry();

        protected virtual void ValidateProperties()
        {

        }

        protected virtual void ThrowOnInvalidProperty(string propertyName, string? reason = null)
        {
            var str = $"Failed validation for '{propertyName}'";
            if (reason == null)
            {
                str += $", reason: '{reason}'.";
            }
            else
            {
                str += ".";
            }

            throw new System.Exception(str);
        }
    }
}