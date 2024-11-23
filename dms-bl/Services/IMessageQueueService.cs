using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dms_bl.Services
{
    public interface IMessageQueueService
    {
        void SendToQueue(string message);
    }
}
