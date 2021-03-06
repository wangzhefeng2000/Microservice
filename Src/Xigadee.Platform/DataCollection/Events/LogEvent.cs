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
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This is the root class for logging events for the Microservice framework.
    /// </summary>
    [DebuggerDisplay("{Level} {Category} ")]
    public class LogEvent: EventBase
    {
        #region Constructors
        protected LogEvent() { }

        public LogEvent(Exception ex) : this(LoggingLevel.Error, null, null, ex)
        {
        }
        public LogEvent(string message, Exception ex) : this(LoggingLevel.Error, message, null, ex)
        {

        }
        public LogEvent(string message) : this(LoggingLevel.Info, message, null, null)
        {

        }
        public LogEvent(LoggingLevel level, string message) : this(level, message, null, null)
        {

        }
        public LogEvent(LoggingLevel level, string message, string category) : this(level, message, category, null)
        {

        }

        public LogEvent(LoggingLevel level, string message, string category, Exception ex)
        {
            Level = level;
            Message = message;
            Category = category;
            Ex = ex;

            AdditionalData = new Dictionary<string, string>();
        }
        #endregion

        public virtual LoggingLevel Level { get; set; }

        public virtual string Message { get; set; }

        public virtual string Category { get; set; }

        public virtual Exception Ex { get; set; }

        public virtual Dictionary<string, string> AdditionalData { get; }
    }
}
