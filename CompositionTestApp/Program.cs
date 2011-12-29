using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Attributes;

namespace CompositionTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            GenerateWrapperCode();
            Console.ReadLine();
        }

        private static void GenerateWrapperCode()
        {
            var assembly = Assembly.GetAssembly(typeof(ComposableObjects.ClassA));
            var currentNamespace = string.Empty;
            var nameSpaceMustBeClosed = false;

            foreach (var type in assembly.GetTypes().OrderBy(x => x.Namespace))
            {
                var members = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(member => member.GetCustomAttributes(typeof(CompositionFieldAttribute), true).Length > 0)
                    .ToArray();

                if (members.Length <= 0) continue;

                if (type.Namespace != currentNamespace)
                {
                    if (nameSpaceMustBeClosed)
                    {
                        Console.WriteLine("}");
                    }
                    Console.WriteLine("namespace " + type.Namespace);
                    Console.WriteLine("{");
                    nameSpaceMustBeClosed = true;
                }

                ProcessType(type, members);
            }

            if (nameSpaceMustBeClosed)
            {
                Console.WriteLine("}");
            }
        }

        private static void ProcessType(Type type, IEnumerable<FieldInfo> members)
        {
            Console.WriteLine(string.Format("\t{0} partial class {1}",
                                                type.IsPublic ? "public " : "private", type.Name));
            Console.WriteLine("\t{");
            foreach (var member in members)
            {
                ProcessMember(member);
            }
            Console.WriteLine("\t}");
        }

        private static void ProcessMember(FieldInfo member)
        {
            var attributes = member.GetCustomAttributes(typeof(CompositionFieldAttribute), true);
            foreach (var att in attributes.OfType<CompositionFieldAttribute>())
            {
                WriteEvents(member, att.MappedEvents);
                WriteProperties(member, att.MappedProperties);
                WriteMethods(member, att.MappedMethods);
            }
        }

        private static void WriteEvents(FieldInfo field, ICollection<string> events)
        {
            if (events == null || events.Count == 0) return;
            
            var attachEvents = new List<string>();
            var memberName = field.Name;

            var eventList = field.FieldType
                .GetEvents()
                .Where(x => events.Contains(x.Name))
                .ToList();
            if (eventList.Count <= 0) return;

            foreach (var ev in eventList)
            {
                WriteEvent(memberName, ev);
                attachEvents.Add(string.Format("{0}.{1} += Handle{0}{1};\n", memberName, ev.Name));
            }
            Console.WriteLine("\t\tprivate void AttachEvents()");
            Console.WriteLine("\t\t{");
            foreach (var attachEvent in attachEvents)
            {
                Console.WriteLine("\t\t\t" + attachEvent);
            }
            Console.WriteLine("\t\t}\n");
            Console.WriteLine("\t\tprivate void DetachEvents()");
            Console.WriteLine("\t\t{");
            foreach (var attachEvent in attachEvents)
            {
                Console.WriteLine("\t\t\t" + attachEvent.Replace("+=", "-="));
            }
            Console.WriteLine("\t\t}\n");
        }

        private static void WriteEvent(string memberName, EventInfo eventInfo)
        {
            var eht = eventInfo.EventHandlerType;

            var eventArgs = "EventArgs";
            var genericArguments = string.Empty;
            if (eht.IsGenericType)
            {
                var genArgs = eht.GetGenericArguments();
                genericArguments = "<" + string.Join(", ", genArgs.Select(x => x.FullName)) + ">";
                if (genArgs.Length > 0) eventArgs = genArgs[0].FullName;
            }
            Console.WriteLine("\t\tpublic event {0}{1} {2};\n", eventInfo.EventHandlerType.Name.Replace("`1", string.Empty), genericArguments, eventInfo.Name);
            Console.WriteLine("\t\tprivate void Raise{0}{1}({2} ea)", memberName, eventInfo.Name, eventArgs);
            Console.WriteLine("\t\t{");
            Console.WriteLine("\t\t\tvar ev = {0};", eventInfo.Name);
            Console.WriteLine("\t\t\tif (ev != null) { ev(this, ea); }");
            Console.WriteLine("\t\t}\n");
            Console.WriteLine("\t\tprivate void Handle{0}{1}(object sender, {2} ea) {{ Raise{1}(ea); }}\n", memberName, eventInfo.Name, eventArgs);  
        }

        private static void WriteProperties(FieldInfo field, ICollection<string> properties)
        {
            if (properties == null || properties.Count == 0) return;
            
            var memberName = field.Name;

            field.FieldType
                .GetProperties()
                .Where(x => properties.Contains(x.Name))
                .ToList()
                .ForEach(x => WriteProperty(memberName, x));
        }

        private static void WriteProperty(string memberName, PropertyInfo propInfo)
        {
            Console.WriteLine("\t\tpublic {0} {1}", GetCSharpTypeName(propInfo.PropertyType), propInfo.Name);
            Console.WriteLine("\t\t{");
            var accessors = propInfo.GetAccessors();
            if (accessors.Count(x => x.Name.StartsWith("get_")) > 0)
            {
                Console.WriteLine("\t\t\tget {{ return {0}.{1}; }}", memberName, propInfo.Name);
            }
            if (accessors.Count(x => x.Name.StartsWith("set_")) > 0)
            {
                Console.WriteLine("\t\t\tset {{ {0}.{1} = value; }}", memberName, propInfo.Name);
            }
            Console.WriteLine("\t\t}");
        }

        private static void WriteMethods(FieldInfo field, ICollection<string> methods)
        {
            if (methods == null || methods.Count == 0) return;
            var memberName = field.Name;
            field.FieldType
                .GetMethods()
                .Where(x => methods.Contains(x.Name))
                .ToList()
                .ForEach(x => WriteMethod(memberName, x));
        }

        private static void WriteMethod(string memberName, MethodInfo method)
        {
            var prms = method.GetParameters();

            var genericArguments = string.Empty;
            if (method.IsGenericMethodDefinition)
            {
                var genprm = method.GetGenericArguments();
                genericArguments = "<" + string.Join(", ", genprm.Select(x => x.Name)) + ">";
            }

            Console.WriteLine("\t\tpublic {0} {1}{2}({3})",
                GetCSharpTypeName(method.ReturnType),
                method.Name,
                genericArguments,
                string.Join(", ", prms.Select(x => GetCSharpTypeName(x.ParameterType) + " " + x.Name)));
            Console.WriteLine("\t\t{");
            Console.WriteLine("\t\t\t{0}{1}.{2}({3});",
                              method.ReturnType == typeof(void) ? string.Empty : "return ",
                              memberName,
                              method.Name,
                              string.Join(", ", prms.Select(x => x.Name)));
            Console.WriteLine("\t\t{");
        }

        private static string GetCSharpTypeName(Type type)
        {
            switch (type.Name)
            {
                case "Boolean": return "bool";
                case "Byte": return "byte";
                case "Char": return "char";
                case "DateTime": return "DateTime";
                case "Decimal": return "decimal";
                case "Double": return "double";
                case "Void": return "void";
                case "Int16": return "short";
                case "Int32": return "int";
                case "Int64": return "long";
                case "SByte": return "sbyte";
                case "Single": return "float";
                case "String": return "string";
                case "UInt16": return "ushort";
                case "UInt32": return "uint";
                case "UInt64": return "ulong";
                default: return type.Name;
            }
        }

    }
}
