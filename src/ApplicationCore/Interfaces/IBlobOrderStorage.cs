using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces
{
    public interface IBlobOrderStorage
    {
        Task<bool> CallAppFunc(Entities.OrderAggregate.Order order);
        Task<bool> SendToQueue(Entities.OrderAggregate.Order order);
    }
}
