using System;
using Microsoft.Extensions.Configuration;
using VaultSharp;

namespace C9S.Configuration.HashicorpVault
{
    public static class HashicorpVaultExtensions
    {
        public static IConfigurationBuilder AddHashicorpVault(
            this IConfigurationBuilder configurationBuilder, IVaultClient client, KVVersion version, params string[] secretPaths)
        {
            if (configurationBuilder == null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (secretPaths == null)
            {
                throw new ArgumentNullException(nameof(secretPaths));
            }

            configurationBuilder.Add(new HashicorpVaultSource()
            {
                Client = client,
                Version = version,
                SecretPaths = secretPaths,
            });

            return configurationBuilder;
        }
    }
}