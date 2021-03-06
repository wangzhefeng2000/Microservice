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

#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
#endregion
namespace Xigadee
{
    public static partial class ConversionHelper
    {

        #region CalculateDelta(int now, int start)
        /// <summary>
        /// This method calculates the delta and takes in to account that the
        /// tickcount recycles to negative every 49 days.
        /// </summary>
        /// <param name="now"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int CalculateDelta(int now, int start)
        {
            int delta;
            if (now >= start)
                delta = now - start;
            else
            {
                //Do this, otherwise you'll be in a world of pain every 49 days.
                long upLimit = ((long)(int.MaxValue)) + Math.Abs(int.MinValue - now);
                delta = (int)(upLimit - start);
            }

            return delta;
        }
        #endregion

        public static int DeltaAsMs(int tickStart, int? tickNow = null)
        {
            return CalculateDelta(tickNow ?? Environment.TickCount, tickStart);
        }

        public static TimeSpan? DeltaAsTimeSpan(int? tickStart, int? tickNow = null)
        {
            if (!tickStart.HasValue)
                return null;

            return TimeSpan.FromMilliseconds(DeltaAsMs(tickStart.Value, tickNow));
        }

        public static string DeltaAsFriendlyTime(int tickStart, int? tickNow = null)
        {
            return ToFriendlyString(DeltaAsTimeSpan(tickStart, tickNow));
        }

        #region ToFriendlyString(TimeSpan? time, string defaultText="NA")

        static readonly Func<TimeSpan?, string> fnTimeConv = (time) =>
        {
            try
            {
                if (Math.Abs(time.Value.TotalMilliseconds) < 1000)
                    return string.Format("{0:F2}ms", time.Value.TotalMilliseconds);

                if (Math.Abs(time.Value.Days) > 0)
                    return time.Value.ToString(@"d'day'hh'h'mm'm'ss'.'ff's'");
                if (Math.Abs(time.Value.Hours) > 0)
                    return time.Value.ToString(@"hh'h'mm'm'ss'.'ff's'");
                if (Math.Abs(time.Value.Minutes) > 0)
                    return time.Value.ToString(@"mm'm'ss'.'ff's'");

                return time.Value.ToString(@"ss'.'ff's'");
            }
            catch (Exception)
            {
                return null;
            }
        };
        /// <summary>
        /// This helper converts a timespan in to a human readable time.
        /// </summary>
        /// <param name="time">The TimeSpan object to convert.</param>
        /// <param name="defaultText">The default text to display if the TimeSpan object is null. NA by default.</param>
        /// <returns>Returns a string representation of the time.</returns>
        public static string ToFriendlyString(this TimeSpan? timeIn, string nullName = "NA")
        {
            return timeIn.HasValue ? ToFriendlyString(timeIn.Value) : nullName;
        }
        /// <summary>
        /// This helper converts a timespan in to a human readable time.
        /// </summary>
        /// <param name="time">The TimeSpan object to convert.</param>
        /// <returns>Returns a string representation of the time.</returns>
        public static string ToFriendlyString(this TimeSpan timeIn)
        {
            string output = fnTimeConv(timeIn);

            if (output == null)
                return "ERR";

            if (timeIn.TotalMilliseconds < 0)
                return "-" + output;

            return output;
        }
        #endregion

    }
}
