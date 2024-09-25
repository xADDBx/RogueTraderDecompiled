using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartWarhammerMovementApPerCellThreateningArea : UnitPart, IHashable
{
	private readonly List<(EntityFactComponent Runtime, OverrideWarhammerMovementApPerCellThreateningArea Component)> m_ThreateningAreaEntries = new List<(EntityFactComponent, OverrideWarhammerMovementApPerCellThreateningArea)>();

	public static float GetThreateningArea(BaseUnitEntity unit)
	{
		PartWarhammerMovementApPerCellThreateningArea warhammerMovementApPerCellThreateningAreaOptional = unit.GetWarhammerMovementApPerCellThreateningAreaOptional();
		if (warhammerMovementApPerCellThreateningAreaOptional != null)
		{
			foreach (var threateningAreaEntry in warhammerMovementApPerCellThreateningAreaOptional.m_ThreateningAreaEntries)
			{
				using (threateningAreaEntry.Runtime.RequestEventContext())
				{
					float? warhammerMovementApPerCellThreateningArea = threateningAreaEntry.Component.GetWarhammerMovementApPerCellThreateningArea();
					if (warhammerMovementApPerCellThreateningArea.HasValue)
					{
						return warhammerMovementApPerCellThreateningArea.GetValueOrDefault();
					}
				}
			}
		}
		return unit.View.Blueprint.WarhammerMovementApPerCellThreateningArea;
	}

	public void Add(OverrideWarhammerMovementApPerCellThreateningArea component)
	{
		m_ThreateningAreaEntries.Add((component.Runtime, component));
	}

	public void Remove(OverrideWarhammerMovementApPerCellThreateningArea component)
	{
		m_ThreateningAreaEntries.Remove((component.Runtime, component));
		RemoveSelfIfEmpty();
	}

	private void RemoveSelfIfEmpty()
	{
		if (m_ThreateningAreaEntries.Empty())
		{
			RemoveSelf();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
