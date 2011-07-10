﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InterceptionFilter.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   An ASP.NET MVC filter attached automatically to all controllers invoked within the test application
//   This is used to capture action results and other output generated by each request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Interception
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Mvc;

    /// <summary>
    /// An ASP.NET MVC filter attached automatically to all controllers invoked within the test application
    ///   This is used to capture action results and other output generated by each request
    /// </summary>
    internal class InterceptionFilter : ActionFilterAttribute
    {
        /// <summary>
        /// The fetch or create item method.
        /// </summary>
        private static readonly MethodInfo fetchOrCreateItemMethod;

        /// <summary>
        /// The static descriptor cache instance.
        /// </summary>
        private static readonly object staticDescriptorCacheInstance;

        /// <summary>
        /// Initializes static members of the <see cref="InterceptionFilter"/> class.
        /// </summary>
        static InterceptionFilter()
        {
            staticDescriptorCacheInstance =
                typeof(ControllerActionInvoker).GetField(
                    "_staticDescriptorCache", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            fetchOrCreateItemMethod = staticDescriptorCacheInstance.GetType().GetMethod(
                "FetchOrCreateItem", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Gets LastHttpContext.
        /// </summary>
        public static HttpContext LastHttpContext { get; private set; }

        /// <summary>
        /// The associate with controller type.
        /// </summary>
        /// <param name="controllerType">
        /// The controller type.
        /// </param>
        public static void AssociateWithControllerType(Type controllerType)
        {
            Func<ControllerDescriptor> descriptorCreator = () => new InterceptionFilterControllerDescriptor(controllerType);

            // The parameters to the invocation are "key", "value".
            fetchOrCreateItemMethod.Invoke(
                staticDescriptorCacheInstance,
                new object[] { controllerType, descriptorCreator });
        }

        /// <summary>
        /// The on action executed.
        /// </summary>
        /// <param name="filterContext">
        /// The filter context.
        /// </param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (LastHttpContext == null)
            {
                LastHttpContext = HttpContext.Current;
            }

            // Clone to get a more stable snapshot
            if ((filterContext != null) && (LastRequestData.ActionExecutedContext == null))
            {
                LastRequestData.ActionExecutedContext = new ActionExecutedContext
                    {
                        ActionDescriptor = filterContext.ActionDescriptor, 
                        Canceled = filterContext.Canceled, 
                        Controller = filterContext.Controller, 
                        Exception = filterContext.Exception, 
                        ExceptionHandled = filterContext.ExceptionHandled, 
                        HttpContext = filterContext.HttpContext, 
                        RequestContext = filterContext.RequestContext, 
                        Result = filterContext.Result, 
                        RouteData = filterContext.RouteData
                    };
            }
        }

        /// <summary>
        /// The on result executed.
        /// </summary>
        /// <param name="filterContext">
        /// The filter context.
        /// </param>
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            // Clone to get a more stable snapshot
            if ((filterContext != null) && (LastRequestData.ResultExecutedContext == null))
            {
                LastRequestData.ResultExecutedContext = new ResultExecutedContext
                    {
                        Canceled = filterContext.Canceled, 
                        Exception = filterContext.Exception, 
                        Controller = filterContext.Controller, 
                        ExceptionHandled = filterContext.ExceptionHandled, 
                        HttpContext = filterContext.HttpContext, 
                        RequestContext = filterContext.RequestContext, 
                        Result = filterContext.Result, 
                        RouteData = filterContext.RouteData
                    };
            }
        }
    }
}