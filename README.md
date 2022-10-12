# smoldot-sharp
An experiment to run smoldot in c# environment.<br>
smoldot is here. https://github.com/paritytech/smoldot<br>

<h3>current status</h3><br>
1. implementing and testing "transaction_unstable_submitAndWatch", or "author_submitAndWatchExtrinsic".<br>
2. it is possible to see extrinsics are in blocks.<br>
3. only tested on local test network.<br>
4. only tested with ws (not wss).<br>
5. only tested wth ip4.<br>
6. only tested wth single stream.<br>
7. testing nodes are polkadot with rococo-local-testnet.json chainspec file.

<h3>current issues</h3><br>
1. (almost) no "finalized" events are received while watching extrinsics. <br>
2. smoldot says "invalid transaction" because "bad proof" after 5-10 minutes after launching.<br>
3. no "bloadcast" happens when all local nodes are written as bootnode in chain-spec.<br>
    (launching two local node, write both as bootnode to chain-spec)<br>
4. samething happens when save database content locally, and relaunch with the data.<br>
5. smoldot continues to try connecting to node that is already shutdowned.<br>
