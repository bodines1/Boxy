using System;
using Unity;
using Unity.Resolution;

namespace Boxy.IoC
{
    /// <summary>
    /// Factory for creating objects and viewmodels in Airflow.ViewModels.
    /// </summary>
    public class UnityDependencyResolver : IDependencyResolver
    {
        private readonly IUnityContainer _container;

        /// <summary>
        /// Initializes a new UnityDependencyResolver.
        /// </summary>
        /// <param name="container">unity container.</param>
        public UnityDependencyResolver(IUnityContainer container)
        {
            _container = container;
            _container.AddExtension(new Diagnostic());
        }

        /// <inheritdoc />
        public void RegisterInstance<TInterface>(TInterface instance)
        {
            _container.RegisterInstance(instance);
        }

        /// <inheritdoc />
        public void RegisterType<TFrom, TTo>()
            where TTo : TFrom
        {
            _container.RegisterType<TFrom, TTo>();
        }

        /// <inheritdoc />
        public object Resolve(Type t)
        {
            return _container.Resolve(t);
        }

        /// <inheritdoc />
        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        /// <inheritdoc />
        public T Resolve<T>(params (string, object)[] parameters)
        {
            var resolveOverride = new ResolverOverride[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Item1 is string paramString && parameters[i].Item2 is object paramObject)
                {
                    resolveOverride[i] = new ParameterOverride(paramString, paramObject);
                }
            }

            return _container.Resolve<T>(resolveOverride);
        }
    }
}
