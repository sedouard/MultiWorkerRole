using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Data.Common;
using System.Threading;
using Microsoft.WindowsAzure.Diagnostics;
namespace MultiWorker
{
    /// <summary>
    /// Crawls through database removing event suggestions. (Those events that have already passed).
    /// 
    /// This keeps suggestions more relevant
    /// </summary>
    class ProcessingWorker3 : IWorkerRole
    {

        private bool m_Stopped = false;

        public Task<bool> OnStart()
        {
            return Task.Factory.StartNew<bool>(() =>
            {
                return true;
            });
        }

        public Task OnRun()
        {
            return Task.Factory.StartNew(() =>
            {
                int count = 0;
                while (!m_Stopped)
                {
                    try
                    {
                        Trace.WriteLine(this.GetType().ToString() + " - Processing " + count);
                        count++;
                    }
                    catch(Exception e)
                    {
                        Trace.TraceError("Unexpected exception in " + this.GetType().ToString() + " exception info \n " + e);
                    }
                }

            });
        }

        public Task OnEnd()
        {
            //Doesn't really stop the task right away but not a big deal for now
            return Task.Factory.StartNew(() =>
            {
                m_Stopped = true;
            });

        }
    }
}
