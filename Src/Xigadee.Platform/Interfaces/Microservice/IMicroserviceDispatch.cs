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

namespace Xigadee
{
    public interface IMicroserviceDispatch
    {
        void Process(TransmissionPayload payload);

        void Process(ServiceMessage message, ProcessOptions options = ProcessOptions.RouteInternal | ProcessOptions.RouteExternal, Action<bool, Guid> release = null, bool isDeadLetterMessage = false);

        void Process(ServiceMessageHeader header, object package = null, int ChannelPriority = 1, ProcessOptions options = ProcessOptions.RouteInternal | ProcessOptions.RouteExternal, Action<bool, Guid> release = null, bool isDeadLetterMessage = false);

        void Process(string ChannelId, string MessageType = null, string ActionType = null, object package = null, int ChannelPriority = 1, ProcessOptions options = ProcessOptions.RouteInternal | ProcessOptions.RouteExternal, Action<bool, Guid> release = null, bool isDeadLetterMessage = false);

        void Process<C>(object package = null, int ChannelPriority = 1, ProcessOptions options = ProcessOptions.RouteInternal | ProcessOptions.RouteExternal, Action<bool, Guid> release = null, bool isDeadLetterMessage = false)
            where C : IMessageContract;
    }
}
