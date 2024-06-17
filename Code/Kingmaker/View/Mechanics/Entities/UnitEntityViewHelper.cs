using JetBrains.Annotations;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharactersRigidbody;

namespace Kingmaker.View.Mechanics.Entities;

public static class UnitEntityViewHelper
{
	public static IKController GetIKControllerOptional([NotNull] this AbstractUnitEntityView view)
	{
		return (view as UnitEntityView)?.IkController;
	}

	public static RigidbodyCreatureController GetRigidbodyControllerOptional([NotNull] this AbstractUnitEntityView view)
	{
		return (view as UnitEntityView)?.RigidbodyController;
	}
}
