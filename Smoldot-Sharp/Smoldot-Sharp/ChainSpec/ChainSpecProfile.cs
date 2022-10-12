using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SmoldotSharp
{
    public class ChainSpecSource
    {
        public readonly string path;
        public readonly string name;
        public readonly bool isRelayChain;
        public readonly bool allowJsonRpc;

        public ChainSpecSource()
        {
            path = string.Empty;
            name = string.Empty;
        }

        public ChainSpecSource(string path, string name, bool isRelayChain, bool allowJsonRpc)
        {
            this.path = path;
            this.name = name;
            this.isRelayChain = isRelayChain;
            this.allowJsonRpc = allowJsonRpc;
        }
    }

    public class ChainSpecData
    {
        public readonly string name;
        public readonly string json;
        public readonly int size;
        public readonly bool isRelayChain;
        public readonly bool allowJsonRpc;

        public ChainSpecData()
        {
            name = string.Empty;
            json = string.Empty;
        }

        public ChainSpecData(string name, string json, int size, bool isRelayChain, bool allowJsonRpc)
        {
            this.name = name;
            this.json = json;
            this.size = size;
            this.isRelayChain = isRelayChain;
            this.allowJsonRpc = allowJsonRpc;
        }
    }

    public class ChainspecProfile
    {
        readonly List<ChainSpecSource> sourceList;

        public ReadOnlyCollection<ChainSpecSource> GetAllSource => sourceList.AsReadOnly();

        public ChainspecProfile(List<ChainSpecSource> sourceList)
        {
            this.sourceList = sourceList; 
        }

        public void AddSource(ChainSpecSource source)
        {
            sourceList.Add(source);
        }

        public void RemoveSource(string name)
        {
            var idx = sourceList.FindIndex((s) => s.name == name);
            if (idx < 0)
            {
                return;
            }

            sourceList.RemoveAt(idx);
        }

        public (bool, ChainSpecSource) GetSource(string name)
        {
            var idx = sourceList.FindIndex((s) => s.name == name);
            if (idx < 0)
            {
                return (false, new ChainSpecSource());
            }

            return (true, sourceList[idx]);
        }

        public List<ChainSpecData> ReadAllSource()
        {
            var len = sourceList.Count;
            var list = new List<ChainSpecData>(len);
            for (int i = 0; i < len; i++)
            {
                var spec = Read(sourceList[i]);
                list.Add(spec);
            }
            return list;
        }

        public async Task<List<ChainSpecData>> ReadAllSourceAsync()
        {
            var len = sourceList.Count;
            var list = new List<ChainSpecData>(len);
            var tasks = new Task[len];
            for (int i = 0; i < len; i++)
            {
                var src = sourceList[i];
                tasks[i] = Task.Run(async () =>
                {
                    var spec = await ReadAsync(src);
                    list.Add(spec);
                });
            }
            await Task.WhenAll(tasks);
            return list;
        }

        public (bool, ChainSpecData) ReadOneSource(string name)
        {
            var src = sourceList.Find((s) => s.name == name);
            if (src == null)
            {
                return (false, new ChainSpecData());
            }

            return (true, Read(src));
        }

        public async Task<(bool, ChainSpecData)> ReadOneSourceAsync(string name)
        {
            var src = sourceList.Find((s) => s.name == name);
            if (src == null)
            {
                return (false, new ChainSpecData());
            }

            var spec = await ReadAsync(src);
            return (true, spec);
        }

        public static ChainSpecData Read(ChainSpecSource source)
        {
            using var fs = File.OpenRead(source.path);
            var len = fs.Length;
            if (len > int.MaxValue)
            {
                throw new ExceedAllocationLimitException();
            }

            var buff = new byte[len];
            fs.Read(buff);
            var json = Encoding.UTF8.GetString(buff);
            return new ChainSpecData(source.name, json, (int)len, 
                source.isRelayChain, source.allowJsonRpc);
        }

        public static async Task<ChainSpecData> ReadAsync(ChainSpecSource source)
        {
            using var fs = File.OpenRead(source.path);
            var len = fs.Length;
            if (len > int.MaxValue)
            {
                throw new ExceedAllocationLimitException();
            }

            var buff = new byte[len];
            await fs.ReadAsync(buff);
            var json = Encoding.UTF8.GetString(buff);
            return new ChainSpecData(source.name, json, (int)len,
                source.isRelayChain, source.allowJsonRpc);
        }
    }
}