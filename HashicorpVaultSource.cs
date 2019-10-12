using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using VaultSharp;

namespace C9S.Configuration.HashicorpVault
{
    internal class HashicorpVaultSource : IConfigurationSource
    {
        public IVaultClient Client { get; set; }
        public KVVersion Version { get; set; }
        public IEnumerable<string> SecretPaths { get; set; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new HashicorpVaultProvider(Client, Version, SecretPaths);
        }
    }
}
