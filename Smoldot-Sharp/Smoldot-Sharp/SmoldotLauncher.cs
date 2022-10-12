using System.Net.Security;
using SmoldotSharp.Msgs;

namespace SmoldotSharp
{
    using SmoldotChannel = Channel<SmoldotMsg>;

    public class SmoldotLauncher
    {
        readonly SmoldotConfig smoldotConfig;
        readonly DatabaseContentStorage dbStorage;
        readonly ChainspecProfile specProfile;
        readonly ISmoldotLogger logger;
        readonly TwoWayChannel<SmoldotMsg> ctrlQ;
        readonly (SmoldotChannel control, SmoldotChannel wasm) ctrlCh;

        public SmoldotChannel ChannelForControlInterface => ctrlCh.control;

        public DatabaseContentStorage DatabaseContentStorage => dbStorage;

        public ChainspecProfile ChainspecProfile => specProfile;

        public SmoldotLauncher(ISmoldotLogger logger, LauncherConfig launcherConfig,
            Obfuscator? obfuscator = null,
            RemoteCertificateValidationCallback? certificateValidationCallback = null)
        {
            this.logger = logger;
            ctrlQ = new TwoWayChannel<SmoldotMsg>();
            ctrlCh = ctrlQ.Open();
            dbStorage = new DatabaseContentStorage(logger, launcherConfig.DbConfig, obfuscator);
            specProfile = new ChainspecProfile(launcherConfig.SpecSourceList);
            smoldotConfig = new SmoldotConfig(dbStorage, specProfile, ctrlCh.wasm, 
                launcherConfig.wasmPath, launcherConfig.logLevel, launcherConfig.cpuRateLim,
                certificateValidationCallback);
        }

        public SmoldotWasmtime LaunchWasm()
        {
            return new SmoldotWasmtime(logger, smoldotConfig);
        }

        public SmoldotControlInterface GetControlInterface()
        {
            return new SmoldotControlInterface(ctrlCh.control);
        }
    }
}