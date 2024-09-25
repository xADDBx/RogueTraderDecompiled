using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Networking.Serialization;

public class GameStateSerializationContext : ContextData<GameStateSerializationContext>
{
	public bool SplitState;

	protected override void Reset()
	{
		SplitState = false;
	}
}
