using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Data.Common;
using Microsoft.WindowsAzure.Diagnostics;


namespace MultiWorker
{
    public class WorkerRole : RoleEntryPoint
    {

        private Object m_DbLock = new Object();

        private readonly List<IWorkerRole> m_Workers = new List<IWorkerRole>()
        {
            new ProcessingWorker1(),
            new ProcessingWorker2(),
            new ProcessingWorker3()
        };

        public override void OnStop()
        {
            Trace.TraceInformation("Stop Called", "Information");
            Dictionary<IWorkerRole, Task> workerTasks = new Dictionary<IWorkerRole, Task>();
            foreach (IWorkerRole worker in m_Workers)
            {
                Trace.WriteLine("Stopping Worker Role " + worker.GetType().Name);
                workerTasks.Add(worker, worker.OnRun());
            }

            Task.WaitAll(workerTasks.Values.ToArray());

            base.OnStop();

        }
        public override void Run()
        {
            Trace.TraceInformation("MutliWorker entry point called", "Information");
            Dictionary<IWorkerRole, Task> workerTasks = new Dictionary<IWorkerRole, Task>();
            foreach (IWorkerRole worker in m_Workers)
            {
                Trace.WriteLine("Running Processing Worker " + worker.GetType().Name);
                workerTasks.Add(worker, worker.OnRun());
            }

            Task.WaitAll(workerTasks.Values.ToArray());

            foreach (IWorkerRole wr in workerTasks.Keys)
            {
                if (workerTasks[wr].IsFaulted && workerTasks[wr].Exception != null)
                {
                    throw workerTasks[wr].Exception;
                }
            }
        }


        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            var config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            config.Logs.ScheduledTransferPeriod = System.TimeSpan.FromMinutes(1.0);
            config.Logs.ScheduledTransferLogLevelFilter = LogLevel.Warning | LogLevel.Error;

            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);

            Dictionary<IWorkerRole, Task<bool>> workerTasks = new Dictionary<IWorkerRole, Task<bool>>();
            foreach (IWorkerRole worker in m_Workers)
            {
                Trace.WriteLine("Initializing Worker Role " + worker.GetType().Name);
                workerTasks.Add(worker, worker.OnStart());
            }

            foreach (IWorkerRole wr in workerTasks.Keys)
            {
                if (workerTasks[wr].Result)
                {
                    Trace.TraceError(wr.GetType().Name + " Failed to Start");
                }

                if (workerTasks[wr].IsFaulted &&
                    workerTasks[wr].Exception != null)
                {
                    throw workerTasks[wr].Exception;
                }

            }

            Trace.WriteLine("MultiWorker OnStart() Completed Worker Role ");
            return base.OnStart();
        }

    }
}
