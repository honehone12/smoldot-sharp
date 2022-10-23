# smoldot-sharp
An experiment to run smoldot in c# environment.<br>
smoldot is here. https://github.com/paritytech/smoldot<br>

<h3>current status</h3>
<ol>
<li>implementing and testing "transaction_unstable_submitAndWatch", or "author_submitAndWatchExtrinsic".</li>
<li>it is possible to see extrinsics are in blocks.</li>
<li>only tested on local test network.</li>
<li>only tested with ws (not wss).</li>
<li>only tested wth ip4.</li>
<li>only tested wth single stream.</li>
<li>testing nodes are polkadot with rococo-local-testnet.json chainspec file.</li>
</ol>

<h3>current issues</h3>
<ol>
<li>(almost) no "finalized" events are received while watching extrinsics.</li>
<li>smoldot says "invalid transaction" because "bad proof" after 5-10 minutes after launching.</li>
<li>no "bloadcast" happens when all local nodes are written as bootnode in chain-spec.<br>
    (launching two local node, write both as bootnode to chain-spec)</li>
<li>samething happens when save database content locally, and relaunch with the data.</li>
<li>smoldot continues to try connecting to node that is already shutdowned.</li>
</ol>

<h3>todo</h3>
<ol>
<li>update extrinsic process from JIT style to subscription style.</li>
<li>my bad... I realized now (!!) this https://github.com/paritytech/smoldot/tree/main/bin/light-base is the repo for onother compile option...</li>
</ol>