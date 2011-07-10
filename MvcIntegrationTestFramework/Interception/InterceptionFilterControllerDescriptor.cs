// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterceptionFilterControllerDescriptor.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   A special ASP.NET MVC controller descriptor used to attach InterceptionFilter to all loaded controllers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Interception
{
    using System;
    using System.Web.Mvc;

    /// <summary>
    /// A special ASP.NET MVC controller descriptor used to attach InterceptionFilter to all loaded controllers
    /// </summary>
    internal class InterceptionFilterControllerDescriptor : ReflectedControllerDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionFilterControllerDescriptor"/> class.
        /// </summary>
        /// <param name="controllerType">
        /// The controller type.
        /// </param>
        public InterceptionFilterControllerDescriptor(Type controllerType)
            : base(controllerType)
        {
        }

        /// <summary>
        /// Finds the specified action for the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionName">The name of the action.</param>
        /// <returns>
        /// The information about the action.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="controllerContext"/> parameter is null.</exception>
        /// <exception cref="T:System.ArgumentException">The <paramref name="actionName"/> parameter is null or empty.</exception>
        public override ActionDescriptor FindAction(ControllerContext controllerContext, string actionName)
        {
            var normalActionDescriptor = (ReflectedActionDescriptor)base.FindAction(controllerContext, actionName);
            return new InterceptionFilterActionDescriptor(normalActionDescriptor.MethodInfo, actionName, this);
        }
    }
}