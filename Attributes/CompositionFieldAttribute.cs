using System;

namespace Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class CompositionFieldAttribute : Attribute
    {
        public string[] MappedProperties { get; set; }
        public string[] MappedMethods { get; set; }
        public string[] MappedEvents { get; set; }
    }
}
