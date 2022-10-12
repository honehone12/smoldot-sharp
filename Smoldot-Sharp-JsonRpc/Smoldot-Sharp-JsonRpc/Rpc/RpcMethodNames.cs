namespace SmoldotSharp.JsonRpc
{
    public static class RpcMethodNames
    {
        public const string GetMetadata = "state_getMetadata";
        public const string GetRuntimeVersion = "state_getRuntimeVersion";
        public const string GetBlockHash = "chain_getBlockHash";
        public const string GetFinalizedHead = "chain_getFinalizedHead";
        public const string GetHeader = "chain_getHeader";
        public const string AccountNextIndex = "system_accountNextIndex";
        public const string SubmitExtrinsic = "author_submitExtrinsic";
        public const string SubmitAndWatchExtrinsic = "author_submitAndWatchExtrinsic";

        // https://github.com/paritytech/json-rpc-interface-spec/
        public const string DatabaseContent = "chainHead_unstable_finalizedDatabase";
        public const string TransactionSubmitAndWatch = "transaction_unstable_submitAndWatch";
        public const string GenesisHash = "chainHead_unstable_genesisHash";
    }
}