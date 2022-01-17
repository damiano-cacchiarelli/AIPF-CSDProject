using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AIPF.Reflection
{
    public static class DynamicType
    {
        private static readonly string NAMESPACE = "AIPF.Reflection";
        private static readonly string CLASSNAME = "DynamicClass";

        private static int id = 0;
        private static string ClassName => id == 0 ? CLASSNAME : CLASSNAME + id;

        /// <summary>
        /// Creates a list of the specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<object> CreateDynamicList(Type type)
        {
            var listType = typeof(List<>);
            var dynamicListType = listType.MakeGenericType(type);
            return (IEnumerable<object>)Activator.CreateInstance(dynamicListType);
        }

        /// <summary>
        /// creates an action which can be used to add items to the list
        /// </summary>
        /// <param name="listType"></param>
        /// <returns></returns>
        public static Action<object[]> GetAddAction(IEnumerable<object> list)
        {
            var listType = list.GetType();
            var addMethod = listType.GetMethod("Add");
            var itemType = listType.GenericTypeArguments[0];
            var itemProperties = itemType.GetProperties();

            var action = new Action<object[]>((values) =>
            {
                var item = Activator.CreateInstance(itemType);

                for (var i = 0; i < values.Length; i++)
                {
                    itemProperties[i].SetValue(item, values[i]);
                }

                addMethod.Invoke(list, new[] { item });
            });

            return action;
        }

        /// <summary>
        /// Creates a type based on the property/type values specified in the properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Type CreateDynamicType(IEnumerable<DynamicTypeProperty> properties, params Type[] interfaces)
        {
            var classCode = GenerateClassCode(properties, interfaces);
            var syntaxTree = CSharpSyntaxTree.ParseText(classCode);
            var references = GetReferences(interfaces);

            var compilation = CSharpCompilation.Create($"{ClassName}{Guid.NewGuid()}.dll",
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                var result = compilation.Emit(ms);

                if (!result.Success)
                {
                    var failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    var message = new StringBuilder();

                    foreach (var diagnostic in failures)
                    {
                        message.AppendFormat("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                    }

                    throw new Exception($"Invalid property definition: {message}.");
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
                    var dynamicType = assembly.GetType($"{NAMESPACE}.{ClassName}");
                    id++;
                    return dynamicType;
                }
            }
        }

        private static string GenerateClassCode(IEnumerable<DynamicTypeProperty> properties, Type[] interfaces)
        {
            StringBuilder classCode = new StringBuilder();

            classCode.AppendLine("using System;");
            foreach (var interfaceName in interfaces)
            {
                classCode.AppendLine($"using {interfaceName.Namespace};");
            }
            classCode.AppendLine($"namespace {NAMESPACE} {{");
            classCode.AppendLine($"public class {ClassName} {(interfaces.Length > 0 ? ":" : "")}");

            for (var j = 0; j < interfaces.Length; j++)
            {
                var className = interfaces[j].Name;
                var i = className.IndexOf("`");
                if (i > 0)
                {
                    className = $"{className.Remove(i)}<{ClassName}>";
                }
                classCode.Append($"{className} {(j == interfaces.Length - 1 ? "" : ",")}");
            }
            classCode.Append("{");

            foreach (var property in properties)
            {
                classCode.AppendLine($"public {property.Type.Name} {property.Name} {{get; set; }}");
            }
            /*
            foreach (var interfaceType in interfaces)
            {
                Attribute[] attrs = Attribute.GetCustomAttributes(interfaceType, typeof(Reflectable));
                foreach (Reflectable attr in attrs)
                {
                    if (attr.HasReflectionCode())
                    {
                        classCode.AppendLine(attr.GetReflectionCode());
                    }
                }
            }
            */
            classCode.AppendLine("}");
            classCode.AppendLine("}");

            return classCode.ToString();
        }

        private static MetadataReference[] GetReferences(Type[] interfaces)
        {
            var references = new List<MetadataReference>()
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DictionaryBase).GetTypeInfo().Assembly.Location)
            };
            references.AddRange(
                interfaces.Select(interfaceType => MetadataReference.CreateFromFile(interfaceType.GetTypeInfo().Assembly.Location)));
            return references.ToArray();
        }
    }
}