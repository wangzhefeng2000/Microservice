﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xigadee;

namespace Test.Xigadee
{
    [TestClass]
    public class DispatcherTests
    {
        CommandInitiator mCommandInit;
        ChannelPipelineIncoming cpipeIn = null;
        ChannelPipelineOutgoing cpipeOut = null;
        DebugMemoryDataCollector collector = null;
        Microservice service = null;
        MicroservicePipeline mPipeline = null;
        TestChannelListener mListener = null;
        TestChannelSender mSender = null;

        [TestInitialize]
        public void TearUp()
        {
            try
            {
                mPipeline = PipelineConfigure(Microservice.Create((s) => service = s, serviceName: GetType().Name));
                mPipeline.Start();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [TestCleanup]
        public void TearDown()
        {
            mPipeline.Stop();
            mPipeline = null;
        }

        [TestMethod]
        public void DispatcherTest1()
        {

        }

        [TestMethod]
        public void DispatcherTest2()
        {

        }


        protected virtual MicroservicePipeline PipelineConfigure(MicroservicePipeline pipeline)
        {
            try
            {
                pipeline
                    .AddDataCollector<DebugMemoryDataCollector>((c) => collector = c)
                    .AddPayloadSerializerDefaultJson()
                    .AddChannelIncoming("internalIn", internalOnly: false)
                        .AssignPriorityPartition(0, 1)
                        .AttachListener<TestChannelListener>(action: (s) => mListener = s)
                        .Revert((c) => cpipeIn = c)
                    .AddChannelOutgoing("internalOut", internalOnly: false)
                        .AssignPriorityPartition(0, 1)
                        .AttachSender<TestChannelSender>(action: (s) => mSender = s)
                        .Revert((c) => cpipeOut = c)
                    .AddCommand(new CommandInitiator() { ResponseChannelId = cpipeOut.Channel.Id }, (c) => mCommandInit = c);

                return pipeline;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
