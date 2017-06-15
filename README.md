# Moqzilla

Simple automatic mocking using Moq.

### Example Setup

Assume we have an interface like this:

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

### General Usage

Moqzilla works with Moq in an NUnit test like so:

```csharp
[Test]
public void MyTest()
{
    // Arrange.
    var mocker = new Mocker();
    var myObject = mocker.Create<MyClass>();
    
    var mockedDependency = mocker.Mock<IDependency>();
    mockedDependency.Setup(m => m.GetTheNumber()).Returns(4);
    
    // Act.
    var observedValue = myObject.GetDoubleTheNumber();

    // Assert.
    Assert.AreEqual(8, observedValue);
}
```

It will also work if you construct your mock before creating
the object.

### Mock Blocks

To keep mock setup more isolated, configuration blocks can be used like so:

```csharp
[Test]
public void MyTest()
{
    // Arrange.
    var mocker = new Mocker();
    var myObject = mocker.Create<MyClass>();
    
    mocker.Mock<IDependency>(mock => {
        mock.Setup(m => m.GetTheNumber()).Returns(4);
    });
    
    // Act.
    var observedValue = myObject.GetDoubleTheNumber();

    // Assert.
    Assert.AreEqual(8, observedValue);
}
```

### Activation

Mocks can be configured as objects are created. Activators for a dependency are run
each time `Mocker.Create<T>` is called, if they are in the constructor.

```csharp
[Test]
public void MyTest()
{
    // Arrange.
    var mocker = new Mocker();
    var myObject = mocker.Create<MyClass>();
    
    mocker.Activate<IDependency>(mock => mock.Setup(m => m.GetTheNumber()).Returns(4));
    
    // Act.
    var observedValue = myObject.GetDoubleTheNumber();

    // Assert.
    Assert.AreEqual(8, observedValue);
}
```

### Prerequisites

- Moq
  - 4.2+ for .NET Framework
  - 4.7+ for .NET Standard

### Explicitly supported frameworks

- .NET Framework 4.0
- .NET Framework 4.5
- .NET Standard 1.3
