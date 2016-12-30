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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Xigadee
{
    public static partial class AzureExtensionMethods
    {
        public const string AzureStorageGroupName = "AzureStorage";

        [ConfigSettingKey(AzureStorageGroupName)]
        public const string KeyAzureStorageAccountName = "AzureStorageAccountName";

        [ConfigSettingKey(AzureStorageGroupName)]
        public const string KeyAzureStorageAccountAccessKey = "AzureStorageAccountAccessKey";

        [ConfigSetting(AzureStorageGroupName)]
        public static string AzureStorageAccountName(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyAzureStorageAccountName);

        [ConfigSetting(AzureStorageGroupName)]
        public static string AzureStorageAccountAccessKey(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyAzureStorageAccountAccessKey);

        [ConfigSetting(AzureStorageGroupName)]
        public static StorageCredentials AzureStorageCredentials(this IEnvironmentConfiguration config, bool throwExceptionIfMissing = true)
        {
            if (string.IsNullOrEmpty(config.AzureStorageAccountName()) || string.IsNullOrEmpty(config.AzureStorageAccountAccessKey()))
                if (throwExceptionIfMissing)
                    throw new Exception();
                else
                    return null;

            return new StorageCredentials(config.AzureStorageAccountName(), config.AzureStorageAccountAccessKey());
        }
    }
}