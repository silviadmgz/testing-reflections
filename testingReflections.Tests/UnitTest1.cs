using FluentAssertions;

namespace testingReflections.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var instance = new ReflectionsTesting();
        var result = instance.GetAttributes("hello-world");

        result.Should().Be("Silvia");
    }
}