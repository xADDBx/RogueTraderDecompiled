using Kingmaker.GameCommands;
using MemoryPack;
using MemoryPack.Formatters;

namespace Kingmaker.MemoryPack.Formatters;

public static class RogueTrader_GameCoreDynamicUnionFormatters
{
	public static void Register()
	{
		MemoryPackFormatterProvider.Register(new DynamicUnionFormatter<GameCommand>((0, typeof(GameCommandWithSynchronized))));
		MemoryPackFormatterProvider.Register(new DynamicUnionFormatter<GameCommandWithSynchronized>());
	}
}
