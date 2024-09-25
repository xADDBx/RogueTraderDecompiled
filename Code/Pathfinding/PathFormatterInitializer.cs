using MemoryPack;

namespace Pathfinding;

public static class PathFormatterInitializer
{
	public static void RegisterFormatter()
	{
		if (!MemoryPackFormatterProvider.IsRegistered<Path>())
		{
			MemoryPackFormatterProvider.Register(new PathFormatter());
		}
	}
}
