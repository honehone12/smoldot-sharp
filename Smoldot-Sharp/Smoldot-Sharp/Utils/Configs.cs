using SmoldotSharp.Msgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;

namespace SmoldotSharp
{
    public static class ThreadConfig
    {
        static int DelayBackingField = 10;

        public static int Delay
        {
            get
            {
                return DelayBackingField;       
            }
            set
            {
                value = value < 0 ? 0 : value;
                value = value > 1000 ? 1000 : value;
                DelayBackingField = value;
            }
        }
    }

    public partial class LauncherConfig
    {
        public readonly ChainSpecSource[] specSource;
        public readonly bool storeLocally;
        public readonly string localFileExtension;
        public readonly string localFileDir;
        public readonly string wasmPath;
        public readonly SmoldotLogLevel logLevel;
        public readonly double cpuRateLim;

        public LauncherConfig(ChainSpecSource[] specSource,
            bool storeLocally, string localFileExtension, string localFileDir,
            string wasmPath, SmoldotLogLevel logLevel, double cpuRateLim)
        {
            this.specSource = specSource;
            this.storeLocally = storeLocally;
            this.localFileExtension = localFileExtension;
            this.localFileDir = localFileDir;
            this.wasmPath = wasmPath;
            this.logLevel = logLevel;
            this.cpuRateLim = cpuRateLim;
        }

        public DatabaseConfig DbConfig
            => new DatabaseConfig(storeLocally, localFileExtension, localFileDir);

        public List<ChainSpecSource> SpecSourceList
            => specSource.ToList();
    }

    public class SmoldotConfig
    {
        public const string MemoryModuleName = "memory";

        public readonly string wasmPath;
        public readonly SmoldotLogLevel logLevel;
        public readonly double cpuRateLim;
        public readonly DatabaseContentStorage dbStorage;
        public readonly ChainspecProfile specProfile;
        public readonly Channel<SmoldotMsg> controlCh;
        public readonly RemoteCertificateValidationCallback? certificateValidationCallback;

        public SmoldotConfig(DatabaseContentStorage dbStorage, ChainspecProfile specProfile,
            Channel<SmoldotMsg> controlCh, string wasmPath, SmoldotLogLevel logLevel, double cpuRateLim,
            RemoteCertificateValidationCallback? certificateValidationCallback = null)
        {
            this.dbStorage = dbStorage;
            this.specProfile = specProfile;
            this.controlCh = controlCh;
            this.wasmPath = wasmPath;
            this.logLevel = logLevel;
            this.cpuRateLim = cpuRateLim;
            this.certificateValidationCallback = certificateValidationCallback;
        }

        public static unsafe int ConvertCpuRateLimit(double zero2One)
        {
            if (zero2One > 1.0f || zero2One < 0.0f)
            {
                zero2One = 1.0f;
            }

            var r = Math.Truncate(uint.MaxValue * zero2One);
            var rateValue = (uint)r;
            void* ptr = &rateValue;
            return *(int*)ptr;
        }
    }

    public class DatabaseConfig
    {
        static string MagicPhraseBackingField 
            = "15914a21605u19894103524l412824441m22r1254z152m4159168411d3207t72";

        /// <summary>
        /// MagicPhrase acts like magic-number, added to database content.
        /// If this value does not match with the one stored before,
        /// content stored will be discarded.
        /// </summary>
        public static string MagicPhrase
        {
            get
            {
                return MagicPhraseBackingField;
            }
            set
            {
                if (value.Length >= 16)
                {
                    MagicPhraseBackingField = value;
                }
                else
                {
                    throw new Exception("MagicPhrase is expected loger than 16 in length.");
                }
            }
        }

        public readonly bool storeLocally;
        public readonly string localFileExtension;
        public readonly string localFileDir;

        public DatabaseConfig(bool storeLocally, string localFileExtension, 
            string localFileDir)
        {
            this.storeLocally = storeLocally;
            this.localFileExtension = localFileExtension;
            this.localFileDir = localFileDir;
        }
    }
}