using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;

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
            private readonly IDisposable _disposable;
            private readonly IComparable _comparable;
            private readonly IEnumerable<string> _enumerable;

            public TestSubjectWithMultipleDependenciesExample(
                IDisposable disposable, 
                IComparable comparable, 
                IEnumerable<string> enumerable)
            {
                _disposable = disposable;
                _comparable = comparable;
                _enumerable = enumerable;
            }

            public string[] Enumerate()
            {
                return _enumerable.ToArray();
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

        #endregion

    }
}
