using System.Collections.Generic;

namespace Kingmaker.AreaLogic.SceneControllables;

public class ControllableComponentCache
{
	public static readonly HashSet<ControllableComponent> All;

	static ControllableComponentCache()
	{
		All = new HashSet<ControllableComponent>();
	}
}
