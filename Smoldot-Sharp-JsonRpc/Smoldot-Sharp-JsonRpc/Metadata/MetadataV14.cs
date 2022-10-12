using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using ScaleSharpLight;

namespace SmoldotSharp.JsonRpc.MetadataInternal
{
    public enum PrimitivesV14 : byte
    {
        Bool,
        Char,
        Str,
        U8,
        U16,
        U32,
        U64,
        U128,
        U256,
        I8,
        I16,
        I32,
        I64,
        I128,
        I256
    }

    public enum TypeDefV14 : byte
    {
        Composit,
        Variant,
        Sequence,
        Array,
        Tuple,
        Primitive,
        Compact,
        BitSequence,
    }

    public class FieldDataV14
    {
        public (Prefix op, string name) name;
        public uint typeId;
        public (Prefix op, string name) typeName;
        public string[] docs = Array.Empty<string>();
    }

    public class TypeDictionaryV14<T> : Dictionary<
        uint,
        (
            string path,
            (string Tname, (Prefix op, uint typeId) id)[] genericsParams,
            T typeDef,
            string[] docs
        )
    >
    {
        public TypeDictionaryV14() : base() { }
    }

    public class TypeRegistryV14
    {
        public readonly Dictionary<uint, TypeDefV14> idTable = new Dictionary<uint, TypeDefV14>();
        public readonly TypeDictionaryV14<FieldDataV14[]> compositDictionary = new TypeDictionaryV14<FieldDataV14[]>();
        public readonly TypeDictionaryV14<
            (
                string name,
                FieldDataV14[] fields,
                byte index,
                string[] docs
            )[]
        > variantDictionary = new TypeDictionaryV14<(string, FieldDataV14[], byte, string[])[]>();
        public readonly TypeDictionaryV14<uint> sequenceDictionary = new TypeDictionaryV14<uint>();
        public readonly TypeDictionaryV14<(uint len, uint typeId)> arrayDictionary = new TypeDictionaryV14<(uint, uint)>();
        public readonly TypeDictionaryV14<uint[]> tupleDictionary = new TypeDictionaryV14<uint[]>();
        public readonly TypeDictionaryV14<PrimitivesV14> primitiveDictionary = new TypeDictionaryV14<PrimitivesV14>();
        public readonly TypeDictionaryV14<uint> compactDictionary = new TypeDictionaryV14<uint>();
        public readonly TypeDictionaryV14<(uint storeType, uint orderType)> bitSequenceDictionary = new TypeDictionaryV14<(uint, uint)>();

        #region print
        string SPrintIdTable()
        {
            var sb = new StringBuilder();
            sb.AppendLine("ID Table");
            foreach (var item in idTable)
            {
                sb.Append($"[{item.Key}]: {item.Value} | ");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintComposit()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Conposit");
            foreach (var item in compositDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                for (int i = 0; i < typeDef.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append("name: ");
                    var op = typeDef[i].name.op.IsNone() ? "None" : "Some";
                    sb.Append($"{op}/{typeDef[i].name.name} | ");
                    sb.Append($"typeId: {typeDef[i].typeId} | ");
                    sb.Append("typeName: ");
                    var top = typeDef[i].typeName.op.IsNone() ? "None" : "Some";
                    sb.Append($"{top}/{typeDef[i].typeName.name} | ");
                    sb.Append($"docs: ");
                    for (int j = 0; j < typeDef[i].docs.Length; j++)
                    {
                        sb.Append($"(({j})) ");
                        sb.Append($"{typeDef[i].docs[j]} ");
                    }
                }
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintVariant()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Variant");
            foreach (var item in variantDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                for (int i = 0; i < typeDef.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"name: {typeDef[i].name} | ");
                    sb.Append($"fields: ");
                    for (int j = 0; j < typeDef[i].fields.Length; j++)
                    {
                        sb.Append($"(({j})) ");
                        sb.Append("name: ");
                        var op = typeDef[i].fields[j].name.op.IsNone() ? "None" : "Some";
                        sb.Append($"{op}/{typeDef[i].fields[j].name.name} | ");
                        sb.Append($"typeId: {typeDef[i].fields[j].typeId} | ");
                        sb.Append("typeName: ");
                        var top = typeDef[i].fields[j].typeName.op.IsNone() ? "None" : "Some";
                        sb.Append($"{top}/{typeDef[i].fields[j].typeName.name} | ");
                        sb.Append($"docs: ");
                        for (int k = 0; k < typeDef[i].fields[j].docs.Length; k++)
                        {
                            sb.Append($"((({k}))) ");
                            sb.Append($"{typeDef[i].fields[j].docs[k]} ");
                        }
                    }
                    sb.Append($"index: {typeDef[i].index} | ");
                    sb.Append($"docs: ");
                    for (int j = 0; j < typeDef[i].docs.Length; j++)
                    {
                        sb.Append($"(({j})) ");
                        sb.Append($"{typeDef[i].docs[j]} ");
                    }
                }
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintSequence()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Sequence");
            foreach (var item in sequenceDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                sb.Append($"id: {typeDef}");
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintArray()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Array");
            foreach (var item in arrayDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                sb.Append($"len: {typeDef.len} id: {typeDef.typeId}");
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintTuple()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Tuple");
            foreach (var item in tupleDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                for (int i = 0; i < typeDef.Length; i++)
                {
                    sb.Append($"({i}) id: {typeDef[i]} ");
                }
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintPrimitive()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Primitive");
            foreach (var item in primitiveDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                sb.Append($"{typeDef}");
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintCompact()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Compact");
            foreach (var item in compactDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                sb.Append($"id: {typeDef}");
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintBitSeq()
        {
            var sb = new StringBuilder();
            sb.AppendLine("BitSequence");
            foreach (var item in bitSequenceDictionary)
            {
                sb.AppendLine("--------------------");
                var (path, genericsParams, typeDef, docs) = item.Value;
                sb.Append($"[id]: {item.Key} ");
                sb.Append($"[path]: {path}");
                sb.Append("\n");
                sb.Append("[genericsParams]: ");
                for (int i = 0; i < genericsParams.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"<T>name {genericsParams[i].Tname} | ");
                    var op = genericsParams[i].id.op.IsNone() ? "None" : "Some";
                    sb.Append($"<T>id {op}/{genericsParams[i].id.typeId} ");
                }
                sb.Append("\n");
                sb.Append("[typeDef]: ");
                sb.Append($"storeType: {typeDef.storeType} orderType: {typeDef.orderType}");
                sb.Append("\n");
                sb.Append($"[docs]: ");
                for (int i = 0; i < docs.Length; i++)
                {
                    sb.Append($"({i}) ");
                    sb.Append($"{docs[i]} ");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }
        #endregion

        public List<string> String()
        {
            return new List<string>
                {
                    SPrintIdTable(),
                    SPrintComposit(),
                    SPrintVariant(),
                    SPrintSequence(),
                    SPrintArray(),
                    SPrintTuple(),
                    SPrintPrimitive(),
                    SPrintCompact(),
                    SPrintBitSeq()
                };
        }
    }

    public enum StorageEntryTypeV14 : byte
    {
        Plain,
        Map,
    }

    public enum StorageEntryModifierV14 : byte
    {
        Optional,
        Default
    }

    public enum StorageHasherV14 : byte
    {
        Blake2_128,
        Blake2_256,
        Blake2_128Concat,
        Twox128,
        Twox256,
        Twox64Concat,
        Identity
    }

    public class StorageEntryV14
    {
        public string storageName = string.Empty;
        public StorageEntryModifierV14 storageMod;
        public StorageEntryTypeV14 storageType;
        public uint plainTypeId;
        public (StorageHasherV14[] hashers, uint keyType, uint valueType) mapPrpos =
            (Array.Empty<StorageHasherV14>(), default(uint), default(uint));
        public byte[] storageDefault = Array.Empty<byte>();
    }

    public class PalletMetadataV14
    {
        public readonly Dictionary<string, byte> indexTable = new Dictionary<string, byte>();
        public readonly Dictionary<byte, (string prefix, StorageEntryV14[] entries)> storageDictionary = new Dictionary<byte, (string, StorageEntryV14[])>();
        public readonly Dictionary<byte, (Prefix op, uint typeId)> callDictionary = new Dictionary<byte, (Prefix, uint)>();
        public readonly Dictionary<byte, (Prefix op, uint typeId)> eventDictionary = new Dictionary<byte, (Prefix, uint)>();
        public readonly Dictionary<byte,
            (
                string name,
                uint typeId,
                byte[] defaultValue,
                string[] docs
            )[]
        > constantsDictionary = new Dictionary<byte, (string, uint, byte[], string[])[]>();
        public readonly Dictionary<byte, (Prefix op, uint typeId)> errorDictionary = new Dictionary<byte, (Prefix, uint)>();

        #region print
        string SPrintIndexTable()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Index Table");
            foreach (var item in indexTable)
            {
                sb.Append($"pallet name: {item.Key} , index: {item.Value} | ");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintStorageEntries()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Storage Entries");
            foreach (var item in storageDictionary)
            {
                sb.AppendLine($"index: {item.Key}");
                var group = item.Value;
                sb.AppendLine($"storage prefix: {group.prefix}");
                var entries = group.entries;
                for (int i = 0; i < entries.Length; i++)
                {
                    sb.AppendLine("--------------------");
                    sb.Append($"name: {entries[i].storageName} | ");
                    sb.Append($"mod: {entries[i].storageMod} | ");
                    sb.Append($"type: {entries[i].storageType} | ");
                    switch (entries[i].storageType)
                    {
                        case StorageEntryTypeV14.Plain:
                            sb.Append($"type id: {entries[i].plainTypeId} | ");
                            break;
                        case StorageEntryTypeV14.Map:
                            sb.Append("hashers : ");
                            for (int j = 0; j < entries[i].mapPrpos.hashers.Length; j++)
                            {
                                sb.Append($"{entries[i].mapPrpos.hashers[j]}, ");
                            }
                            sb.Append("| ");
                            sb.Append($"key type: {entries[i].mapPrpos.keyType} | ");
                            sb.Append($"value type: {entries[i].mapPrpos.valueType} | ");
                            break;
                        default:
                            sb.Append("!!unknown!! | ");
                            break;
                    }
                    sb.Append($"default : 0x");
                    for (int j = 0; j < entries[i].storageDefault.Length; j++)
                    {
                        sb.Append($"{entries[i].storageDefault[j]}");
                    }
                    sb.Append("\n");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintCall()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Call");
            foreach (var item in callDictionary)
            {
                sb.Append($"index: {item.Key}, ");
                var call = item.Value;
                var op = call.op.IsNone() ? "None" : "Some";
                sb.Append($"{op}/{call.typeId} | ");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintEvent()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Event");
            foreach (var item in eventDictionary)
            {
                sb.Append($"index: {item.Key}, ");
                var eventitem = item.Value;
                var op = eventitem.op.IsNone() ? "None" : "Some";
                sb.Append($"{op}/{eventitem.typeId} | ");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintConstants()
        {
            var sb = new StringBuilder();
            sb.Append("Constants");
            foreach (var item in constantsDictionary)
            {
                sb.AppendLine($"index: {item.Key}, ");
                var cons = item.Value;
                for (int i = 0; i < cons.Length; i++)
                {
                    sb.AppendLine("--------------------");
                    sb.Append($"name: {cons[i].name} | ");
                    sb.Append($"type id: {cons[i].typeId} | ");
                    sb.Append($"default : 0x");
                    for (int j = 0; j < cons[i].defaultValue.Length; j++)
                    {
                        sb.Append($"{cons[i].defaultValue[j]}");
                    }
                    sb.Append($"docs: ");
                    for (int j = 0; j < cons[i].docs.Length; j++)
                    {
                        sb.Append($"({j}) {cons[i].docs[j]}");
                    }
                    sb.Append("\n");
                }
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        string SPrintError()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Error");
            foreach (var item in errorDictionary)
            {
                sb.Append($"index: {item.Key}, ");
                var error = item.Value;
                var op = error.op.IsNone() ? "None" : "Some";
                sb.Append($"{op}/{error.typeId} | ");
            }
            sb.Append("\n");
            return sb.ToString();
        }
        #endregion

        public List<string> String()
        {
            return new List<string>
                {
                    SPrintIndexTable(),
                    SPrintStorageEntries(),
                    SPrintCall(),
                    SPrintEvent(),
                    SPrintConstants(),
                    SPrintError(),
                };
        }
    }

    public class ExtrinsicMetadataV14
    {
        public uint extrinsicTypeId;
        public byte version;
        public List<(
            string signedIdentifier,
            uint signedExtensionTypeId,
            uint additionalTypeId
        )> signedExtensionsList = new List<(string, uint, uint)>();

        string SPrint()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Extrinsics");
            sb.AppendLine($"extrinsic typeId: {extrinsicTypeId}");
            sb.AppendLine($"extrinsic version: {version}");
            sb.AppendLine("Signed Extensions");
            for (int i = 0; i < signedExtensionsList.Count; i++)
            {
                sb.AppendLine("--------------------");
                sb.Append(
                    $"signedIdentifier: {signedExtensionsList[i].signedIdentifier} | ");
                sb.Append(
                    $"signedExtensionTypeId: {signedExtensionsList[i].signedExtensionTypeId} | ");
                sb.Append(
                    $"additionalTypeId: {signedExtensionsList[i].additionalTypeId} | ");
                sb.Append("\n");
            }
            sb.Append("\n");
            return sb.ToString();
        }

        public List<string> String()
        {
            return new List<string>
            {
                SPrint()
            };
        }
    }

    public class MetaV14 : Meta
    {
        public readonly TypeRegistryV14 typeRegistry = new TypeRegistryV14();
        public readonly PalletMetadataV14 palletMetada = new PalletMetadataV14();
        public readonly ExtrinsicMetadataV14 extrinsicMetadata = new ExtrinsicMetadataV14();
        public uint runtimeTypeId;

        public override MetaQuery GetQuery => new MetaQueryV14(this);

        public override List<string> String()
        {
            var list = new List<string>();
            list.AddRange(typeRegistry.String());
            list.AddRange(palletMetada.String());
            list.AddRange(extrinsicMetadata.String());
            list.Add($"runtime typeId {runtimeTypeId}");
            return list;
        }
    }

    public class MetaV14Decoder : MetaDecoder
    {
        // https://github.com/paritytech/frame-metadata/blob/main/frame-metadata/src/v14.rs

        public override Meta? Decode(Span<byte> bytes, MetadataStoreSetting setting)
        {
            var pos = 0;
            pos += bytes.FixedDecodeU32(out var magic);
            var version = bytes[pos++];
            if (magic != Metadata.MagicNumber || version != 14)
            {
                return null;
            }

            var meta = new MetaV14();
            if (!TryDecodeTypeRegistry(bytes, setting, meta.typeRegistry, ref pos))
            {
                return null;
            }

            if (!TryDecodePalletMetadata(bytes, setting, meta.palletMetada, ref pos))
            {
                return null;
            }

            if (!TryDecodeExtrinsicMetadata(bytes, setting, meta.extrinsicMetadata, ref pos))
            {
                return null;
            }

            if (!bytes[pos..].TryCompactDecode(out var tmpRTType, out var r))
            {
                return null;
            }

            meta.runtimeTypeId = (uint)tmpRTType;
            Debug.Assert(pos + r == bytes.Length);
            return meta;
        }

        bool TryDecodeTypeRegistry(Span<byte> buff, MetadataStoreSetting setting,
            TypeRegistryV14 target, ref int pos)
        {
            #region root vec
            if (!buff[pos..].TryDecodeVectorPrefix(out var tmpTypeLen, out var r))
            {
                return false;
            }

            pos += r;
            var typeLen = (int)tmpTypeLen;
            for (int i = 0; i < typeLen; i++)
            {
                uint rootId;
                #region root id
                if (!buff[pos..].TryCompactDecode(out var tmpRootId, out r))
                {
                    return false;
                }
                rootId = (uint)tmpRootId;
                pos += r;
                #endregion

                string combinedPath;
                #region path
                if (!buff[pos..].TryDecodeVectorPrefix(out var tmpPathLen, out r))
                {
                    return false;
                }
                var pathLen = (int)tmpPathLen;
                var path = new string[pathLen];
                pos += r;
                for (int j = 0; j < pathLen; j++)
                {
                    if (!buff[pos..].TryDecodeVecU8(out var pathBytes, out r))
                    {
                        return false;
                    }
                    path[j] = Encoding.UTF8.GetString(pathBytes);
                    pos += r;
                }
                combinedPath = string.Join("::", path);
                #endregion

                (string, (Prefix, uint))[] genericsTypeParams;
                #region type params
                if (!buff[pos..].TryDecodeVectorPrefix(out var tmpParamLen, out r))
                {
                    return false;
                }
                var paramLen = (int)tmpParamLen;
                genericsTypeParams = new (string, (Prefix, uint))[paramLen];
                pos += r;
                for (int j = 0; j < paramLen; j++)
                {
                    if (!buff[pos..].TryDecodeVecU8(out var paramNameBytes, out r))
                    {
                        return false;
                    }
                    genericsTypeParams[j].Item1 = Encoding.UTF8.GetString(paramNameBytes);
                    pos += r;

                    if (!buff[pos..].TryCompactDecodeOption(
                        out (Prefix op, BigInteger paramId) op, out r))
                    {
                        return false;
                    }
                    genericsTypeParams[j].Item2 = (op.op, (uint)op.paramId);
                    pos += r;
                }
                #endregion

                var typeDef = (TypeDefV14)buff[pos++];
                var compositDef = Array.Empty<FieldDataV14>();
                var variantDef = Array.Empty<(string, FieldDataV14[], byte, string[])>();
                var sequenceDef = default(uint);
                var arrayDef = default((uint, uint));
                var tupleDef = Array.Empty<uint>();
                var primDef = default(PrimitivesV14);
                var compactDef = default(uint);
                var bitDef = default((uint, uint));
                #region type def
                switch (typeDef)
                {
                    #region composit
                    case TypeDefV14.Composit:
                        if (!buff[pos..].TryDecodeVectorPrefix(out var tmpFieldLen, out r))
                        {
                            return false;
                        }
                        pos += r;
                        var fieldLen = (int)tmpFieldLen;
                        compositDef = new FieldDataV14[fieldLen];
                        for (int j = 0; j < fieldLen; j++)
                        {
                            // field name
                            if (!buff[pos..].TryDecodePrefix(out var fNameOp))
                            {
                                return false;
                            }
                            pos++;
                            var fName = string.Empty;
                            if (!fNameOp.IsNone())
                            {
                                if (!buff[pos..].TryDecodeVecU8(out var fNameBytes, out r))
                                {
                                    return false;
                                }
                                fName = Encoding.UTF8.GetString(fNameBytes);
                                pos += r;
                            }

                            // field id
                            if (!buff[pos..].TryCompactDecode(out var tmpFieldId, out r))
                            {
                                return false;
                            }
                            var fieldId = (uint)tmpFieldId;
                            pos += r;

                            // field type name
                            if (!buff[pos..].TryDecodePrefix(out var fTypeNameOp))
                            {
                                return false;
                            }
                            pos++;
                            var fTypeName = string.Empty;
                            if (!fTypeNameOp.IsNone())
                            {
                                if (!buff[pos..].TryDecodeVecU8(out var fTypeNameBytes, out r))
                                {
                                    return false;
                                }
                                fTypeName = Encoding.UTF8.GetString(fTypeNameBytes);
                                pos += r;
                            }

                            // field docs
                            if (!buff[pos..].TryDecodeVectorPrefix(out var fDocsLen, out r))
                            {
                                return false;
                            }
                            var iFDocsLen = (int)fDocsLen;
                            var fDocs = setting.storeDocs ? new string[iFDocsLen] : Array.Empty<string>();
                            pos += r;
                            for (int k = 0; k < iFDocsLen; k++)
                            {
                                if (!buff[pos..].TryDecodeVecU8(out var fDocsBytes, out r))
                                {
                                    return false;
                                }

                                if (setting.storeDocs)
                                {
                                    fDocs[k] = Encoding.UTF8.GetString(fDocsBytes);
                                }
                                pos += r;
                            }

                            compositDef[j] = new FieldDataV14
                            {
                                name = (fNameOp, fName),
                                typeId = fieldId,
                                typeName = (fTypeNameOp, fTypeName),
                                docs = fDocs
                            };
                        }
                        break;
                    #endregion
                    #region variant
                    case TypeDefV14.Variant:
                        if (!buff[pos..].TryDecodeVectorPrefix(out var tmpVariantLen, out r))
                        {
                            return false;
                        }
                        pos += r;
                        var variantLen = (int)tmpVariantLen;
                        variantDef = new (string, FieldDataV14[], byte, string[])[variantLen];
                        for (int j = 0; j < variantLen; j++)
                        {
                            // variant name
                            if (!buff[pos..].TryDecodeVecU8(out var tmpVariantNameBytes, out r))
                            {
                                return false;
                            }
                            var variantName = Encoding.UTF8.GetString(tmpVariantNameBytes);
                            pos += r;

                            // variant fields
                            if (!buff[pos..].TryDecodeVectorPrefix(out var tmpVFieldLen, out r))
                            {
                                return false;
                            }
                            pos += r;
                            var vFieldLen = (int)tmpVFieldLen;
                            var variantFields = new FieldDataV14[vFieldLen];
                            for (int k = 0; k < vFieldLen; k++)
                            {
                                // field name
                                if (!buff[pos..].TryDecodePrefix(out var fNameOp))
                                {
                                    return false;
                                }
                                pos++;
                                var fName = string.Empty;
                                if (!fNameOp.IsNone())
                                {
                                    if (!buff[pos..].TryDecodeVecU8(out var fNameBytes, out r))
                                    {
                                        return false;
                                    }
                                    fName = Encoding.UTF8.GetString(fNameBytes);
                                    pos += r;
                                }

                                // field id
                                if (!buff[pos..].TryCompactDecode(out var fieldId, out r))
                                {
                                    return false;
                                }
                                pos += r;

                                // field type name
                                if (!buff[pos..].TryDecodePrefix(out var fTypeNameOp))
                                {
                                    return false;
                                }
                                pos++;
                                var fTypeName = string.Empty;
                                if (!fTypeNameOp.IsNone())
                                {
                                    if (!buff[pos..].TryDecodeVecU8(out var fTypeNameBytes, out r))
                                    {
                                        return false;
                                    }
                                    fTypeName = Encoding.UTF8.GetString(fTypeNameBytes);
                                    pos += r;
                                }

                                // field docs
                                if (!buff[pos..].TryDecodeVectorPrefix(out var tmpFDocsLen, out r))
                                {
                                    return false;
                                }
                                var fDocsLen = (int)tmpFDocsLen;
                                var fDocs = setting.storeDocs ? new string[fDocsLen] : Array.Empty<string>();
                                pos += r;
                                for (int l = 0; l < fDocsLen; l++)
                                {
                                    if (!buff[pos..].TryDecodeVecU8(out var fDocsBytes, out r))
                                    {
                                        return false;
                                    }

                                    if (setting.storeDocs)
                                    {
                                        fDocs[l] = Encoding.UTF8.GetString(fDocsBytes);
                                    }
                                    pos += r;
                                }

                                variantFields[k] = new FieldDataV14
                                {
                                    name = (fNameOp, fName),
                                    typeId = (uint)fieldId,
                                    typeName = (fTypeNameOp, fTypeName),
                                    docs = fDocs
                                };
                            }

                            // variant index
                            var idx = buff[pos++];

                            // variant docs
                            if (!buff[pos..].TryDecodeVectorPrefix(out var tmpVDocsLen, out r))
                            {
                                return false;
                            }
                            var vDocsLen = (int)tmpVDocsLen;
                            var vDocs = setting.storeDocs ? new string[vDocsLen] : Array.Empty<string>();
                            pos += r;
                            for (int k = 0; k < vDocsLen; k++)
                            {
                                if (!buff[pos..].TryDecodeVecU8(out var vDocsBytes, out r))
                                {
                                    return false;
                                }
                                if (setting.storeDocs)
                                {
                                    vDocs[k] = Encoding.UTF8.GetString(vDocsBytes);
                                }
                                pos += r;
                            }

                            variantDef[j] = (variantName, variantFields, idx, vDocs);
                        }
                        break;
                    #endregion
                    #region sequence
                    case TypeDefV14.Sequence:
                        if (!buff[pos..].TryCompactDecode(out var seqId, out r))
                        {
                            return false;
                        }

                        sequenceDef = (uint)seqId;
                        pos += r;
                        break;
                    #endregion
                    #region array
                    case TypeDefV14.Array:
                        pos += buff[pos..].FixedDecodeU32(out var arrayLen);
                        if (!buff[pos..].TryCompactDecode(out var arrayId, out r))
                        {
                            return false;
                        }

                        arrayDef = (arrayLen, (uint)arrayId);
                        pos += r;
                        break;
                    #endregion
                    #region tuple
                    case TypeDefV14.Tuple:
                        if (!buff[pos..].TryDecodeVectorPrefix(out var tmpTupleFieldLen, out r))
                        {
                            return false;
                        }

                        pos += r;
                        var tupleFieldLen = (int)tmpTupleFieldLen;
                        tupleDef = new uint[tupleFieldLen];
                        if (tupleFieldLen != 0)
                        {
                            var tmpTFields = new BigInteger[tupleFieldLen];
                            if (!buff[pos..].TryMultipleCompactDecode(tmpTFields, out _, out r))
                            {
                                return false;
                            }

                            pos += r;
                            for (int j = 0; j < tupleFieldLen; j++)
                            {
                                tupleDef[j] = (uint)tmpTFields[j];
                            }
                        }
                        break;
                    #endregion
                    #region primitive
                    case TypeDefV14.Primitive:
                        primDef = (PrimitivesV14)buff[pos++];
                        break;
                    #endregion
                    #region compact
                    case TypeDefV14.Compact:
                        if (!buff[pos..].TryCompactDecode(out var tmpCompactId, out r))
                        {
                            return false;
                        }

                        compactDef = (uint)tmpCompactId;
                        pos += r;
                        break;
                    #endregion
                    #region bit sequence
                    case TypeDefV14.BitSequence:
                        if (!buff[pos..].TryCompactDecode(out var bStore, out r))
                        {
                            return false;
                        }

                        pos += r;
                        if (!buff[pos..].TryCompactDecode(out var bOrder, out r))
                        {
                            return false;
                        }

                        bitDef = ((uint)bStore, (uint)bOrder);
                        pos += r;
                        break;
                    #endregion
                    default:
                        return false;
                }
                #endregion

                string[] docs;
                #region docs
                if (!buff[pos..].TryDecodeVectorPrefix(out var docsLen, out r))
                {
                    return false;
                }

                var iDocsLen = (int)docsLen;
                docs = setting.storeDocs ? new string[iDocsLen] : Array.Empty<string>(); ;
                pos += r;
                for (int j = 0; j < iDocsLen; j++)
                {
                    if (!buff[pos..].TryDecodeVecU8(out var docsBytes, out r))
                    {
                        return false;
                    }

                    if (setting.storeDocs)
                    {
                        docs[j] = Encoding.UTF8.GetString(docsBytes);
                    }
                    pos += r;
                }
                #endregion

                switch (typeDef)
                {
                    case TypeDefV14.Composit:
                        target.compositDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, compositDef, docs)
                        );
                        break;
                    case TypeDefV14.Variant:
                        target.variantDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, variantDef, docs)
                        );
                        break;
                    case TypeDefV14.Sequence:
                        target.sequenceDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, sequenceDef, docs)
                        );
                        break;
                    case TypeDefV14.Array:
                        target.arrayDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, arrayDef, docs)
                        );
                        break;
                    case TypeDefV14.Tuple:
                        target.tupleDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, tupleDef, docs)
                        );
                        break;
                    case TypeDefV14.Primitive:
                        target.primitiveDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, primDef, docs)
                        );
                        break;
                    case TypeDefV14.Compact:
                        target.compactDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, compactDef, docs)
                        );
                        break;
                    case TypeDefV14.BitSequence:
                        target.bitSequenceDictionary.Add(
                            rootId, (combinedPath, genericsTypeParams, bitDef, docs)
                        );
                        break;
                    default:
                        return false;
                }
                target.idTable.Add(rootId, typeDef);
            }
            #endregion
            return true;
        }

        bool TryDecodePalletMetadata(Span<byte> buff, MetadataStoreSetting setting,
            PalletMetadataV14 target, ref int pos)
        {
            #region root vec
            if (!buff[pos..].TryDecodeVectorPrefix(out var tmpPalletLen, out var r))
            {
                return false;
            }

            pos += r;
            var palletLen = (int)tmpPalletLen;
            for (int i = 0; i < palletLen; i++)
            {
                // pallet name
                if (!buff[pos..].TryDecodeVecU8(out var palletNameBytes, out r))
                {
                    return false;
                }

                pos += r;
                var palletName = Encoding.UTF8.GetString(palletNameBytes);

                #region storage metadata
                if (!buff[pos..].TryDecodePrefix(out var opStorage))
                {
                    return false;
                }

                var storageEntries = Array.Empty<StorageEntryV14>();
                var storagePfx = string.Empty;
                ++pos;
                if (!opStorage.IsNone())
                {
                    // storage prefix
                    if (!buff[pos..].TryDecodeVecU8(out var storagePfxBytes, out r))
                    {
                        return false;
                    }

                    pos += r;
                    storagePfx = Encoding.UTF8.GetString(storagePfxBytes);

                    // storage entry
                    if (!buff[pos..].TryDecodeVectorPrefix(out var tmpEntryLen, out r))
                    {
                        return false;
                    }

                    pos += r;
                    var entryLen = (int)tmpEntryLen;
                    storageEntries = new StorageEntryV14[entryLen];
                    for (int j = 0; j < entryLen; j++)
                    {
                        storageEntries[j] = new StorageEntryV14();

                        if (!buff[pos..].TryDecodeVecU8(out var storageNameBytes, out r))
                        {
                            return false;
                        }

                        pos += r;
                        storageEntries[j].storageName = Encoding.UTF8.GetString(storageNameBytes);
                        storageEntries[j].storageMod = (StorageEntryModifierV14)buff[pos++];
                        storageEntries[j].storageType = (StorageEntryTypeV14)buff[pos++];
                        switch (storageEntries[j].storageType)
                        {
                            case StorageEntryTypeV14.Plain:
                                if (!buff[pos..].TryCompactDecode(out var tmpPlainType, out r))
                                {
                                    return false;
                                }

                                pos += r;
                                storageEntries[j].plainTypeId = (uint)tmpPlainType;
                                break;
                            case StorageEntryTypeV14.Map:
                                if (!buff[pos..].TryDecodeVectorPrefix(out var tmpHashersLen, out r))
                                {
                                    return false;
                                }

                                pos += r;
                                var hashersLen = (int)tmpHashersLen;
                                storageEntries[j].mapPrpos.hashers = new StorageHasherV14[hashersLen];
                                for (int k = 0; k < hashersLen; k++)
                                {
                                    storageEntries[j].mapPrpos.hashers[k] = (StorageHasherV14)buff[pos++];
                                }
                                if (!buff[pos..].TryCompactDecode(out var tmpKeyType, out r))
                                {
                                    return false;
                                }

                                pos += r;
                                storageEntries[j].mapPrpos.keyType = (uint)tmpKeyType;
                                if (!buff[pos..].TryCompactDecode(out var tmpValueType, out r))
                                {
                                    return false;
                                }

                                pos += r;
                                storageEntries[j].mapPrpos.valueType = (uint)tmpValueType;
                                break;
                            default:
                                return false;
                        }

                        if (!buff[pos..].TryDecodeVecU8(out storageEntries[j].storageDefault, out r))
                        {
                            return false;
                        }

                        pos += r;
                        if (!buff[pos..].TryDecodeVectorPrefix(out var tmpEntryDocsLen, out r))
                        {
                            return false;
                        }

                        pos += r;
                        var entryDocsLen = (int)tmpEntryDocsLen;
                        var storageEntryDocs =
                            setting.storeDocs ? new string[entryDocsLen] : Array.Empty<string>();
                        for (int k = 0; k < entryDocsLen; k++)
                        {
                            if (!buff[pos..].TryDecodeVecU8(out var entryDocsBytes, out r))
                            {
                                return false;
                            }

                            pos += r;
                            if (setting.storeDocs)
                            {
                                storageEntryDocs[k] = Encoding.UTF8.GetString(entryDocsBytes);
                            }
                        }
                    }
                }
                #endregion
                #region call metadata
                if (!buff[pos..].TryDecodePrefix(out var opCall))
                {
                    return false;
                }

                ++pos;
                uint callTypeId = default;
                if (!opCall.IsNone())
                {
                    if (!buff[pos..].TryCompactDecode(out var tmpCallTypeId, out r))
                    {
                        return false;
                    }

                    callTypeId = (uint)tmpCallTypeId;
                    pos += r;
                }
                #endregion
                #region event metadata
                if (!buff[pos..].TryDecodePrefix(out var opEvent))
                {
                    return false;
                }

                ++pos;
                uint eventTypeId = default;
                if (!opEvent.IsNone())
                {
                    if (!buff[pos..].TryCompactDecode(out var tmpEventTypeId, out r))
                    {
                        return false;
                    }

                    eventTypeId = (uint)tmpEventTypeId;
                    pos += r;
                }
                #endregion
                #region constant metadata
                if (!buff[pos..].TryDecodeVectorPrefix(out var tmpConstantsLen, out r))
                {
                    return false;
                }

                pos += r;
                var constantsLen = (int)tmpConstantsLen;
                var constants =
                    new (string name, uint typeId, byte[] scaleValue, string[] docs)[constantsLen];
                for (int j = 0; j < constantsLen; j++)
                {
                    if (!buff[pos..].TryDecodeVecU8(out var constantNameBytes, out r))
                    {
                        return false;
                    }

                    pos += r;
                    constants[j].name = Encoding.UTF8.GetString(constantNameBytes);
                    if (!buff[pos..].TryCompactDecode(out var tmpConstantTypeId, out r))
                    {
                        return false;
                    }

                    constants[j].typeId = (uint)tmpConstantTypeId;
                    pos += r;
                    if (!buff[pos..].TryDecodeVecU8(out constants[j].scaleValue, out r))
                    {
                        return false;
                    }

                    pos += r;
                    if (!buff[pos..].TryDecodeVectorPrefix(out var tmpConstantDocsLen, out r))
                    {
                        return false;
                    }

                    pos += r;
                    var constantDocsLen = (int)tmpConstantDocsLen;
                    constants[j].docs =
                        setting.storeDocs ? new string[constantDocsLen] : Array.Empty<string>();
                    for (int k = 0; k < constantDocsLen; k++)
                    {
                        if (!buff[pos..].TryDecodeVecU8(out var constantDocsBytes, out r))
                        {
                            return false;
                        }

                        pos += r;
                        if (setting.storeDocs)
                        {
                            constants[j].docs[k] = Encoding.UTF8.GetString(constantDocsBytes);
                        }
                    }
                }
                #endregion
                #region error metadata
                if (!buff[pos..].TryDecodePrefix(out var opError))
                {
                    return false;
                }

                ++pos;
                uint errorTypeId = default;
                if (!opError.IsNone())
                {
                    if (!buff[pos..].TryCompactDecode(out var tmpErrorTypeId, out r))
                    {
                        return false;
                    }

                    errorTypeId = (uint)tmpErrorTypeId;
                    pos += r;
                }
                #endregion

                var palletIndex = buff[pos++];
                target.indexTable.Add(palletName, palletIndex);
                target.storageDictionary.Add(palletIndex, (storagePfx, storageEntries));
                target.callDictionary.Add(palletIndex, (opCall, callTypeId));
                target.eventDictionary.Add(palletIndex, (opEvent, eventTypeId));
                target.constantsDictionary.Add(palletIndex, constants);
                target.errorDictionary.Add(palletIndex, (opError, errorTypeId));
            }
            #endregion
            return true;
        }

        bool TryDecodeExtrinsicMetadata(Span<byte> buff, MetadataStoreSetting _,
            ExtrinsicMetadataV14 target, ref int pos)
        {
            if (!buff[pos..].TryCompactDecode(out var tmpExtType, out var r))
            {
                return false;
            }

            pos += r;
            target.extrinsicTypeId = (uint)tmpExtType;
            target.version = buff[pos++];

            #region signed extensions
            if (!buff[pos..].TryDecodeVectorPrefix(out var tmpSignedExsLen, out r))
            {
                return false;
            }

            pos += r;
            var signedExsLen = (int)tmpSignedExsLen;
            for (int i = 0; i < signedExsLen; i++)
            {
                if (!buff[pos..].TryDecodeVecU8(out var tmpIdentBytes, out r))
                {
                    return false;
                }

                pos += r;
                if (!buff[pos..].TryCompactDecode(out var tmpSignedExType, out r))
                {
                    return false;
                }

                pos += r;
                if (!buff[pos..].TryCompactDecode(out var tmpAdditinal, out r))
                {
                    return false;
                }

                pos += r;
                target.signedExtensionsList.Add((
                    Encoding.UTF8.GetString(tmpIdentBytes),
                    (uint)tmpSignedExType,
                    (uint)tmpAdditinal
                ));
            }
            #endregion
            return true;
        }
    }
}
