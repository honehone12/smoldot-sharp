using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SmoldotSharp
{
    public class DatabaseContent
    {
        public readonly string chainName;
        public readonly string content;
        public readonly int bytesLen;

        public DatabaseContent()
        {
            chainName = string.Empty;
            content = string.Empty;
            bytesLen = default;
        }

        public DatabaseContent(string chainName, string content)
        {
            this.chainName = chainName;
            this.content = content;
            bytesLen = Encoding.UTF8.GetByteCount(content);
        }

        public DatabaseContent(string chainName, string content, int bytesLen)
        {
            this.chainName = chainName;
            this.content = content;
            this.bytesLen = bytesLen;
        }
    }

    public class DatabaseContentStorage
    {
        readonly ISmoldotLogger logger;
        readonly string fileExtension;
        readonly string pathPrefix;
        readonly bool notStoreLocally;
        Obfuscator? obfuscator;

        public DatabaseContentStorage(ISmoldotLogger logger, DatabaseConfig config,
            Obfuscator? obfuscator = null)
        {
            fileExtension = config.localFileExtension;
            pathPrefix = config.localFileDir;
            notStoreLocally = !config.storeLocally;
            this.logger = logger;
            this.obfuscator = obfuscator;
        }

        public void SetObfuscator(Obfuscator obfuscator)
        {
            this.obfuscator = obfuscator;
        }

        public void Write(DatabaseContent databaseContent)
        {
            if (notStoreLocally)
            {
                return;
            }

            var path = DatabasePath(databaseContent.chainName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using var ws = File.OpenWrite(path);
            if (obfuscator != null)
            {
                var enc = obfuscator.Obfuscate(databaseContent.content);
                ws.Write(enc.Span);
            }
            else
            {
                var json = WritePlainProcess(databaseContent.content);   
                var span = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(json));
                ws.Write(span);
            }
        }

        string WritePlainProcess(string content)
        {
            var ja = new JArray
            {
                DatabaseConfig.MagicPhrase,
                content
            };
            return JsonConvert.SerializeObject(ja);
        }

        public async Task WriteAsync(DatabaseContent databaseContent)
        {
            if (notStoreLocally)
            {
                return;
            }

            var path = DatabasePath(databaseContent.chainName);
            using var ws = File.OpenWrite(path);
            if (obfuscator is null)
            {
                var json = WritePlainProcess(databaseContent.content);
                var mem = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(json));
                await ws.WriteAsync(mem);
            }
            else if (obfuscator is AsyncObfuscator ao)
            {
                var enc = await ao.ObfuscateAsync(databaseContent.content);
                await ws.WriteAsync(enc);
            }
            else
            {
                var enc = obfuscator.Obfuscate(databaseContent.content);
                await ws.WriteAsync(enc);
            }
        }

        void Archive(string path)
        {
            var archived = false;
            var numTry = 1;
            var ext = "." + fileExtension;
            var p = path.Replace(ext, "");
            while (!archived)
            {
                var newPath = string.Join("", p, "_archive" + numTry.ToString(), ext);
                if (File.Exists(newPath))
                {
                    numTry++;
                    continue;
                }

                File.Move(path, newPath);
                archived = true;
            }
        }

        public DatabaseContent Read(string chainName)
        {
            if (notStoreLocally)
            {
                return new DatabaseContent();
            }

            var path = DatabasePath(chainName);
            if (!File.Exists(path))
            {
                return new DatabaseContent();
            }

            using var fs = File.OpenRead(path);
            var len = fs.Length;
            if (len >= int.MaxValue)
            {
                logger.Log(SmoldotLogLevel.Info, $"Archiving database content. name: {chainName}");
                fs.Close();
                Archive(path);
                return new DatabaseContent();
            }

            var buff = new byte[len];
            fs.Read(buff);
            if (obfuscator != null)
            {
                var content = obfuscator.Deobfuscate(buff);
                return new DatabaseContent(chainName, content);
            }
            else
            {
                return ReadPlainProcess(buff, chainName);
            }
        }

        DatabaseContent ReadPlainProcess(byte[] buff, string chainName)
        {
            var ja = JsonConvert.DeserializeObject<JArray>(Encoding.UTF8.GetString(buff));
            if (ja != null && ja[0].ToString().Equals(DatabaseConfig.MagicPhrase))
            {
                return new DatabaseContent(chainName, ja[1].ToString());
            }
            else
            {
                logger.Log(SmoldotLogLevel.Warn, 
                    "Magic number was not expected, returns empty content.");
                return new DatabaseContent();
            }
        }

        public async Task<DatabaseContent> ReadAsync(string chainName)
        {
            if (notStoreLocally)
            {
                return new DatabaseContent();
            }

            var path = DatabasePath(chainName);
            if (!File.Exists(path))
            {
                return new DatabaseContent();
            }

            using var fs = File.OpenRead(path);
            var len = fs.Length;
            if (len >= int.MaxValue)
            {
                logger.Log(SmoldotLogLevel.Info, $"Archiving database content. name: {chainName}");
                fs.Close();
                await Task.Run(() => Archive(path));
                return new DatabaseContent();
            }

            var buff = new byte[len];
            await fs.ReadAsync(buff);
            if (obfuscator is null)
            {
                return ReadPlainProcess(buff, chainName);

            }
            else if (obfuscator is AsyncObfuscator ao)
            {
                var content = await ao.DeobfuscateAsync(buff);
                return new DatabaseContent(chainName, content);
            }
            else
            {
                var content = obfuscator.Deobfuscate(buff);
                return new DatabaseContent(chainName, content);
            }
        }

        string DatabasePath(string chainName)
        {
            var fileName = string.Join(".", chainName, fileExtension);
            return Path.Combine(pathPrefix, fileName);
        }
    }
}