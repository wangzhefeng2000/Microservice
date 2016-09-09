﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Auth;

namespace Xigadee
{
    public static partial class AzureExtensionMethods
    {
        [ConfigSettingKey("Storage")]
        public const string KeyStorageAccountName = "StorageAccountName";
        [ConfigSettingKey("Storage")]
        public const string KeyStorageAccountAccessKey = "StorageAccountAccessKey";

        [ConfigSetting("Storage")]
        public static string StorageAccountName(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyStorageAccountName);

        [ConfigSetting("Storage")]
        public static string StorageAccountAccessKey(this IEnvironmentConfiguration config) => config.PlatformOrConfigCache(KeyStorageAccountAccessKey);

        [ConfigSetting("Storage")]
        public static StorageCredentials StorageCredentials(this IEnvironmentConfiguration config)
        {
            if (string.IsNullOrEmpty(config.StorageAccountName()) || string.IsNullOrEmpty(config.StorageAccountAccessKey()))
                return null;

            return new StorageCredentials(config.StorageAccountName(), config.StorageAccountAccessKey());
        }
    }
}