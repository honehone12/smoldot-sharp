using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Linq;
using System.Collections.ObjectModel;
using Wasmtime;
using SmoldotSharp.Msgs;

namespace SmoldotSharp
{
    using Timer = System.Timers.Timer;
    using TimerChannel = OneWayChannel<SmoldotMsg>;

    public class SmoldotWasmtime : IDisposable
    {
        readonly ISmoldotLogger logger;
        readonly SmoldotConfig config;
        readonly Stopwatch stopwatch;
        readonly Dictionary<int, Timer> timerTable = new Dictionary<int, Timer>();
        readonly Dictionary<int, (bool isRelayChain, string name)> chainTable
            = new Dictionary<int, (bool, string)>();
        readonly HashSet<int> connectionSet = new HashSet<int>();
        readonly TimerChannel timerQ;
        readonly (TimerChannel.Tx tx, TimerChannel.Rx rx) timerCh;
        readonly Thread smoldotThread;
        
        SmoldotTransport? transport;
        SmoldotController? controller;
        Memory? memory;

        Action<int, int, int>? Init;
        Action? StartShutdown;
        Func<int, int>? Alloc;
        Func<int, int, int, int, int, int, int, int>? AddChain;
        Action<int>? RemoveChain;
        Func<int, int>? ChainIsOk;
        Func<int, int>? ChainErrorLen;
        Func<int, int>? ChainErrorPtr;
        Func<int, int, int, int>? JsonRpcSend;
        Func<int, int>? JsonRpcResponsePeek;
        Action<int>? JsonRpcResponsePop;
        Action<int>? TimerFinished;
        Action<int, int>? ConnectionOpenSingleStream;
        Action<int, int, int, int>? StreamMessage;
        Action<int, int, int>? ConnectionClosed;

        public SmoldotWasmtime(ISmoldotLogger logger, SmoldotConfig config)
        {
            this.logger = logger;
            this.config = config;
            timerQ = new TimerChannel();
            timerCh = timerQ.Open();
            stopwatch = new Stopwatch();
            var threadStart = new ThreadStart(SmoldotThreadMain);
            smoldotThread = new Thread(threadStart);
            smoldotThread.Start();
        }

        public void Dispose()
        {
            try
            {
                memory = null;
                stopwatch.Stop();
                foreach (var t in timerTable.Values)
                {
                    t.Close();
                }
                transport?.CloseAll();
                smoldotThread?.Join();
            }
            catch (Exception e)
            {
                logger.Log(SmoldotLogLevel.Error, $"Error on Dispose(): {e.Message}");
                Debug.Fail("Error on Dispose().");
            }
        }

        void SmoldotThreadMain()
        {
            try
            {
                logger.Log(SmoldotLogLevel.Debug, "Initializing smoldot... Reading wasm from file...");
                using var engine = new Engine();
                using var module = Module.FromFile(engine, config.wasmPath);
                using var linker = new Linker(engine);
                using var store = new Store(engine);
                store.SetWasiConfiguration(new WasiConfiguration().WithInheritedEnvironment());
                Export(linker);
                var instance = linker.Instantiate(store, module);
                Import(instance);
                memory = instance.GetMemory(SmoldotConfig.MemoryModuleName) ??
                    throw new InitializationFailedException(SmoldotConfig.MemoryModuleName);
                transport = new WebSocketTransport(logger, config.certificateValidationCallback);
                controller = new SmoldotController(logger, config.controlCh, 
                    config.specProfile, config.dbStorage,
                    new ReadOnlyDictionary<int, (bool, string)>(chainTable));

                stopwatch.Start();
                transport.OnOpen += OnConnectionOpenSingleStream;
                transport.OnReceived += OnTransportReceived;
                transport.OnClose += OnTransportClosed;
                transport.OnError += OnTransportError;
                controller.OnAddChainRequest += InvokeAddChain;
                controller.OnRemoveChainRequest += InvokeRemoveChain;
                controller.OnSendJsonRpcRequest += SendJsonRpc;
                controller.OnStartShutdownRequest += InvokeStartShutdown;
                InvokeInit();
                logger.Log(SmoldotLogLevel.Debug, "Initialize smoldot done.");
                controller.OnInitialize();

                ThreadMainLoop();
            }
            catch (Exception e)
            {
                memory = null;
                logger.Log(SmoldotLogLevel.Error, e.Message);
                Debug.Fail("Error");
            }
        }

        void ThreadMainLoop()
        {
            while (memory != null && transport != null && controller != null)
            {
                if (!transport.ShouldUpdate && !controller.ShouldUpdate && timerCh.rx.Count == 0)
                {
                    Thread.Sleep(ThreadConfig.Delay);
                    continue;
                }

                while (timerCh.rx.TryDequeue(out var msg))
                {
                    if (msg is TimerFinishedMsg m)
                    {
                        OnTimerFinished(m.id);
                    }
                }

                transport.Update();
                controller.Update();
            }

            logger.Log(SmoldotLogLevel.Debug, "Smoldot thread is closed.");
        }

        void Import(Instance instance)
        {
            var targetName = "init";
            Init = instance.GetAction<int, int, int>(targetName) 
                ?? throw new InitializationFailedException(targetName);

            targetName = "start_shutdown";
            StartShutdown = instance.GetAction(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "alloc";
            Alloc = instance.GetFunction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "add_chain";
            AddChain = instance.GetFunction<int, int, int, int, int, int, int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "remove_chain";
            RemoveChain = instance.GetAction<int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "chain_is_ok";
            ChainIsOk = instance.GetFunction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "chain_error_len";
            ChainErrorLen = instance.GetFunction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "chain_error_ptr";
            ChainErrorPtr = instance.GetFunction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "json_rpc_send";
            JsonRpcSend = instance.GetFunction<int, int, int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "json_rpc_responses_peek";
            JsonRpcResponsePeek = instance.GetFunction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "json_rpc_responses_pop";
            JsonRpcResponsePop = instance.GetAction<int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "timer_finished";
            TimerFinished = instance.GetAction<int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "connection_open_single_stream";
            ConnectionOpenSingleStream = instance.GetAction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "connection_open_multi_stream";
            ConnectionOpenMultiStream = instance.GetAction<int, int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "stream_message";
            StreamMessage = instance.GetAction<int, int, int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "connection_stream_opened";
            ConnectionStreamOpened = instance.GetAction<int, int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "connection_closed";
            ConnectionClosed = instance.GetAction<int, int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);

            targetName = "stream_closed";
            StreamClosed = instance.GetAction<int, int>(targetName)
                ?? throw new InitializationFailedException(targetName);
        }

        void InvokeInit()
        {
            Debug.Assert(Init != null);

            Init.Invoke((int)config.logLevel, config.logLevel >= SmoldotLogLevel.Trace ? 1 : 0, 
                SmoldotConfig.ConvertCpuRateLimit(config.cpuRateLim));
        }

        void InvokeAddChain(ChainSpecData chainSpec, DatabaseContent dbContent)
        {
            Debug.Assert(memory != null && controller != null
                && Alloc != null && AddChain != null);

            //allocate chainspec
            var chainSpecLen = chainSpec.size;
            var chainSpecAddr = Alloc.Invoke(chainSpecLen);
            memory.WriteString(chainSpecAddr, chainSpec.json);
            // allocate database
            var dbContentAddr = Alloc.Invoke(dbContent.bytesLen);
            memory.WriteString(dbContentAddr, dbContent.content);
            // allocate relaychain
            var relayChainList = chainTable.Where((kv) => kv.Value.isRelayChain).
                Select((kv) => kv.Key).ToList();
            var relayChainListLen = relayChainList.Count;
            var relayChainListAddr = Alloc.Invoke(relayChainListLen);
            for (int i = 0, addr = relayChainListAddr; i < relayChainListLen; i++, addr += 4)
            {
                memory.WriteInt32(addr, relayChainList[i]);
            }

            var id = AddChain.Invoke(chainSpecAddr, chainSpecLen, dbContentAddr, dbContent.bytesLen,
                chainSpec.allowJsonRpc ? 1 : 0, relayChainListAddr, relayChainListLen);

            if (CheckChainIsOk(id))
            {
                chainTable.Add(id, (chainSpec.isRelayChain, chainSpec.name));
                controller.OnChainAdded(id, chainSpec.name);
            }
        }

        void SendJsonRpc(int id, string json)
        {
            Debug.Assert(JsonRpcSend != null && Alloc != null && memory != null);

            if (!chainTable.ContainsKey(id) || !CheckChainIsOk(id))
            {
                return;
            }

            var len = Encoding.UTF8.GetByteCount(json);
            var ptr = Alloc.Invoke(len);
            memory.WriteString(ptr, json);
            var result = JsonRpcSend.Invoke(ptr, len, id);
            if (result > 0)
            {
                // Need error handling.
                logger.Log(SmoldotLogLevel.Error, $"[RPC-ERROR] code:{result} id: {id}, req: {json}");
            }
            else
            {
                logger.Log(SmoldotLogLevel.Debug, $"[RPC-SEND] id: {id}, req: {json}");
            }
        }

        void InvokeRemoveChain(int id)
        {
            Debug.Assert(RemoveChain != null);

            if (chainTable.ContainsKey(id))
            {
                RemoveChain.Invoke(id);
            }
        }

        bool CheckChainIsOk(int id)
        {
            Debug.Assert(memory != null && ChainIsOk != null && 
                ChainErrorLen != null && ChainErrorPtr != null);

            if (ChainIsOk.Invoke(id) == 0)
            {
                var errLen = ChainErrorLen.Invoke(id);
                var errPtr = ChainErrorPtr.Invoke(id);
                var message = memory.ReadString(errPtr, errLen);
                logger.Log(SmoldotLogLevel.Error, $"[CHAIN-ERROR] id: {id}, {message}");
                InvokeRemoveChain(id);

                Debug.Fail($"chain [{id}] is not ok.");
                return false;
            }

            return true;
        }

        void InvokeStartShutdown()
        {
            Debug.Assert(StartShutdown != null);

            // Startshutdown always throw and need to stop thread,
            // otherwise smoldot will continue runnig even after shutdown.
            try
            {
                StartShutdown.Invoke();
            }
            catch (Exception e)
            {
                var m = e.Message;
                if (m.StartsWith("Exited with i32 exit status 0"))
                {
                    m = "Smoldot exited with code 0.";
                    logger.Log(SmoldotLogLevel.Debug, m);
                }
                else
                {
                    logger.Log(SmoldotLogLevel.Error, m);
                }
            }
            memory = null;
        }

        void OnTimerFinished(int id)
        {
            Debug.Assert(TimerFinished != null);

            TimerFinished.Invoke(id);
            if (timerTable.ContainsKey(id))
            {
                timerTable[id].Close();
                timerTable.Remove(id);
            }
        }

        void OnConnectionOpenSingleStream(int id)
        {
            Debug.Assert(ConnectionOpenSingleStream != null);

            ConnectionOpenSingleStream.Invoke(id, 0);
        }

        void OnTransportReceived(int connId, int streamId, byte[] data)
        {
            Debug.Assert(memory != null && Alloc != null && StreamMessage != null);

            var len = data.Length;
            var ptr = Alloc.Invoke(len);
            memory.WriteBytes(ptr, data);
            StreamMessage.Invoke(connId, streamId, ptr, len);
        }

        void OnTransportClosed(int id, string reason)
        {
            if (connectionSet.Contains(id))
            {
                Debug.Assert(memory != null && Alloc != null && ConnectionClosed != null);

                var len = Encoding.UTF8.GetByteCount(reason);
                var ptr = Alloc.Invoke(len);
                memory.WriteString(ptr, reason);
                ConnectionClosed.Invoke(id, ptr, len);
                connectionSet.Remove(id);
            }
        }

        void OnTransportError(int id, string what)
        {
            // closing might be done in the sametime.
            // so, just logging for now.
            logger.Log(SmoldotLogLevel.Error, $"Transport error id:{id}, what:{what}");
        }

        void Export(Linker linker)
        {
            var smoldotMod = "smoldot";
            linker.DefineWasi();

            linker.DefineFunction<int, int>(
                smoldotMod, "panic", Panic
            );
            linker.DefineFunction<int>(
                smoldotMod, "json_rpc_responses_non_empty", JsonRpcResponseNonEmpty
            );
            linker.DefineFunction<int, int, int, int, int>(
                smoldotMod, "log", Log
            );
            linker.DefineFunction<double>(
                smoldotMod, "unix_time_ms", UnixTimeMs
            );
            linker.DefineFunction<double>(
                smoldotMod, "monotonic_clock_ms", MonotonicClockMs
            );
            linker.DefineFunction<int, double>(
                smoldotMod, "start_timer", StartTimer
            );
            linker.DefineFunction<int, int, int, int, int>(
                smoldotMod, "connection_new", ConnectionNew
            ) ;
            linker.DefineFunction<int>(
                smoldotMod, "connection_close", ConnectionClose
            );
            linker.DefineFunction<int>(
                smoldotMod, "connection_stream_open", ConnectionStreamOpen
            );
            linker.DefineFunction<int, int>(
                smoldotMod, "connection_stream_close", ConnectionStreamClose
            );
            linker.DefineFunction<int, int, int, int>(
                smoldotMod, "stream_send", StreamSend
            );
            linker.DefineFunction<int, int>(
                smoldotMod, "current_task_entered", CurrentTaskEntered
            );
            linker.DefineFunction(
                smoldotMod, "current_task_exit", CurrentTaskExit
            );

         
        }

        void Panic(int ptr, int len)
        {
            Debug.Assert(memory != null && transport != null && controller != null);

            var what = memory.ReadString(ptr, len);
            logger.Log(SmoldotLogLevel.Error, $"[SMOLDOT-PANIC] {what}");
            controller.OnPanic();
            transport.CloseAll();
            InvokeStartShutdown();
        }

        void JsonRpcResponseNonEmpty(int chainId)
        {
            Debug.Assert(memory != null && controller != null
                && JsonRpcResponsePeek != null && JsonRpcResponsePop != null);

            while (true)
            {
                var infoPtr = JsonRpcResponsePeek(chainId);
                var ptr = memory.ReadInt32(infoPtr);
                var len = memory.ReadInt32(infoPtr + 4);
                if (len <= 0)
                {
                    return;
                }

                var res = memory.ReadString(ptr, len);
                Debug.Assert(chainTable.ContainsKey(chainId));
                logger.Log(SmoldotLogLevel.Debug, $"[RPC-RESPONSE] id: {chainId}, res: {res}");
                controller.OnJsonRpcRespond(chainId, chainTable[chainId].name, res);
                JsonRpcResponsePop(chainId);
            }
        }

        void Log(int level, int targetPtr, int targetLen, int messagePtr, int messageLen)
        {
            Debug.Assert(memory != null);

            var target = memory.ReadString(targetPtr, targetLen);
            var msg = memory.ReadString(messagePtr, messageLen);
            logger.Log((SmoldotLogLevel)level, $"[SMOLDOT-LOG] target: {target}, message: {msg}");
        }

        double UnixTimeMs()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        double MonotonicClockMs()
        {
            return stopwatch.Elapsed.TotalMilliseconds;
        }

        void StartTimer(int id, double milliseconds)
        {
            if (milliseconds <= 0.0)
            {
                timerCh.tx.Enqueue(new TimerFinishedMsg(id));
                return;
            }

            var timer = new Timer
            {
                Interval = milliseconds,
                AutoReset = false
            };
            timer.Elapsed += (_, __) => timerCh.tx.Enqueue(new TimerFinishedMsg(id));
            timer.Start();
        }

        int ConnectionNew(int id, int addrPtr, int addrLen, int errorPtrPtr)
        {
            Debug.Assert(memory != null && transport != null && Alloc != null);

            var addrRaw = memory.ReadString(addrPtr, addrLen);
            var (ok, err) = transport.NewConnection(id, addrRaw);
            if (ok)
            {
                Debug.Assert(!connectionSet.Contains(id));
                connectionSet.Remove(id);
                connectionSet.Add(id);
                return 0;
            }
            else
            {
                var errCode = 1;
                var len = Encoding.UTF8.GetByteCount(err);
                var alloc = Alloc.Invoke(len);
                memory.WriteString(alloc, err);
                memory.WriteInt32(errorPtrPtr, alloc);
                memory.WriteInt32(errorPtrPtr + 4, len);
                memory.WriteInt32(errorPtrPtr + 8, errCode);
                return errCode;
            }
        }

        void ConnectionClose(int connectionId)
        {
            Debug.Assert(transport != null);

            if (connectionSet.Contains(connectionId))
            {
                connectionSet.Remove(connectionId);
                transport.Close(connectionId);
            }
        }

        void StreamSend(int connectionId, int streamId, int ptr, int len)
        {
            Debug.Assert(memory != null && transport != null);

            var data = memory.ReadBytes(ptr, len);
            transport.Message(connectionId, streamId, data);
        }

        void CurrentTaskEntered(int ptr, int len)
        {
            Debug.Assert(memory != null);

            var taskName = memory.ReadString(ptr, len);
            logger.Log(SmoldotLogLevel.Trace, $"[SMOLDOT-TASK-ENTER] task: {taskName}");
        }

        void CurrentTaskExit()
        {
            logger.Log(SmoldotLogLevel.Trace, "[SMOLDOT-TASK-EXIT]");
        }

        #region MultiStreamFeatures NotCoveredYet

        Action<int, int, int>? ConnectionOpenMultiStream;
        Action<int, int, int>? ConnectionStreamOpened;
        Action<int, int>? StreamClosed;

        void ConnectionStreamOpen(int connectionId)
        {
            throw new NotImplementedException("connection_stream_open called, but not implemented.");
        }

        void ConnectionStreamClose(int connectionId, int streamId)
        {
            throw new NotImplementedException("connection_stream_close called, but not implemented.");
        }

        #endregion
    }
}