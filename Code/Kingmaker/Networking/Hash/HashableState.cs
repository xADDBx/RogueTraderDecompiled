using Kingmaker.Controllers.Net;
using Kingmaker.EntitySystem;
using Kingmaker.Signals;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Networking.Hash;

[HashRoot]
public struct HashableState : IHashable
{
	[JsonProperty]
	public Kingmaker.Player player;

	[JsonProperty]
	public SceneEntitiesState sceneEntitiesState;

	[JsonProperty]
	public AreaPersistentState areaPersistentState;

	[JsonProperty]
	public RandomState randomState;

	[JsonProperty]
	public PlayerCommandsCollection<SynchronizedData> synchronizedData;

	[JsonProperty]
	public SignalService.SignalServiceState signalService;

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<Kingmaker.Player>.GetHash128(player);
		result.Append(ref val);
		Hash128 val2 = ClassHasher<SceneEntitiesState>.GetHash128(sceneEntitiesState);
		result.Append(ref val2);
		Hash128 val3 = ClassHasher<AreaPersistentState>.GetHash128(areaPersistentState);
		result.Append(ref val3);
		Hash128 val4 = ClassHasher<RandomState>.GetHash128(randomState);
		result.Append(ref val4);
		Hash128 val5 = ClassHasher<PlayerCommandsCollection<SynchronizedData>>.GetHash128(synchronizedData);
		result.Append(ref val5);
		Hash128 val6 = ClassHasher<SignalService.SignalServiceState>.GetHash128(signalService);
		result.Append(ref val6);
		return result;
	}
}
