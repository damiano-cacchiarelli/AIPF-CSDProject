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

namespace AIPF
{
    /// <summary>
    /// A property name, and type used to generate a property in the dynamic class.
    /// </summary>
    public class DynamicTypeProperty
    {
        public DynamicTypeProperty(string name, Type type)
        {
            Name = name;
            Type = type;
        }
        public string Name { get; set; }
        public Type Type { get; set; }
    }

    public static class DynamicType
    {
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
            StringBuilder classCode = new StringBuilder();

            // Generate the class code
            classCode.AppendLine("using System;");
            foreach (var interfaceName in interfaces)
            {
                classCode.AppendLine($"using {interfaceName.Namespace};");
            }
            classCode.AppendLine("namespace Dexih {");
            classCode.AppendLine($"public class DynamicClass {(interfaces.Length > 0 ? ":" : "")}");

            for (var j = 0; j < interfaces.Length; j++)
            {
                var className = interfaces[j].Name;
                var i = className.IndexOf("`");
                if (i > 0)
                {
                    className = className.Remove(i) + "<DynamicClass>";
                }
                classCode.Append($"{className} {(j == interfaces.Length-1 ? "" : ",")}");
            }
            classCode.Append("{");

            foreach (var property in properties)
            {
                classCode.AppendLine($"public {property.Type.Name} {property.Name} {{get; set; }}");
            }
            classCode.AppendLine("}");
            classCode.AppendLine("}");

            var syntaxTree = CSharpSyntaxTree.ParseText(classCode.ToString());

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DictionaryBase).GetTypeInfo().Assembly.Location)
            };
            var intRef = new List<MetadataReference>();
            foreach (var interfaceType in interfaces)
            {
                intRef.Add(MetadataReference.CreateFromFile(interfaceType.GetTypeInfo().Assembly.Location));
            }
            intRef.AddRange(references);
            references = intRef.ToArray();

            var compilation = CSharpCompilation.Create("DynamicClass" + Guid.NewGuid() + ".dll",
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
                    var dynamicType = assembly.GetType("Dexih.DynamicClass");
                    return dynamicType;
                }
            }
        }
    }
}