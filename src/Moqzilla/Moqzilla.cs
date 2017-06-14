using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using System.Reflection;

namespace Moqzilla
{
    /// <summary>
    /// A container for dynamically creating mocks.
    /// </summary>
    public class Moqzilla
    {
        /// <summary>
        /// Mock repository.
        /// </summary>
        private readonly IDictionary<Type, object> _repository;

        /// <summary>
        /// Cached empty Type array.
        /// </summary>
        private static readonly Type[] EmptyTypeArray = new Type[] { };

        /// <summary>
        /// Cached empty object array.
        /// </summary>
        private static readonly object[] EmptyObjectArray = new object[] { };

        /// <summary>
        /// Create a Moqzilla container.
        /// </summary>
        public Moqzilla()
        {
            _repository = new Dictionary<Type, object>();
        }

        /// <summary>
        /// Returns TRUE if the specified type is an interface type.
        /// </summary>
        private static bool TypeIsInterface(Type type)
        {
            #if NETSTANDARD1_3
                return type.GetTypeInfo().IsInterface;
            #else
                return type.IsInterface;
            #endif
        }

        /// <summary>
        /// Creates an object, mocking up all dependencies in the constructor.
        /// </summary>
        public TSubject Create<TSubject>()
            where TSubject : class
        {
            // Cache constructor parameters.
            var type = typeof(TSubject);
            var constructors = type.GetConstructors();
            var parameters = constructors
                .ToDictionary(c => c, c => c.GetParameters());

            // Determine which constructors we can use.
            var validConstructors = constructors
                .Where(c => parameters[c]
                    .All(p => TypeIsInterface(p.ParameterType)))
                .ToArray();

            // If there are no constructors, fail.
            if (!validConstructors.Any())
                throw new MoqzillaException(MoqzillaExceptionType.NoValidConstructors);

            // Choose the constructor with the most dependencies.
            var mostSpecificConstructor = constructors
                .OrderBy(c => parameters[c].Length)
                .First();

            // Pull mocked objects for the constructor from the repository.
            var constructorArguments = parameters[mostSpecificConstructor]
                .Select(p => Get(p.ParameterType).Object)
                .ToArray();

            // Instantiate the object.
            return (TSubject)mostSpecificConstructor.Invoke(constructorArguments);
        }

        /// <summary>
        /// Get a mock from the repository for a type determined at runtime.
        /// </summary>
        protected Mock Get(Type type)
        {
            // Return a cached mock, if we have one.
            if (_repository.ContainsKey(type))
                return (Mock)_repository[type];

            // Create a mock - reflection is needed for invocation due to how Moq works.
            var mock = typeof(Mock<>)
                .MakeGenericType(type)
                .GetConstructor(EmptyTypeArray)
                ?.Invoke(EmptyObjectArray);

            _repository[type] = mock;
            return (Mock)mock;
        }

        /// <summary>
        /// Get a mock from the container.
        /// </summary>
        public Mock<TSubject> Mock<TSubject>()
            where TSubject : class
        {
            var type = typeof(TSubject);

            if (_repository.ContainsKey(type))
                return (Mock<TSubject>) _repository[type];

            var mock = new Mock<TSubject>();
            _repository[type] = mock;
            return mock;
        }

        /// <summary>
        /// Removes all mocks from the container. This does not affect objects already created
        /// </summary>
        public void Reset()
        {
            _repository.Clear();
        }
    }
}
