using SmoldotSharp;
using System.Diagnostics;

namespace SmoldotSharpTest
{
    internal class Test
    {
        static async Task ChainSpecTest()
        {
            var src = new ChainSpecSource("test.json", "moe", false, true);
            var data = ChainspecProfile.Read(src);
            Debug.Assert(data.json.Equals("Moemoe Kyun"));

            var ascdata = await ChainspecProfile.ReadAsync(src);
            Debug.Assert(ascdata.json.Equals("Moemoe Kyun"));

            var list = new List<ChainSpecSource> { src };
            var specProfile = new ChainspecProfile(list);
            var specList = await specProfile.ReadAllSourceAsync();
            Debug.Assert(specList[0].json.Equals("Moemoe Kyun"));
            specList = specProfile.ReadAllSource();
            Debug.Assert(specList[0].json.Equals("Moemoe Kyun"));
            (var ok, var oneSpec) = await specProfile.ReadOneSourceAsync("moe");
            Debug.Assert(ok);
            Debug.Assert(oneSpec.json.Equals("Moemoe Kyun"));
            (ok, oneSpec) = specProfile.ReadOneSource("moe");
            Debug.Assert(ok);
            Debug.Assert(oneSpec.json.Equals("Moemoe Kyun"));
            Console.WriteLine("ok");
        }

        static async Task DatabaseTest()
        {
            var logger = new SmoldotDevLogger(SmoldotLogLevel.Warn);
            var (aesKey, hmacKey) 
                = AesHmacKeys.GenerateKeysSource(AesKeySize.Size32, HmacKeySize.Size64);
            var keys = new AesHmacKeys(aesKey, hmacKey);
            var dbStorage = new DatabaseContentStorage(logger,
                new DatabaseConfig(true, "dbc", ""), 
                new AesHmac(logger, keys, HmacFunc.HmacSha512));
            var dbContent = new DatabaseContent("moe", "Moemoe Kyun");
            dbStorage.Write(dbContent);
            Debug.Assert(dbStorage.Read("moe").content.Equals("Moemoe Kyun"));
            await dbStorage.WriteAsync(dbContent);
            var r = await dbStorage.ReadAsync("moe");
            Debug.Assert(r.content.Equals("Moemoe Kyun"));
            Console.WriteLine("ok");

            var plainStorage = new DatabaseContentStorage(logger,
                new DatabaseConfig(true, "pln", ""));
            var plainContent = new DatabaseContent("moe", "Moemoe Kyun");
            plainStorage.Write(plainContent);
            Debug.Assert(plainStorage.Read("moe").content.Equals("Moemoe Kyun"));
            await plainStorage.WriteAsync(plainContent);
            var p = await plainStorage.ReadAsync("moe");
            Debug.Assert(p.content.Equals("Moemoe Kyun"));
            Console.WriteLine("ok");
        }

        static void ConverterTest()
        {
            var i = SmoldotConfig.ConvertCpuRateLimit(1.0);
            var ui = (uint)i;
            Debug.Assert(ui == uint.MaxValue);
            Console.WriteLine("ok");
        }

        class TestClass
        {
            public string str = string.Empty;
        }

        struct TestStruct
        {
            public uint integer;
        }

        static void TestBoxedObject()
        {
            var i = 10u;
            var box = new BoxedObject(i);
            var (ok, newI) = box.UnboxAsUnmanaged<uint>();
            Debug.Assert(ok);
            Debug.Assert(newI == 10u);

            var s = "moemoe";
            box = new BoxedObject(s);
            (ok, var newS) = box.UnboxAsString();
            Debug.Assert(ok);
            Debug.Assert(newS.Equals("moemoe"));

            var f = new TestStruct { integer = 10u };
            box = new BoxedObject(f);
            (ok, var newF) = box.UnboxAsStruct<TestStruct>();
            Debug.Assert(ok);
            Debug.Assert(newF.integer == 10u);

            var c = new TestClass { str = "moemoe" };
            box = new BoxedObject(c);
            (ok, var newC) = box.UnboxAsClass<TestClass>();
            Debug.Assert(ok);
            Debug.Assert(newC != null);
            Debug.Assert(newC.str.Equals("moemoe"));
        }

        static async Task Main()
        {
            TestBoxedObject();
            ConverterTest();
            await Task.WhenAll(ChainSpecTest(), DatabaseTest());
        }
    }
}