﻿using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace testingReflections;

public class ReflectionsTesting
{
    public string GetAttributes(string exerciseName)
    {
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
        var xunitLocation = typeof(Assert).Assembly.Location;
        var factAttributeLocation = typeof(FactAttribute).Assembly.Location;

        var metaReferenceToCurrentAssembly = MetadataReference.CreateFromFile(assemblyLocation);
        var xunitMetaReferenceToCurrentAssembly = MetadataReference.CreateFromFile(xunitLocation);
        var factAttributeMetaReferenceToCurrentAssembly = MetadataReference.CreateFromFile(factAttributeLocation);
        var runtimeMetaReferenceToCurrentAssembly =
            MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location);

        var compilation = CSharpCompilation.Create(null,
            syntaxTrees:
            [
                syntaxTreeCode,
                syntaxTreeTests
            ],
            references:
            [
                metaReferenceToCurrentAssembly, xunitMetaReferenceToCurrentAssembly,
                factAttributeMetaReferenceToCurrentAssembly,
                runtimeMetaReferenceToCurrentAssembly,
            ],
            options: options
        );

        using var stream = new MemoryStream();


        var methodResult = compilation.Emit(stream);

        if (!methodResult.Success)
        {
            foreach (Diagnostic diagnostic in methodResult.Diagnostics)
            {
                Console.WriteLine($"Diagnostics: {diagnostic}");
                return diagnostic.ToString();
            }
        }

        var exerciseAssembly = Assembly.Load(stream.GetBuffer());

        var testClass = exerciseAssembly.GetTypes().First(t => t.Name.EndsWith("Tests"));
        var instance = Activator.CreateInstance(testClass, null);

        var methods = testClass.GetMethods();

        List<string> testResults = new();

        foreach (var test in methods)
        {
            var output = test.Invoke(instance, null);
            testResults.Add(output.ToString());
        }

        return testResults.ToString();
    }
}