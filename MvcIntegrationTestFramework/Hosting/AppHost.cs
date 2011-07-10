// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Public">
//   Free
// </copyright>
// <summary>
//   Hosts an ASP.NET application within an ASP.NET-enabled .NET appdomain
//   and provides methods for executing test code within that appdomain
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MvcIntegrationTestFramework.Hosting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;

    using MvcIntegrationTestFramework.Browsing;
    using MvcIntegrationTestFramework.Interception;

    /// <summary>
    ///   Hosts an ASP.NET application within an ASP.NET-enabled .NET appdomain
    ///   and provides methods for executing test code within that appdomain
    /// </summary>
    public class AppHost
    {
        /// <summary>
        ///   Gets the method info to get the application instance method.
        /// </summary>
        private static readonly MethodInfo GetApplicationInstanceMethod;

        /// <summary>
        ///   The the method info to recycle application instance method.
        /// </summary>
        private static readonly MethodInfo RecycleApplicationInstanceMethod;

        /// <summary>
        ///   The gateway to the ASP.NET-enabled .NET appdomain
        /// </summary>
        private readonly AppDomainProxy appDomainProxy;

        /// <summary>
        ///   Initializes static members of the <see cref = "AppHost" /> class.
        /// </summary>
        static AppHost()
        {
            // Get references to some MethodInfos we'll need to use later to bypass nonpublic access restrictions
            var httpApplicationFactory = typeof(HttpContext).Assembly.GetType("System.Web.HttpApplicationFactory", true);
            GetApplicationInstanceMethod = httpApplicationFactory.GetMethod(
                "GetApplicationInstance", BindingFlags.Static | BindingFlags.NonPublic);
            RecycleApplicationInstanceMethod = httpApplicationFactory.GetMethod(
                "RecycleApplicationInstance", BindingFlags.Static | BindingFlags.NonPublic);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref = "AppHost" /> class.
        /// </summary>
        /// <param name = "appPhysicalDirectory">
        ///   The app physical directory.
        /// </param>
        /// <param name = "virtualDirectory">
        ///   The virtual directory.
        /// </param>
        /// <exception cref = "InvalidOperationException">
        ///   Thrown when MvcIntegrationTestFramework.dll cannot be located form the bin directory.
        /// </exception>
        private AppHost(string appPhysicalDirectory, string virtualDirectory = "/")
        {
            try
            {
                this.appDomainProxy =
                    (AppDomainProxy)
                    ApplicationHost.CreateApplicationHost(
                        typeof(AppDomainProxy), virtualDirectory, appPhysicalDirectory);
            }
            catch (FileNotFoundException ex)
            {
                if (ex.Message.Contains("MvcIntegrationTestFramework"))
                {
                    throw new InvalidOperationException(
                        string.Format(
                            CultureInfo.CurrentCulture, 
                            "Could not load MvcIntegrationTestFramework.dll within a bin directory under {0}. Is this the path to your ASP.NET MVC application, and have you set up a post-build event to copy your test assemblies and their dependencies to this folder? See the demo project for an example.", 
                            appPhysicalDirectory));
                }

                throw;
            }

            this.appDomainProxy.RunCodeInAppDomain(
                () =>
                    {
                        InitializeApplication();
                        AttachTestControllerDescriptorsForAllControllers();
                        LastRequestData.Reset();
                    });
        }

        /// <summary>
        ///   Gets the current application instance.
        /// </summary>
        /// <returns>
        ///   The application instance.
        /// </returns>
        public static HttpApplication GetApplicationInstance()
        {
            var writer = new StringWriter();
            var workerRequest = new SimpleWorkerRequest(string.Empty, string.Empty, writer);
            var httpContext = new HttpContext(workerRequest);
            return (HttpApplication)GetApplicationInstanceMethod.Invoke(null, new object[] { httpContext });
        }

        /// <summary>
        ///   Creates an instance of the AppHost so it can be used to simulate a browsing session.
        /// </summary>
        /// <param name = "projectName">
        ///   The name of the MVC project.
        /// </param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "projectName" /> is null.
        /// </exception>
        /// <returns>
        ///   The simulated application host.
        /// </returns>
        public static AppHost Simulate(string projectName)
        {
            if (projectName == null)
            {
                throw new ArgumentNullException("projectName");
            }

            var pathToMvcProject =
                new DirectoryInfo(
                    Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", projectName)));

            return new AppHost(pathToMvcProject.ToString());
        }

        /// <summary>
        ///   Starts with application host with the specified test script.
        /// </summary>
        /// <param name = "testScript">
        ///   The test script.
        /// </param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "testScript" /> is null.
        /// </exception>
        public void Start(Action<BrowsingSession> testScript)
        {
            if (testScript == null)
            {
                throw new ArgumentNullException("testScript");
            }

            var serializableDelegate = new SerializableDelegate<Action<BrowsingSession>>(testScript);
            this.appDomainProxy.RunBrowsingSessionInAppDomain(serializableDelegate);
        }

        /// <summary>
        ///   The attach test controller descriptors for all controllers.
        /// </summary>
        private static void AttachTestControllerDescriptorsForAllControllers()
        {
            var allControllerTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     from type in assembly.GetTypes()
                                     where typeof(IController).IsAssignableFrom(type)
                                     select type;

            foreach (var controllerType in allControllerTypes)
            {
                InterceptionFilter.AssociateWithControllerType(controllerType);
            }
        }

        /// <summary>
        ///   The initialize application.
        /// </summary>
        private static void InitializeApplication()
        {
            var appInstance = GetApplicationInstance();
            appInstance.PostRequestHandlerExecute += delegate
                {
                    // Collect references to context objects that would otherwise be lost
                    // when the request is completed
                    if (LastRequestData.HttpSessionState == null)
                    {
                        LastRequestData.HttpSessionState = HttpContext.Current.Session;
                    }

                    if (LastRequestData.Response == null)
                    {
                        LastRequestData.Response = HttpContext.Current.Response;
                    }
                };

            RefreshEventsList(appInstance);
            RecycleApplicationInstance(appInstance);
        }

        /// <summary>
        ///   The recycle application instance.
        /// </summary>
        /// <param name = "appInstance">
        ///   The app instance.
        /// </param>
        private static void RecycleApplicationInstance(HttpApplication appInstance)
        {
            RecycleApplicationInstanceMethod.Invoke(null, new object[] { appInstance });
        }

        /// <summary>
        ///   The refresh events list.
        /// </summary>
        /// <param name = "applicationInstance">
        ///   The application instance.
        /// </param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when the <paramref name = "applicationInstance" /> is null.
        /// </exception>
        private static void RefreshEventsList(HttpApplication applicationInstance)
        {
            if (applicationInstance == null)
            {
                throw new ArgumentNullException("applicationInstance");
            }

            var stepManager =
                typeof(HttpApplication).GetField("_stepManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(applicationInstance);

            var resumeStepsWaitCallback =
                typeof(HttpApplication).GetField(
                    "_resumeStepsWaitCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(
                        applicationInstance);

            var buildStepsMethod = stepManager.GetType().GetMethod(
                "BuildSteps", BindingFlags.NonPublic | BindingFlags.Instance);

            buildStepsMethod.Invoke(stepManager, new[] { resumeStepsWaitCallback });
        }
    }
}