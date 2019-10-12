# C9S.Configuration.HashicorpVault
Adds vault secrets to configuration using VaultSharp

# What is C9S.Configuration.HashicorpVault

C9S.Configuration.HashicorpVault is a Configuration Provider to easily add K/V secrets to your Configuration. 

# How it Works 

The HashicorpVault provider parses all the secrets your specify and adds it to the ConfigurationBuilder. 
Each secret added is prefixed with "HashicorpVault" this is used to avoid any possible conflicts. 

Example: Let's say my secret path is `secret/test` and I have the following value: `mykey=myvalue`. 
The provider will add `HashicorpVault:test:mykey` to the ConfigurationBuilder with the value `myvalue`. 

You can thet access the value values by getting the "HashicorpVault" section: `Configuration.GetSection("HashicorpVault")`

# How to Install 

`dotnet add package C9S.Configuration.HashicorpVault`

# Example Usage 

Program.cs

```cs
public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    WebHost.CreateDefaultBuilder(args)
        .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
        .ConfigureAppConfiguration((builderContext, config) =>
        {
            var authMethod = new GoogleCloudAuthMethodInfo("my-iam-role", "jwt");
            var vaultClientSettings = new VaultClientSettings("https://domain.com:8200", authMethod);
            var vaultClient = new VaultClient(vaultClientSettings);

            var env = builderContext.HostingEnvironment; 
            config.AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{env.EnvironmentName}.json")
                    .AddHashicorpVault(vaultClient, KVVersion.V1, "test"); 
        })
        .UseKestrel(options => 
        {   
            options.Listen(IPAddress.Any, 5000, listenOptions => {});
        })
        .UseStartup<Startup>();
```

# Bonus

Consider using this with the `C9S.Configuration.Variables` package. 

appsettings.json 

```json
{
    "MySecret": "${HashicorpVault.test.mykey}"
}
```

Startup.cs

```cs
public void ConfigureServices(IServiceCollection services)
{
    ...

    Configuration.ResolveVariables("${", "}"); // From the C9S.Configuration.Variables package
    var mySecret = Configuration.GetSection("MySecret"); // Value = "myvalue"

    ...
}
```