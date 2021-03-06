﻿#region using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#endregion
namespace Xigadee
{
    /// <summary>
    /// This interface is used to provide generic support for event sources for components
    /// </summary>
    public interface IEventSource
    {
        Task Write<K,E>(string originatorId, EventSourceEntry<K,E> entry, DateTime? utcTimeStamp = null, bool sync = false);

        string Name { get; }
    }
}
