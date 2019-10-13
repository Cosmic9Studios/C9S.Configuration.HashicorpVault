using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using C9S.Configuration.HashicorpVault.Helpers;
using Microsoft.Extensions.Configuration;
using VaultSharp;
using VaultSharp.V1.SecretsEngines;
using VaultSharp.V1.SecretsEngines.KeyValue;

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

        private async Task<IEnumerable<string>> GetKeys(string secretPath) =>
            version == KVVersion.V1 ? 
                (await client.V1.Secrets.KeyValue.V1.ReadSecretPathsAsync(secretPath)).Data.Keys : 
                (await client.V1.Secrets.KeyValue.V2.ReadSecretPathsAsync(secretPath)).Data.Keys;
        
        private async Task<Dictionary<string, object>> GetData(string fullPath) => 
            version == KVVersion.V1 ? 
                (await client.V1.Secrets.KeyValue.V1.ReadSecretAsync(fullPath)).Data :
                (await client.V1.Secrets.KeyValue.V2.ReadSecretAsync(fullPath)).Data.Data;


        private async Task LoadAsync()
        {
            var dictionaries = new List<Dictionary<string, string>>();
    
            foreach (var secretPath in secretPaths)
            {
                foreach (var path in await GetKeys(secretPath))
                {
                    var fullPath = string.IsNullOrWhiteSpace(secretPath) ? path : $"{secretPath}/{path}";
                    var data = await GetData(fullPath);
                    dictionaries.Add(data.ToDictionary(x => $"{Prefix}:{fullPath.Replace('/', ':')}:{x.Key}", x => x.Value.ToString()));
                }
            }

            Data = new Dictionary<string, string>(dictionaries.SelectMany(dict => dict).ToDictionary(pair => pair.Key, pair => pair.Value));
        }
    }
}
