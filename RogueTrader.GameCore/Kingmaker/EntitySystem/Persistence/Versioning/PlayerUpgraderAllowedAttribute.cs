using System;

namespace Kingmaker.EntitySystem.Persistence.Versioning;

public class PlayerUpgraderAllowedAttribute : Attribute
{
	public bool RequiresArea { get; }

	public PlayerUpgraderAllowedAttribute(bool requiresArea = false)
	{
		RequiresArea = requiresArea;
	}
}
