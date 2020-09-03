using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Infra.EventBus
{
    public interface IEventBus
    {
        void Publish<T>(T ententy);
        string Consumer();
    }
}
