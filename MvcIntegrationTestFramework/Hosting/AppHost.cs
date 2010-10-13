using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using MvcIntegrationTestFramework.Browsing;
using MvcIntegrationTestFramework.Interception;

namespace MvcIntegrationTestFramework.Hosting
{
    /// <summary>
    /// Hosts an ASP.NET application within an ASP.NET-enabled .NET appdomain
    /// and provides methods for executing test code within that appdomain
    /// </summary>
    public class AppHost
    {
        private readonly AppDomainProxy _appDomainProxy; // The gateway to the ASP.NET-enabled .NET appdomain

        private AppHost(string appPhysicalDirectory, string virtualDirectory = "/")
        {
            try {
                _appDomainProxy = (AppDomainProxy) ApplicationHost.CreateApplicationHost(typeof (AppDomainProxy), virtualDirectory, appPhysicalDirectory);
            } catch(FileNotFoundException ex) {
                if((ex.Message != null) && ex.Message.Contains("MvcIntegrationTestFramework"))
                    throw new InvalidOperationException("Could not load MvcIntegrationTestFramework.dll within a bin directory under " + appPhysicalDirectory + ". Is this the path to your ASP.NET MVC application, and have you set up a post-build event to copy your test assemblies and their dependencies to this folder? See the demo project for an example.");
                throw;
            }

            _appDomainProxy.RunCodeInAppDomain(() => {
                InitializeApplication();
                AttachTestControllerDescriptorsForAllControllers();
                LastRequestData.Reset();
            });
        }

        public void BrowsingSession(Action<BrowsingSession> testScript)
        {
            var serializableDelegate = new SerializableDelegate<Action<BrowsingSession>>(testScript);
            _appDomainProxy.RunBrowsingSessionInAppDomain(serializableDelegate);
        }

        #region Initializing app & interceptors
        private static void InitializeApplication()
        {
            var appInstance = GetApplicationInstance();
            appInstance.PostRequestHandlerExecute += delegate {
                // Collect references to context objects that would otherwise be lost
                // when the request is completed
                if(LastRequestData.HttpSessionState == null)
                    LastRequestData.HttpSessionState = HttpContext.Current.Session;
                if (LastRequestData.Response == null)
                    LastRequestData.Response = HttpContext.Current.Response;
            };
            RefreshEventsList(appInstance);

            RecycleApplicationInstance(appInstance);
        }

        private static void AttachTestControllerDescriptorsForAllControllers()
        {
            var allControllerTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     from type in assembly.GetTypes()
                                     where typeof (IController).IsAssignableFrom(type)
                                     select type;
            foreach (var controllerType in allControllerTypes)
                InterceptionFilter.AssociateWithControllerType(controllerType);
        }
        #endregion

        #region Reflection hacks
        private static readonly MethodInfo getApplicationInstanceMethod;
        private static readonly MethodInfo recycleApplicationInstanceMethod;

        static AppHost()
        {
            // Get references to some MethodInfos we'll need to use later to bypass nonpublic access restrictions
            var httpApplicationFactory = typeof(HttpContext).Assembly.GetType("System.Web.HttpApplicationFactory", true);
            getApplicationInstanceMethod = httpApplicationFactory.GetMethod("GetApplicationInstance", BindingFlags.Static | BindingFlags.NonPublic);
            recycleApplicationInstanceMethod = httpApplicationFactory.GetMethod("RecycleApplicationInstance", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static HttpApplication GetApplicationInstance()
        {
            var writer = new StringWriter();
            var workerRequest = new SimpleWorkerRequest("", "", writer);
            var httpContext = new HttpContext(workerRequest);
            return (HttpApplication)getApplicationInstanceMethod.Invoke(null, new object[] { httpContext });
        }

        private static void RecycleApplicationInstance(HttpApplication appInstance)
        {
            recycleApplicationInstanceMethod.Invoke(null, new object[] { appInstance });
        }

        private static void RefreshEventsList(HttpApplication appInstance)
        {
            object stepManager = typeof (HttpApplication).GetField("_stepManager", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(appInstance);
            object resumeStepsWaitCallback = typeof(HttpApplication).GetField("_resumeStepsWaitCallback", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(appInstance);
            var buildStepsMethod = stepManager.GetType().GetMethod("BuildSteps", BindingFlags.NonPublic | BindingFlags.Instance);
            buildStepsMethod.Invoke(stepManager, new[] { resumeStepsWaitCallback });
        }

        #endregion

        /// <summary>
        /// Creates an instance of the AppHost so it can be used to simulate a browsing session.
        /// </summary>
        /// <param name="pathToYourWebProject">
        /// The path to your web project. This is optional if you don't
        /// specify we try to guess that it is in the first directory like
        /// ../../../*/web.config
        /// </param>
        /// <returns></returns>
        public static AppHost Simulate(string pathToYourWebProject = null)
        {
            if (pathToYourWebProject == null)
            {
                var guessDirectory = new DirectoryInfo(
                                        Path.GetFullPath(
                                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..")));

                var projectDirs = guessDirectory.GetDirectories();
                foreach (var pd in projectDirs)
                {
                    if (pd.GetFiles("web.config").Length == 1)
                    {
                        pathToYourWebProject = pd.FullName;
                        continue;
                    }
                }
            }

            var ourDll = Path.Combine(pathToYourWebProject, "bin", "MvcIntegrationTestFramework.dll");
            if (!File.Exists(ourDll))
            {
                File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MvcIntegrationTestFramework.dll"), ourDll);
            }

            //return new AppHost(pathToYourWebProject, "/__test");
            return new AppHost(pathToYourWebProject);
        }
    }
}