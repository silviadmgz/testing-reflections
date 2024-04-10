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
        var tests = File.ReadAllText($"{pathToFolder}/{exerciseFileName}Tests.cs");

        var syntaxTreeCode = SyntaxFactory.ParseSyntaxTree(code);

        var syntaxTreeTests = SyntaxFactory.ParseSyntaxTree(tests);

        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary);

        var assemblyLocation = typeof(object).Assembly.Location;
        var metaReferenceToCurrentAssembly = MetadataReference.CreateFromFile(assemblyLocation);

        var compilation = CSharpCompilation.Create(null,
            syntaxTrees: [
                syntaxTreeCode, 
                // syntaxTreeTests
            ],
            references: [metaReferenceToCurrentAssembly],
            options: options
        );

        using var stream = new MemoryStream();


        var methodResult = compilation.Emit(stream);

        // Check if the compilation was successful
        if (!methodResult.Success)
        {
            // Handle compilation errors
            foreach (Diagnostic diagnostic in methodResult.Diagnostics)
            {
                Console.WriteLine($"Diagnostics: {diagnostic}");
                return diagnostic.ToString();
            }
        }


        var exerciseAssembly = Assembly.Load(stream.GetBuffer());
        // return exerciseAssembly.GetTypes().First(t => t.Name.EndsWith("Tests"));
        // Debug.WriteLine(exerciseAssembly.GetTypes().First());

        var assemblyType = exerciseAssembly.GetTypes().First();
        var method = assemblyType.GetMethods().First();
        var output = method.Invoke(null, null);
        return output.ToString();
    }
}