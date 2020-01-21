using System;

namespace Boxy.IoC
{
    /// <summary>
    /// Factory for creating objects and viewmodels in Airflow.ViewModels.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Registers the given instance of a type with the resolver.
        /// </summary>
        /// <typeparam name="TInterface">Type of the object you are registering.</typeparam>
        /// <param name="instance">The instance you want to receive when resolving the type.</param>
        void RegisterInstance<TInterface>(TInterface instance);

        /// <summary>
        /// Registers a specific sub-type as the resolvable type of object if a resolve is called on the super-type.
        /// </summary>
        /// <typeparam name="TFrom">Super-type.</typeparam>
        /// <typeparam name="TTo">Sub-type.</typeparam>
        void RegisterType<TFrom, TTo>()
            where TTo : TFrom;

        /// <summary>
        /// Generic object creator that injects dependencies into it.
        /// </summary>
        /// <param name="t">Type of object.</param>
        /// <returns>The object of type t.</returns>
        object Resolve(Type t);

        /// <summary>
        /// Generic object creator that injects dependencies into it.
        /// </summary>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>The object of type T.</returns>
        T Resolve<T>();

        /// <summary>
        /// Generic object creator that injects dependencies into it.
        /// </summary>
        /// <param name="parameters">Collection of tuples to provide a way overriding constructor parameters.</param>
        /// <typeparam name="T">Type of object.</typeparam>
        /// <returns>The object of type T.</returns>
        T Resolve<T>(params (string, object)[] parameters);
    }
}
