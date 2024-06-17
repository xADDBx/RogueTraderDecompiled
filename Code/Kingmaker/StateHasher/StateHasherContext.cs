using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Networking;
using Kingmaker.Networking.Hash;
using Kingmaker.Networking.Serialization;
using Kingmaker.Signals;
using StateHasher.Core;

namespace Kingmaker.StateHasher;

public readonly ref struct StateHasherContext
{
	private readonly GameStateSerializationContext m_Context;

	private StateHasherContext(GameStateSerializationContext context)
	{
		m_Context = context;
		RandomState.Instance.Refresh();
	}

	public HashableState GetHashableState()
	{
		HashableState result = default(HashableState);
		result.player = Game.Instance.State.PlayerState;
		result.sceneEntitiesState = Game.Instance.State.PlayerState.CrossSceneState;
		result.areaPersistentState = Game.Instance.State.LoadedAreaState;
		result.randomState = RandomState.Instance;
		result.synchronizedData = Game.Instance.SynchronizedDataController.SynchronizedData;
		result.signalService = SignalService.Instance.State;
		return result;
	}

	public void Dispose()
	{
		m_Context.Dispose();
		RecursiveReferences.Reset();
	}

	public static StateHasherContext Request()
	{
		return new StateHasherContext(ContextData<GameStateSerializationContext>.Request());
	}
}
