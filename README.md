# Moqzilla

Simple automatic mocking using Moq.

### Example

Assuming we have an interface like this:

```csharp
public interface IDependency
{
    int GetTheNumber();
}
```

And a class that consumes it like this:

```csharp
public class MyClass
{
    private readonly IDependency _dependency;

    public MyClass(IDependency dependency)
	{
	    _dependency = dependency;
	}
	
	public int GetDoubleTheNumber()
	{
	    return _dependency.GetTheNumber() * 2;
	}
}
```

Moqzilla works with Moq in an NUnit test like so:

```csharp
[Test]
public void MyTest()
{
    // Arrange.
    var moqzilla = new Moqzilla();
    var myObject = moqzilla.Create<MyClass>();
	
    var mockedDependency = moqzilla.Get<IDependency>();
    mockedDependency.Setup(m => m.GetTheNumber()).Returns(4);
	
	// Act.
	var observedValue = myObject.GetDoubleTheNumber();
	
	// Assert.
	Assert.AreEqual(8, observedValue);
}
```

It will also work if you construct your mock before creating
the object.

### Prerequisites

- Moq
  - 4.2+ for .NET Framework
  - 4.7+ for .NET Standard

### Explicitly supported frameworks

- .NET Framework 4.0
- .NET Framework 4.5
- .NET Standard 1.3
