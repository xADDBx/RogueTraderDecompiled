using System.Globalization;
using System.IO;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.Serialization;
using Kingmaker.Replay;
using Kingmaker.StateHasher;
using Newtonsoft.Json;
using StateHasher.Core.Hashers;

namespace Kingmaker.Networking;

public class HashCalculator
{
	private readonly StringWriter m_StringWriter = new StringWriter(CultureInfo.InvariantCulture);

	private static JsonSerializer JsonSerializer => GameStateJsonSerializer.Serializer;

	public HashCalculator()
	{
		HasherProviderInitializer.InitializeDefault();
	}

	public int GetCurrentStateHash()
	{
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		return GetStateHashByNewMethod(currentNetworkTick);
	}

	public int GetStateHashByNewMethod(int networkStepIndex)
	{
		using StateHasherContext stateHasherContext = StateHasherContext.Request();
		HashableState state = stateHasherContext.GetHashableState();
		SplitState(ref state);
		if (Kingmaker.Replay.Replay.IsActive)
		{
			state.synchronizedData = null;
			state.signalService = null;
		}
		return StructHasher<HashableState>.GetHash128(ref state).GetHashCode();
	}

	private static void SplitState(ref HashableState state)
	{
		if (ContextData<GameStateSerializationContext>.Current != null)
		{
			ContextData<GameStateSerializationContext>.Current.SplitState = true;
		}
		int currentNetworkTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		state = new HashableState
		{
			player = ((currentNetworkTick % 5 == 0) ? state.player : null),
			sceneEntitiesState = ((currentNetworkTick % 5 == 1) ? state.sceneEntitiesState : null),
			areaPersistentState = ((currentNetworkTick % 5 == 2) ? state.areaPersistentState : null),
			randomState = ((currentNetworkTick % 5 == 3) ? state.randomState : null),
			synchronizedData = ((currentNetworkTick % 5 == 4) ? state.synchronizedData : null),
			signalService = ((currentNetworkTick % 5 == 4) ? state.signalService : null)
		};
	}
}
