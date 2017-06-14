using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

// ReSharper disable UnusedMethodReturnValue.Local
// ReSharper disable UnusedParameter.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Moqzilla.Test
{
    [TestFixture]
    public class MoqzillaTestFixture
    {

        #region Test Subject Classes

        /// <summary>
        /// Used as a target for testing a constructor without dependencies.
        /// </summary>
        private class TestSubjectWithNoDependencies
        {
        }

        /// <summary>
        /// Used as a target for testing a constructor with a single dependency.
        /// </summary>
        private class TestSubjectWithSingleDependencyExample
        {
            private readonly IDisposable _disposable;

            public TestSubjectWithSingleDependencyExample(
                IDisposable disposable)
            {
                _disposable = disposable;
            }

            public void Dispose()
            {
                _disposable.Dispose();
            }
        }

        /// <summary>
        /// Used as a target for testing a constructor with multiple dependencies.
        /// </summary>
        private class TestSubjectWithMultipleDependenciesExample
        {
            private readonly IEnumerable<string> _enumerable;

            public TestSubjectWithMultipleDependenciesExample(
                IDisposable disposable, 
                IComparable comparable, 
                IEnumerable<string> enumerable)
            {
                _enumerable = enumerable;
            }

            public IEnumerator<string> GetEnumerator()
            {
                return _enumerable.GetEnumerator();
            }
        }

        /// <summary>
        /// Used as a target for testing a class without constructors that can be fully mocked.
        /// </summary>
        private class TestSubjectWithoutValidConstructorExample
        {
            public TestSubjectWithoutValidConstructorExample(string a)
            {
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Create_Throws_WhenAttemptingToCreateClassWithoutMockableDependencies()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            Action act = () =>
                moqzilla.Create<TestSubjectWithoutValidConstructorExample>();

            // Assert.
            act.ShouldThrow<MoqzillaException>()
                .WithMessage("Moqzilla could not find constructors that consist entirely of interfaces.");
        }

        [Test]
        public void Create_DoesNotThrow_WhenNoMocksRequired()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            Action act = () =>
                moqzilla.Create<TestSubjectWithNoDependencies>();

            // Assert.
            act.ShouldNotThrow();
        }

        [Test]
        public void Create_DoesNotThrow_WhenMockingSingleDependency()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            Action act = () => 
                moqzilla.Create<TestSubjectWithSingleDependencyExample>();

            // Assert.
            act.ShouldNotThrow();
        }

        [Test]
        public void Create_DoesNotThrow_WhenMockingMultipleDependencies()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            Action act = () =>
                moqzilla.Create<TestSubjectWithMultipleDependenciesExample>();

            // Assert.
            act.ShouldNotThrow();
        }

        [Test]
        public void Create_ReturnsCorrectType()
        {
            // Arrange.
            var moqzilla = new Moqzilla();
            
            // Act.
            var obj = moqzilla.Create<TestSubjectWithMultipleDependenciesExample>();

            // Assert.
            obj.Should().NotBeNull()
                .And.BeOfType<TestSubjectWithMultipleDependenciesExample>();
        }

        [Test]
        public void Create_UsesPreexistingMocks()
        {
            // Arrange.
            var moqzilla = new Moqzilla();
            var mock = moqzilla.Mock<IDisposable>();
            var obj = moqzilla.Create<TestSubjectWithSingleDependencyExample>();

            // Act.
            obj.Dispose();

            // Assert.
            mock.Verify(x => x.Dispose());
        }

        [Test]
        public void Create_UsesPostexistingMocks()
        {
            // Arrange.
            var moqzilla = new Moqzilla();
            var obj = moqzilla.Create<TestSubjectWithSingleDependencyExample>();
            var mock = moqzilla.Mock<IDisposable>();

            // Act.
            obj.Dispose();

            // Assert.
            mock.Verify(x => x.Dispose());
        }

        [Test]
        public void Mock_ReturnsSameObject()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            var obj0 = moqzilla.Mock<IDisposable>();
            var obj1 = moqzilla.Mock<IDisposable>();

            // Assert.
            obj0.Should().BeSameAs(obj1);
        }

        [Test]
        public void Mock_InjectsCorrectGenericMock()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            var dep = moqzilla.Mock<IEnumerable<string>>();
            var obj = moqzilla.Create<TestSubjectWithMultipleDependenciesExample>();
            obj.GetEnumerator();

            // Assert.
            dep.Verify(x => x.GetEnumerator(), Times.Once);
        }

        [Test]
        public void Reset_ClearsMockRepository()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            var obj0 = moqzilla.Mock<IDisposable>();
            moqzilla.Reset();
            var obj1 = moqzilla.Mock<IDisposable>();

            // Assert.
            obj0.Should().NotBeSameAs(obj1);
        }

        [Test]
        public void Reset_ClearsSingleMock()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            var disposable0 = moqzilla.Mock<IDisposable>();
            var comparable0 = moqzilla.Mock<IComparable>();
            moqzilla.Reset<IDisposable>();
            var disposable1 = moqzilla.Mock<IDisposable>();
            var comparable1 = moqzilla.Mock<IComparable>();

            // Assert.
            disposable0.Should().NotBeSameAs(disposable1);
            comparable0.Should().BeSameAs(comparable1);
        }

        [Test]
        public void Inject_SetsSingleMock()
        {
            // Arrange.
            var moqzilla = new Moqzilla();

            // Act.
            var oldObj = moqzilla.Mock<IDisposable>();
            var newObj = new Mock<IDisposable>();
            moqzilla.Inject(newObj);
            var output = moqzilla.Mock<IDisposable>();

            // Assert.
            oldObj.Should().NotBeSameAs(newObj);
            newObj.Should().BeSameAs(output);
        }

        #endregion

    }
}
