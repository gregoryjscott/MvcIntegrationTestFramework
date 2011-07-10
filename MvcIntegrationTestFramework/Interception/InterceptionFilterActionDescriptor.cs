// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterceptionFilterActionDescriptor.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   A special ASP.NET MVC action descriptor used to attach InterceptionFilter to all loaded controllers
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Interception
{
    using System.Reflection;
    using System.Web.Mvc;

    /// <summary>
    /// A special ASP.NET MVC action descriptor used to attach InterceptionFilter to all loaded controllers
    /// </summary>
    internal class InterceptionFilterActionDescriptor : ReflectedActionDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterceptionFilterActionDescriptor"/> class.
        /// </summary>
        /// <param name="methodInfo">
        /// The method info.
        /// </param>
        /// <param name="actionName">
        /// The action name.
        /// </param>
        /// <param name="controllerDescriptor">
        /// The controller descriptor.
        /// </param>
        public InterceptionFilterActionDescriptor(
            MethodInfo methodInfo, string actionName, ControllerDescriptor controllerDescriptor)
            : base(methodInfo, actionName, controllerDescriptor)
        {
        }

        /// <summary>
        /// Retrieves information about action filters.
        /// </summary>
        /// <returns>
        /// The filter information.
        /// </returns>
        public override FilterInfo GetFilters()
        {
            var usualFilters = base.GetFilters();
            var interceptionFilter = new InterceptionFilter();
            usualFilters.ActionFilters.Insert(0, interceptionFilter);
            usualFilters.ResultFilters.Insert(0, interceptionFilter);
            return usualFilters;
        }
    }
}