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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Xigadee
{
    /// <summary>
    /// This class contains a brief summary the resource status
    /// </summary>
    public class ResourceStatus
    {
        public string Name { get; set; }

        public CircuitBreakerState State { get; set; } = CircuitBreakerState.Closed;

        public int? RetryInSeconds { get; set; }

        public int FilterPercentage { get; set; } = 100;
    }
}
