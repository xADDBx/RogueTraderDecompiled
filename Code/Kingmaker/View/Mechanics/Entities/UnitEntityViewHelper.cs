using JetBrains.Annotations;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.CharactersRigidbody;
using UnityEngine;

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

	public static Vector3 GetViewPosition([NotNull] this AbstractUnitEntityView view, Vector3 mechanicsPosition)
	{
		if (view.MovementAgent.NodeLinkTraverser.IsTraverseNow)
		{
			return SizePathfindingHelper.FromMechanicsToViewPosition(view.EntityData, mechanicsPosition);
		}
		return view.GetViewPositionOnGround(mechanicsPosition);
	}
}
