﻿#region Copyright
// Copyright Hitachi Consulting
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xigadee;

namespace Test.Xigadee.Azure
{
    [Contract(AzureQueue.ChannelIn, "Simple", "Command")]
    public interface ISimpleCommand: IMessageContract{}

    [TestClass]
    public class AzureQueue
    {
        public const string ChannelIn = "remote";
        public const string SbConn = "Endpoint=sb://xigadeedev-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=WXZaLdK1w03CeZv7cvDZxENp6Nxy4OlEmWcKF1x/y+E=";

        [TestMethod]
        public void TestMethod1()
        {
            CommandInitiator init;

            var sender = new MicroservicePipeline("sender")
                .ConfigurationOverrideSet(AzureExtensionMethods.KeyServiceBusConnection, SbConn)
                .AddChannelOutgoing("remote")
                    .AttachAzureServiceBusQueueSender()
                    .AttachCommandInitiator(out init)
                .Revert()
                    .AddChannelIncoming("response")
                    .AttachAzureServiceBusTopicListener()
                ;

            var listener = new MicroservicePipeline("listener")
                .ConfigurationOverrideSet(AzureExtensionMethods.KeyServiceBusConnection, SbConn)
                .AddChannelIncoming("remote")
                    .AttachAzureServiceBusQueueListener()
                .Revert()
                .AddChannelOutgoing("response")
                    .AttachAzureServiceBusTopicSender()
                ;

            listener.Start();

            sender.Start();

            var rs = init.Process<ISimpleCommand,string, string>("hello").Result;

            Assert.IsTrue(rs.Response == "mom");
        }
    }
}