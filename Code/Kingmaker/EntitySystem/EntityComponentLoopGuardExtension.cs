using JetBrains.Annotations;

namespace Kingmaker.EntitySystem;

public static class EntityComponentLoopGuardExtension
{
	public static EntityFactComponentLoopGuard RequestLoopGuard([NotNull] this EntityFactComponent component)
	{
		return EntityFactComponentLoopGuard.Request(component);
	}
}
