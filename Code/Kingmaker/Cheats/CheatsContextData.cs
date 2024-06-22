using Core.Cheats;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Cheats;

public static class CheatsContextData
{
	private class TestContextData : ContextData<TestContextData>
	{
		protected override void Reset()
		{
		}
	}

	[Cheat(Name = "break_context_data", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RandomEncounterStatusSwitch()
	{
		ContextData<TestContextData>.Request();
	}
}
