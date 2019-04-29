using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
                                                         
namespace EmailNotificationService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //var service = new EmailNotificationService();
            //service.StartService();
            //while (true)
            //{ Thread.Sleep(1000); }
            //return;

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new EmailNotificationService() 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
