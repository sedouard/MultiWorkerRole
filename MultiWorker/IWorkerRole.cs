using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWorker
{
    public interface IWorkerRole
    {
        Task<bool> OnStart();
        Task OnRun();
        Task OnEnd();
    }
}
