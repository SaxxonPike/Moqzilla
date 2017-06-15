using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Moq;
using System.Reflection;

namespace Moqzilla
{
    /// <summary>
    /// A container for dynamically creating mocks.
    /// </summary>
    public class Mocker
    {
        /// <summary>
        /// Mock repository. Values are Mock of T
        /// </summary>
        private readonly IDictionary<Type, object> _mockRepository;

        /// <summary>
        /// Activator repository. Values are Action of T
        /// </summary>
        private readonly IDictionary<Type, object> _activatorRepository;

        /// <summary>
        /// Cached empty Type array.
        /// </summary>
        private static readonly Type[] EmptyTypeArray = { };

        /// <summary>
        /// Cached empty object array.
        /// </summary>
        private static readonly object[] EmptyObjectArray = { };

        /// <summary>
        /// Create a Moqzilla container.
        /// </summary>
        public Mocker()
        {
            _mockRepository = new Dictionary<Type, object>();
            _activatorRepository = new Dictionary<Type, object>();
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
        /// <exception cref="MockerException">Thrown when no mockable constructor could be found.</exception>
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
                throw new MockerException(MockerExceptionType.NoValidConstructors);

            // Choose the constructor with the most dependencies.
            var mostSpecificConstructor = constructors
                .OrderBy(c => parameters[c].Length)
                .First();

            // Pull mocked objects for the constructor from the repository.
            var constructorArguments = parameters[mostSpecificConstructor]
                .Select(p => Get(p.ParameterType).Object)
                .ToArray();

            // Run activations.
            foreach (var parameter in parameters[mostSpecificConstructor])
            {
                if (!_activatorRepository.ContainsKey(parameter.ParameterType))
                    continue;

                var activator = _activatorRepository[parameter.ParameterType];
                var activatorType = activator.GetType();
                activatorType.GetMethod("Invoke").Invoke(activator, new object[] { Get(parameter.ParameterType) });
            }

            // Instantiate the object.
            return (TSubject)mostSpecificConstructor.Invoke(constructorArguments);
        }

        /// <summary>
        /// Get a mock from the repository for a type determined at runtime.
        /// </summary>
        protected Mock Get(Type type)
        {
            // Return a cached mock, if we have one.
            if (_mockRepository.ContainsKey(type))
                return (Mock)_mockRepository[type];

            // Create a mock - reflection is needed for invocation due to how Moq works.
            var mock = typeof(Mock<>)
                .MakeGenericType(type)
                .GetConstructor(EmptyTypeArray)
                ?.Invoke(EmptyObjectArray);

            _mockRepository[type] = mock;
            return (Mock)mock;
        }

        /// <summary>
        /// Get a mock from the container.
        /// </summary>
        public Mock<TSubject> Mock<TSubject>()
            where TSubject : class
        {
            var type = typeof(TSubject);

            if (_mockRepository.ContainsKey(type))
                return (Mock<TSubject>) _mockRepository[type];

            var mock = new Mock<TSubject>();
            _mockRepository[type] = mock;
            return mock;
        }

        /// <summary>
        /// Set up a mock within the container. This allows you to pass in
        /// a block solely for the purpose of configuring a mock.
        /// </summary>
        public Mock<TSubject> Mock<TSubject>(Action<Mock<TSubject>> setupMethod)
            where TSubject : class
        {
            var mock = Mock<TSubject>();
            setupMethod?.Invoke(mock);
            return mock;
        }

        /// <summary>
        /// Removes all mocks from the container. Objects created with
        /// <see cref="Create{TSubject}"/> prior to this call are not affected.
        /// This does not clear activations.
        /// </summary>
        public void Reset()
        {
            _mockRepository.Clear();
        }

        /// <summary>
        /// Removes a single mock from the container. Objects created with
        /// <see cref="Create{TSubject}"/> prior to this call are not affected.
        /// This does not clear activations.
        /// </summary>
        public void Reset<TSubject>()
        {
            _mockRepository.Remove(typeof(TSubject));
        }

        /// <summary>
        /// Injects a mock into the container. Objects created with
        /// <see cref="Create{TSubject}"/> prior to this call are not affected.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the specified mock is null.</exception>
        public void Inject<TSubject>(Mock<TSubject> mock)
            where TSubject : class
        {
            _mockRepository[typeof(TSubject)] = mock
                ?? throw new ArgumentNullException(nameof(mock));
        }

        /// <summary>
        /// Registers an activation. When <see cref="Create{TSubject}"/> is invoked,
        /// activations are invoked before object creation. This is used to set up consistent parts
        /// of a mock. 
        /// </summary>
        public void Activate<TSubject>(Action<Mock<TSubject>> activator)
            where TSubject : class
        {
            var type = typeof(TSubject);

            if (_activatorRepository.ContainsKey(type))
            {
                // Chain existing activations.
                var currentActivation = (Action<Mock<TSubject>>)_activatorRepository[type];
                _activatorRepository[type] = (Action<Mock<TSubject>>)(mock =>
                {
                    currentActivation(mock);
                    activator(mock);
                });
            }
            else
            {
                _activatorRepository[type] = activator;
            }
        }
    }
}
