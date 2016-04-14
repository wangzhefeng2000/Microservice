﻿#region using
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#endregion
namespace Xigadee
{
    /// <summary>
    /// This is a transient class that shows the current task slot availability.
    /// </summary>
    public class TaskAvailability: StatisticsBase<TaskAvailabilityStatistics>, ITaskAvailability
    {
        #region Declarations
        /// <summary>
        /// This collection holds a reference to the active tasks to ensure that returning or killed processes are not counted twice.
        /// </summary>
        private ConcurrentDictionary<long, Guid> mActiveBag;
        /// <summary>
        /// This is the current count of the internal running tasks.
        /// </summary>
        private TaskManagerPrioritySettings mPriorityInternal;
        /// <summary>
        /// This is the status requests.
        /// </summary>
        private TaskManagerPrioritySettings[] mPriorityStatus;

        /// <summary>
        /// This is the incremental process counter.
        /// </summary>
        private long mProcessSlot = 0;

        /// <summary>
        /// This is the current count of the killed tasks that have been freed up because the task has failed to return.
        /// </summary>
        private int mTasksKilled = 0;
        /// <summary>
        /// This is the number of killed processes that did return.
        /// </summary>
        private long mTasksKilledDidReturn = 0;

        private readonly int mTasksMaxConcurrent;
        #endregion
        #region Constructor
        /// <summary>
        /// This constructor sets the allowed levels.
        /// </summary>
        /// <param name="levels">The level count.</param>
        /// <param name="maxConcurrent">The maximum number of allowed executing tasks.</param>
        public TaskAvailability(int levels, int maxConcurrent)
        {
            Levels = levels;
            mTasksMaxConcurrent = maxConcurrent;

            mPriorityInternal = new TaskManagerPrioritySettings(TaskTracker.PriorityInternal);

            mPriorityStatus = new TaskManagerPrioritySettings[levels];
            Enumerable.Range(0, levels).ForEach((l) => mPriorityStatus[l] = new TaskManagerPrioritySettings(l));

            mActiveBag = new ConcurrentDictionary<long, Guid>();
        }
        #endregion

        protected override void StatisticsRecalculate()
        {
            base.StatisticsRecalculate();
            try
            {
                mStatistics.TasksMaxConcurrent = mTasksMaxConcurrent;

                mStatistics.Active = mActiveBag.Count;
                mStatistics.SlotsAvailable = Count;

                mStatistics.Killed = mTasksKilled;
                mStatistics.KilledDidReturn = mTasksKilledDidReturn;

                if (mPriorityStatus != null)
                    mStatistics.Levels = mPriorityStatus
                        .Union(new TaskManagerPrioritySettings[] { mPriorityInternal })
                        .OrderByDescending((s) => s.Level)
                        .Select((s) => s.Debug)
                        .ToArray();
            }
            catch (Exception ex)
            {
                mStatistics.Ex = ex;
            }
        }

        #region BulkheadReserve(int level, int slotCount)
        /// <summary>
        /// This method sets the bulk head reservation for a particular level.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="slotCount"></param>
        /// <returns></returns>
        public bool BulkheadReserve(int level, int slotCount)
        {

            if (slotCount < 0)
                return false;

            if (level < LevelMin || level > LevelMax)
                return false;

            mPriorityStatus[level].BulkHeadReserve(slotCount);

            return true;
        }
        #endregion

        #region LevelMin
        /// <summary>
        /// This is the minimum task priority level.
        /// </summary>
        public int LevelMin { get { return 0; } }
        #endregion
        #region LevelMax
        /// <summary>
        /// This is the maximum task priority level.
        /// </summary>
        public int LevelMax { get { return Levels - 1; } }
        #endregion

        #region Levels
        /// <summary>
        /// This is the maximum number of priority levels
        /// </summary>
        public int Levels { get; }
        #endregion

        #region Level(int priority)
        /// <summary>
        /// Find any available slots for the level.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <returns>returns the available slots.</returns>
        public int Level(int priority)
        {
            if (Count > 0)
                return Count;

            if (priority > LevelMax)
                priority = LevelMax;

            if (priority < LevelMin)
                priority = LevelMin;

            //OK, do we have any bulkhead reservation.
            do
            {
                int available = mPriorityStatus[priority].Available;

                if (available > 0)
                    return available;

                priority--;
            }
            while (priority >= LevelMin);

            return 0;
        }
        #endregion

        #region Increment(TaskTracker tracker)
        /// <summary>
        /// This method adds a tracker to the availability counters.
        /// </summary>
        /// <param name="tracker">The tracker to add.</param>
        /// <returns>Returns the tracker id.</returns>
        public long Increment(TaskTracker tracker)
        {
            if (tracker.ProcessSlot.HasValue && mActiveBag.ContainsKey(tracker.ProcessSlot.Value))
                throw new ArgumentOutOfRangeException($"The tracker has already been submitted.");

            tracker.ProcessSlot = Interlocked.Increment(ref mProcessSlot);
            if (!mActiveBag.TryAdd(tracker.ProcessSlot.Value, tracker.Id))
                throw new ArgumentOutOfRangeException($"The tracker has already been submitted.");

            if (tracker.IsInternal)
                mPriorityInternal.Increment();
            else
                mPriorityStatus[tracker.Priority.Value].Increment();

            return tracker.ProcessSlot.Value;
        } 
        #endregion
        #region Decrement(TaskTracker tracker, bool force = false)
        /// <summary>
        /// This method removes a tracker from the availability counters.
        /// </summary>
        /// <param name="tracker">The tracker to remove.</param>
        /// <param name="force">A flag indicating whether the tracker was forceably deleted.</param>
        public void Decrement(TaskTracker tracker, bool force = false)
        {
            if (!tracker.ProcessSlot.HasValue)
                throw new ArgumentOutOfRangeException($"The tracker does not have a process slot set.");

            Guid value;
            if (!mActiveBag.TryRemove(tracker.ProcessSlot.Value, out value))
                return;

            //Remove the internal task count.
            if (tracker.IsInternal)
                mPriorityInternal.Decrement(tracker.IsKilled);
            else
                mPriorityStatus[tracker.Priority.Value].Decrement(tracker.IsKilled);

            if (tracker.IsKilled)
            {
                Interlocked.Decrement(ref mTasksKilled);
                if (!force)
                    Interlocked.Increment(ref mTasksKilledDidReturn);
            }
        } 
        #endregion

        #region Count
        /// <summary>
        /// This figure is the number of remaining task slots available. Internal tasks are removed from the running tasks.
        /// </summary>
        public int Count
        {
            get
            {
                return mTasksMaxConcurrent - (mActiveBag.Count - mPriorityInternal.Active);
            }
        }
        #endregion
    }
}