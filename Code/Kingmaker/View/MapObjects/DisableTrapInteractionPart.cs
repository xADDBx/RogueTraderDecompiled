using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.MapObjects.Traps;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

public class DisableTrapInteractionPart : InteractionPart<InteractionSettings>, IHashable
{
	public new TrapObjectData Owner => (TrapObjectData)base.Owner;

	protected override UIInteractionType GetDefaultUIType()
	{
		return UIInteractionType.Action;
	}

	public override BaseUnitEntity SelectUnit(ReadonlyList<BaseUnitEntity> units, bool muteEvents = false, StatType? skillFromVariant = null)
	{
		return Owner.SelectUnit(units, muteEvents);
	}

	public override bool CheckTechUse()
	{
		return true;
	}

	public override bool CanInteract()
	{
		if (base.CanInteract() && Owner.TrapActive)
		{
			if (!Owner.View.IsScriptZoneTrigger)
			{
				return Owner.IsAwarenessCheckPassed;
			}
			return true;
		}
		return false;
	}

	protected override void OnInteract(BaseUnitEntity user)
	{
		if (Owner.TrapActive)
		{
			Owner.Interact(user);
		}
	}

	public override bool IsEnoughCloseForInteraction(BaseUnitEntity unit, Vector3? position = null)
	{
		if (unit.IsInCombat && Owner.View.Settings.ScriptZoneTrigger != null)
		{
			IEnumerable<CustomGridNodeBase> nodesSpiralAround = GridAreaHelper.GetNodesSpiralAround((CustomGridNodeBase)unit.CurrentNode.node, unit.SizeRect, 1);
			foreach (IScriptZoneShape shape in Owner.View.Settings.ScriptZoneTrigger.Shapes)
			{
				foreach (CustomGridNodeBase item in nodesSpiralAround)
				{
					if (shape.Contains(item))
					{
						return true;
					}
				}
			}
		}
		return base.IsEnoughCloseForInteraction(unit, position);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
