using System;
using ScaleSharpLight;

namespace SmoldotSharp.JsonRpc
{
    using MetadataInternal;

    public class MetaQueryV14 : MetaQuery
    {
        readonly MetaV14 meta;

        public MetaQueryV14(MetaV14 meta)
        {
            this.meta = meta;
        }

        public byte ExtrinsicVersion => meta.extrinsicMetadata.version;

        public bool HasModule(string name)
        {
            return meta.palletMetada.indexTable.ContainsKey(name);
        }

        public (bool, CallIndex) FindCallIndex(string moduleName, string callName)
        {
            if (meta.palletMetada.indexTable.ContainsKey(moduleName))
            {
                var modIdx = meta.palletMetada.indexTable[moduleName];
                (var op, var typeId) = meta.palletMetada.callDictionary[modIdx];
                if (!op.IsNone())
                {
                    if (meta.typeRegistry.idTable[typeId] == TypeDefV14.Variant)
                    {
                        var data = meta.typeRegistry.variantDictionary[typeId].typeDef;
                        var (name, _, index, _) = Array.Find(data, (d) => d.name == callName);
                        if (!string.IsNullOrEmpty(name))
                        {
                            return (true, new CallIndex(modIdx, index));
                        }
                    }
#if DEBUG
                    throw new Exception("Ooops!! This is a bug of SmaldotSharp.Json!!");
#endif
                }
            }
            return (false, default);
        }
    }
}
