using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SmoldotSharp.JsonRpc
{
    using MetadataInternal;
    using LatestMetaDecoder = MetadataInternal.MetaV14Decoder;
    using LatestMetaQuery = MetaQueryV14;

    public readonly struct MetadataStoreSetting
    {
        public readonly bool storeDocs;

        public MetadataStoreSetting(bool storeDocs)
        {
            this.storeDocs = storeDocs;
        }
    }

    public static class Metadata
    {
        public const uint MagicNumber = 1635018093;

        static readonly MetaDecoder DeserializeMethod = new LatestMetaDecoder();
        static MetadataStoreSetting StoreSetting = new MetadataStoreSetting(false);
        static Meta? MetaChache;

        public static LatestMetaQuery GetQuery
        {
            get
            {
                if (MetaChache?.GetQuery is LatestMetaQuery query)
                {
                    return query;
                }

                throw new Exception("Please request Metadata and call DecodeMetadata()." +
                    "If you have already done, this is from a bug of version mismatch.");
            }
        }

        public static void SetMetadataStoreSetting(MetadataStoreSetting storeSetting)
        {
            StoreSetting = storeSetting;
        }

        public static bool DecodeMetadata(string src)
        {
            var srcSpan = src.AsSpan().Remove0X();
            var buff = new Span<byte>(new byte[srcSpan.Length / 2]);
            if (!srcSpan.TryHexToBytes(buff, out var w) || w != buff.Length)
            {
                return false;
            }

            MetaChache = DeserializeMethod.Decode(buff, StoreSetting);

            return MetaChache != null;
        }

        public static ReadOnlyCollection<string>? String()
        {
            return MetaChache?.String().AsReadOnly();
        }
    }
}

namespace SmoldotSharp.JsonRpc.MetadataInternal
{
    public abstract class Meta
    {
        public abstract MetaQuery GetQuery { get; }

        public abstract List<string> String();
    }

    public abstract class MetaDecoder
    {
        public abstract Meta? Decode(Span<byte> bytes, MetadataStoreSetting setting);
    }

    public abstract class MetaQuery
    {
        // Empty
    }
}