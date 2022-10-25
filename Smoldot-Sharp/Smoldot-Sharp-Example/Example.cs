using System.Diagnostics;
using Newtonsoft.Json.Linq;
using ScaleSharpLight;
using SmoldotSharp;
using SmoldotSharp.JsonRpc;

namespace SmoldotSharpExample
{
    internal class Example
    {
        const string BobUri =   "5FHneW46xGXgs5mUiveU4sbTyGBzmstUspZC92UhjJM694ty";
        const string AliceUri = "5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY";

        // list of chain spec json files.
        public static readonly ChainSpecSource[] SpecSource =
        {
            new ChainSpecSource(
                /*path*/ "rococo-local-testnet.json",
                /*name*/ "rococo-local-testnet",
                /*is relay chain?*/true, /*allow json rpc?*/true)
        };
        // save database file locally or not.
        const bool StoreLocally = true;
        // file extension of local file. '.'is NOT needed.
        const string LocalFileExtension = "dbc";
        // directory to save local file.
        const string LocalFileDir = "";
        // smoldot wasm path.
        const string WasmPath = "smoldot_light_wasm.wasm";
        // cpu usage limit for wasm. range is 0.0 ~ 1.0.
        const double CpuRateLim = 1.0;

        const int MainThreadDelay = 50;
        
        static async Task Main()
        {
            ThreadConfig.Delay = 10;
            var loop = true;
            var launcherConfig = new LauncherConfig(SpecSource,
                StoreLocally, LocalFileExtension, LocalFileDir,
                WasmPath, SmoldotLogLevel.Info, CpuRateLim);
            var logger = new SmoldotDevLogger(SmoldotLogLevel.Debug);
            var launcher = new SmoldotLauncher(logger, launcherConfig);
            
            var control = launcher.GetControlInterface();
            var bgUpdateTask = control.StartBackgroundUpdateAsync(15);
            var chains = launcher.ChainspecProfile.GetAllSource;
            using var smoldotWasm = launcher.LaunchWasm();
            
            control.OnPanic += () => loop = false;
            control.OnInitialized += () => control.AddChain(chains[0].name);

            //var obfuscator = new AesHmac(logger,
            //    AesHmacKeys.TestAes32Hmac64Keys, HmacFunc.HmacSha512);
            //launcher.DatabaseContentStorage.SetObfuscator(obfuscator);
            //DatabaseConfig.MagicPhrase = "SetMagicPhraseAndChangeItWhenChainSpecsAreChanged";

            while (loop)
            {
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(MainThreadDelay);
                    continue;
                }

                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.B:
                        await control.AddChainAsync(chains[0].name);
                        PrintMetadata();
                        break;
                    case ConsoleKey.D:
                        control.SaveDatabaseContent(chains[0].name);
                        break;
                    case ConsoleKey.M:
                        control.SendJsonRpc(chains[0].name, new Rpc<JObject>("rpc_methods"));
                        break;
                    case ConsoleKey.N:
                        var ctxKeyN = control.SendJsonRpc(chains[0].name, new Rpc<string>("system_name"));
                        var result = await ctxKeyN.GetResultAsync();
                        Debug.Assert(result != null);
                        var (o, s) = result.UnboxAsString();
                        Debug.Assert(o);
                        logger.Log(SmoldotLogLevel.Info, s);
                        break;
                    case ConsoleKey.O:
                        break;
                    case ConsoleKey.P:
                        PrintMetadata();
                        break;
                    case ConsoleKey.Q:
                        control.StartShutdown();
                        break;
                    case ConsoleKey.R:
                        control.RemoveChain(chains[0].name);
                        break;
                    case ConsoleKey.S:
                        var sub = SendBalanceTransfer(control, chains[0].name);
                        break;
                    case ConsoleKey.X:
                        loop = false;
                        break;
                    default:
                        break;
                }
            }

            control.StopBackgroundUpdate();
            await bgUpdateTask;
        }

        static void PrintMetadata()
        {
            var metaStr = Metadata.String();
            Debug.Assert(metaStr != null);
            var metaLen = metaStr.Count;
            for (int i = 0; i < metaLen; i++)
            {
                Console.WriteLine(metaStr[i]);
            }
        }

        static SubscriptionContext SendBalanceTransfer(SmoldotControlInterface control, string chainName)
        {
            var ok = BobUri.AsSpan().TrySS58Decode(out var bobPublicKey, out _);
            Debug.Assert(ok);
            (ok, var bobAddress) = MultiAddress.New(bobPublicKey.ToArray());
            Debug.Assert(ok);
            var val = Compact.CompactInteger(100000000000000ul);
            var data = new byte[1 + MultiAddress.Size + val.CompactEncodedSize()];
            var databuff = new Span<byte>(data);
            var pos = 0;
            databuff[pos++] = bobAddress.multiAddrPrefix;
            bobAddress.GetAccountId.CopyTo(databuff[pos..]);
            pos += MultiAddress.Size;
            pos += val.CompactEncode(databuff[pos..]);
            Debug.Assert(pos == databuff.Length);
            (ok, var aliceKey) = KeySeed.New(KeySeed.Alice);
            Debug.Assert(ok);
            var rpc = new SignedRpc(AliceUri, aliceKey, "Balances", "transfer", data);
            return control.SendAndWatchRpc(chainName, rpc);
        }
    }
}