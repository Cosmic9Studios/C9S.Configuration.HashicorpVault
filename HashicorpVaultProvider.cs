using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.Extensions.Configuration;
using VaultSharp;

namespace C9S.Configuration.HashicorpVault
{
    internal class HashicorpVaultProvider : ConfigurationProvider
    {
        private IVaultClient client;
        private KVVersion version;
        private IEnumerable<string> secretPaths;
        private const string Prefix = "HashicorpVault";

        /// <summary>
        /// Initializes a new instance of the <see cref="HashicorpVaultProvider" /> class.
        /// </summary>
        /// <param name="client">The vault client used to pull secrets.</param>
        /// <param name="version">The KV version you would like to pull secrets from.</param>
        /// <param name="secretPaths">The secret paths you would like to pull secrets from.</param>
        public HashicorpVaultProvider(IVaultClient client, KVVersion version, IEnumerable<string> secretPaths)
        {
            this.client = client;
            this.version = version;
            this.secretPaths = secretPaths;
        }

        public override void Load() => AsyncHelper.RunSync(LoadAsync);

        private async Task LoadAsync()
        {
            var dictionaries = new List<Dictionary<string, string>>();
            
            foreach (var secretPath in secretPaths)
            {
                if (version == KVVersion.V1)
                {
                    foreach (var path in (await client.V1.Secrets.KeyValue.V1.ReadSecretPathsAsync(secretPath)).Data.Keys)
                    {
                        dictionaries.Add((await client.V1.Secrets.KeyValue.V1.ReadSecretAsync($"{secretPath}/{path}")).Data
                            .ToDictionary(x => $"{Prefix}:{secretPath}:{path}", x => x.Value.ToString()));
                    }
                }
                else if (version == KVVersion.V2)
                {
                    foreach (var path in (await client.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync(secretPath)).Data.Keys)
                    {
                        dictionaries.Add((await client.V1.Secrets.KeyValue.V2.ReadSecretAsync($"{secretPath}/{path}")).Data.Data
                            .ToDictionary(x => $"{Prefix}:{secretPath}:{path}", x => x.Value.ToString()));
                    }
                }
            }

            Data = new Dictionary<string, string>(dictionaries.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value));
        }
    }
}
