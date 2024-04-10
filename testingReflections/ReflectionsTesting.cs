using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Xunit;

namespace testingReflections;

public class ReflectionsTesting
{
    public string GetAttributes(string exerciseName)
    {
        List<string> results = new();
        var exerciseFileName = string.Concat(exerciseName
            .Split('-')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1)));

        var pathToFolder = $"../../../../../../Exercism/csharp/{exerciseName}/";
        var code = File.ReadAllText($"{pathToFolder}/{exerciseFileName}.cs");
        var tests= File.ReadAllText($"{pathToFolder}/{exerciseFileName}Tests.cs");
        
        // var syntaxTreeCode =
        //     CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit(), default, $"{pathToFolder}/{exerciseFileName}.cs");
        
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        // var scriptSyntaxTree = CSharpSyntaxTree.ParseText(code, parseOptions);

        var syntaxTreeTests = CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit(), parseOptions,
            $"{pathToFolder}/{exerciseFileName}Tests.cs");
        
        // var syntaxTreeTests =
        //     CSharpSyntaxTree.ParseText(tests, parseOptions);


        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary);

        var compilation = CSharpCompilation.Create("tempAssembly",
            new[]
            {
                // scriptSyntaxTree,
                // syntaxTreeCode
                syntaxTreeTests
            },
            // references: default,
            new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(FactAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Assert).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(SyntaxTree).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(AttributeUsageAttribute).Assembly.Location),

            },
            options: options
        );

        using var stream = new MemoryStream();


        EmitResult result = compilation.Emit(stream);

        // Check if the compilation was successful
        if (!result.Success)
        {
            // Handle compilation errors
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                Console.WriteLine($"Diagnostics: {diagnostic}");
                return diagnostic.ToString();
            }
        }


        var exerciseAssembly = Assembly.Load(stream.ToArray());
        // return exerciseAssembly.GetTypes().First(t => t.Name.EndsWith("Tests"));
        // Debug.WriteLine(exerciseAssembly.GetTypes().First());

        var assemblyType = exerciseAssembly.GetTypes().First();
        

        return assemblyType.ToString();

        return "ok";
        
        
        // var instance = Activator.CreateInstance(tests);

        // foreach (var method in methods)
        // {
        //     var output = method.Invoke(instance, null);
        //     results.Add(output.ToString());
        // }


        return results[0];
    }
}