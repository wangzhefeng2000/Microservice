﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    public static partial class TcpCommunicationPipelineExtensions
    {
        public static C AttachTcpTlsListener<C>(this C cpipe
            , string connectionName = null
            , string serviceBusConnection = null
            , string mappingChannelId = null
            , IEnumerable<ListenerPartitionConfig> priorityPartitions = null
            , IEnumerable<ResourceProfile> resourceProfiles = null
            , Action<TcpTlsChannelListener> onCreate = null)
            where C: IPipelineChannelIncoming<IPipeline>
        {
            throw new NotImplementedException();

            //var component = new AzureSBQueueListener();

            //component.ConfigureAzureMessaging(
            //      cpipe.Channel.Id
            //    , priorityPartitions ?? cpipe.Channel.Partitions.Cast<ListenerPartitionConfig>()
            //    , resourceProfiles
            //    , connectionName ?? cpipe.Channel.Id
            //    , serviceBusConnection ?? cpipe.Pipeline.Configuration.ServiceBusConnection()
            //    );

            //component.IsDeadLetterListener = isDeadLetterListener;

            //onCreate?.Invoke(component);

            //cpipe.AttachListener(component, false);

            return cpipe;
        }

    }
}
