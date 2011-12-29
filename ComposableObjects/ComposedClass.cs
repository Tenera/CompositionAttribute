using Attributes;

namespace ComposableObjects
{
    public partial class ComposedClass
    {
        [CompositionField(MappedProperties = new[] { "PropA", "PropB", "IsPropC" },
                                    MappedMethods = new[] { "DoStuff", "GetCount", "DoGenericStuff" },
                                    MappedEvents = new[] { "DoStuffEvent", "SomeEvent" })]
        private readonly ClassA _classA;

        public ComposedClass(int a, int b, bool c)
        {
            _classA = new ClassA(a, b, c);
        }
    }
}
