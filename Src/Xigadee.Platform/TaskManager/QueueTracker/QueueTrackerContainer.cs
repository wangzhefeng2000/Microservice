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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;


namespace Xigadee
{

    [DebuggerDisplay("QueueTracker - Levels: {mLevels} Capacity: {DebugCapacity}")]
    public class QueueTrackerContainer<Q>: ServiceBase<QueueTrackerStatistics>
        where Q : IQueueTracker, new()
    {
        #region Declarations
        private readonly Dictionary<int, Q> mTasksQueue;

        private readonly int mLevels;

        private int[] mActive;
        #endregion
        #region Constructor
        /// <summary>
        /// This is the default constructor.
        /// </summary>
        /// <param name="levels">The number of priority levels supported by the collection. The default is 3.</param>
        public QueueTrackerContainer(int levels = 3)
        {
            mLevels = levels;
            mActive = new int[levels];

            mTasksQueue = new Dictionary<int, Q>();
            Enumerable.Range(0, levels).ForEach((i) =>
            {
                Q queue = new Q();
                queue.Statistics.Name = string.Format("Queue {0}", i);
                mTasksQueue.Add(i, queue);
            });
        } 
        #endregion

        protected override void StatisticsRecalculate(QueueTrackerStatistics stats)
        {
            base.StatisticsRecalculate(stats);

            stats.Queues = mTasksQueue.Values.Select((q) => q.Statistics).ToList();
        }

        public bool IsEmpty
        {
            get
            {
                int priority = mLevels;
                while (priority > 0)
                {
                    priority--;
                    if (!mTasksQueue[priority].IsEmpty)
                        return false;
                }

                return true;
            }
        }

        public int Count
        {
            get
            {
                return mTasksQueue.Values.Sum((q) => q.Count);
            }
        }

        public void Enqueue(TaskTracker tracker)
        {

            var payload = tracker.Context as TransmissionPayload;

            int priority = tracker.Priority??mLevels - 1;
            if (payload != null)
                priority = payload.Message.ChannelPriority;

            if (priority > mLevels - 1)
                priority = mLevels - 1;

            tracker.Priority = priority;

            mTasksQueue[priority].Enqueue(tracker);
        }

        public IEnumerable<TaskTracker> Dequeue(int itemCount = 1)
        {
            int priority = mLevels;
            while (itemCount > 0 && priority > 0)
            {
                priority--;
                if (mTasksQueue[priority].IsEmpty)
                    continue;

                TaskTracker tracker;
                while (itemCount > 0 && mTasksQueue[priority].TryDequeue(out tracker))
                {
                    itemCount --;
                    yield return tracker;
                }
            }
        }

        private string DebugCapacity
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                int priority = mLevels;

                while (priority > 0)
                {
                    priority--;
                    sb.Append(mTasksQueue[priority].Count);

                    if (priority>0)
                        sb.Append("/");
                }

                return sb.ToString();
            }
        }

        protected override void StartInternal()
        {
        }

        protected override void StopInternal()
        {
        }
    }
}
