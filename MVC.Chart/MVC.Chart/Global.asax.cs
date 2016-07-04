using System;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MVC.Chart
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            PropagateLoggingSettings();

            StartDALRoutines();
        }

        protected void Application_End()
        {
            StopDALRoutines();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();
            Log.Error(exception, "[MvcApplication. Application_Error] Unhandled exception");
            //Response.Clear();
            //Server.ClearError();  - let 'em go to see exception ASP page in browser. Should be handled in production.

        }

        /// <summary>
        /// Propagate well-known sttings from Web.config into used DLL's settings
        /// </summary>
        private void PropagateLoggingSettings()
        {
            var appPath = HttpRuntime.AppDomainAppPath;
            var appDataPath = Path.Combine(appPath, "App_Data");

            var loggingPath = WebConfigurationManager.AppSettings["ApplicationBaseLoggingPath"] ?? "Logs";
            if (!Path.IsPathRooted(loggingPath))
                loggingPath = Path.Combine(appDataPath, loggingPath);

            //DAL Settings
            var logName = WebConfigurationManager.AppSettings["DALLoggingPath"] ?? "DAL.log";
            var finalLoggingPath = Path.Combine(loggingPath, logName);

            DAL.Properties.SettingsPropagator.ApplySettings(finalLoggingPath);

            //MVC.Chart Settings
            logName = WebConfigurationManager.AppSettings["MainLoggingPath"] ?? "MVC.Chart.log";
            finalLoggingPath = Path.Combine(loggingPath, logName);

            Settings.ApplySettings(finalLoggingPath);
        }

        private IDisposable _dbCommiter;

        private void StartDALRoutines()
        {
            var appPath = HttpRuntime.AppDomainAppPath;
            var appDataPath = Path.Combine(appPath, "App_Data");

            var userDbPath = WebConfigurationManager.AppSettings["UserDatabase"] ?? "users.xml";
            if (!Path.IsPathRooted(userDbPath))
                userDbPath = Path.Combine(appDataPath, userDbPath);

            DAL.UserRepository.InitializeInstance(userDbPath);

            _dbCommiter = DAL.UserRepositoryCommiter.Create( TimeSpan.FromMinutes( 2 ) ); // commit users DB every 2 minutes
        }

        private void StopDALRoutines()
        {
            _dbCommiter.Dispose();

            DAL.UserRepository.Instance.Shutdown();
        }
    }
}
