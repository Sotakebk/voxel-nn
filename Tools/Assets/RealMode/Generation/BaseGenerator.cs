using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace RealMode.Generation
{
    public abstract class BaseGenerator : MonoBehaviour
    {
        [GeneratorProperty, Name("Entries to generate"), Index(int.MinValue)]
        public int Instances { get; set; } = 1;

        public abstract string Name { get; }

        public IEnumerable<Entry> Generate()
        {
            var list = new List<Entry>();
            var l = new object();

            Parallel.For(0, Instances, _ =>
            {
                var entry = GenerateOneEntry();
                lock (l)
                    list.Add(entry);
            });

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