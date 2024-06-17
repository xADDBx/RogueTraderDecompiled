using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.View;

public static class ObstaclesHelper
{
	public const float GroupingDistance = 0.65f;

	public const float MaxNavmeshContactDistance = 2f;

	public static void RemoveFromGroup([NotNull] UnitMovementAgentBase unit)
	{
		if (!unit.ConnectedToObstacles)
		{
			return;
		}
		unit.ConnectedToObstacles = false;
		List<UnitMovementAgentBase> obstaclesGroup = unit.ObstaclesGroup;
		if (obstaclesGroup == null)
		{
			return;
		}
		unit.ObstaclesGroup = null;
		foreach (UnitMovementAgentBase item in obstaclesGroup)
		{
			item.ObstaclesGroup = null;
			item.UnitContacts = null;
		}
		for (int i = 0; i < obstaclesGroup.Count; i++)
		{
			UnitMovementAgentBase unitMovementAgentBase = obstaclesGroup[i];
			if (GroupingForbidden(unitMovementAgentBase) || unitMovementAgentBase == unit)
			{
				continue;
			}
			for (int j = i + 1; j < obstaclesGroup.Count; j++)
			{
				UnitMovementAgentBase unitMovementAgentBase2 = obstaclesGroup[j];
				if (!GroupingForbidden(unitMovementAgentBase2) && !(unitMovementAgentBase2 == unit))
				{
					TryConnectUnits(unitMovementAgentBase, unitMovementAgentBase2);
				}
			}
		}
	}

	public static void ConnectToGroups([NotNull] UnitMovementAgentBase unit)
	{
		if (unit == null || GroupingForbidden(unit) || unit.ConnectedToObstacles)
		{
			return;
		}
		unit.ConnectedToObstacles = true;
		if (unit.Unit == null)
		{
			return;
		}
		PartCombatGroup combatGroupOptional = unit.Unit.EntityData.GetCombatGroupOptional();
		if (combatGroupOptional?.Memory == null)
		{
			return;
		}
		foreach (UnitMovementAgentBase item in (from u in combatGroupOptional.Memory.UnitsList
			select u.Unit into u
			where u?.View != null
			select u.View.MovementAgent).Concat(combatGroupOptional.Select((BaseUnitEntity u) => u.View.MovementAgent)).Distinct())
		{
			TryConnectUnits(unit, item);
		}
	}

	private static bool GroupingForbidden(UnitMovementAgentBase u)
	{
		if (!(u.Unit == null) && !u.WantsToMove)
		{
			return u.Unit.IsProne;
		}
		return true;
	}

	private static void TryConnectUnits(UnitMovementAgentBase o1, UnitMovementAgentBase o2)
	{
		if (o1 == o2 || o1 == null || o2 == null || GroupingForbidden(o1) || GroupingForbidden(o2) || !o1.ConnectedToObstacles || !o2.ConnectedToObstacles)
		{
			return;
		}
		PartFaction partFaction = ((o1.Unit != null) ? o1.Unit.EntityData.GetFactionOptional() : null);
		PartFaction partFaction2 = ((o2.Unit != null) ? o2.Unit.EntityData.GetFactionOptional() : null);
		if (!(partFaction == null) && !(partFaction2 == null) && !(partFaction != partFaction2))
		{
			float sqrMagnitude = (o1.transform.position - o2.transform.position).To2D().sqrMagnitude;
			float num = o1.Corpulence + o2.Corpulence + 0.65f;
			if (!(sqrMagnitude > num * num))
			{
				MergeGroups(o1, o2);
				MarkContacts(o1, o2);
			}
		}
	}

	private static void MarkContacts(UnitMovementAgentBase o1, UnitMovementAgentBase o2)
	{
		if (o1.UnitContacts == null)
		{
			o1.UnitContacts = new List<UnitMovementAgentBase>();
		}
		if (o2.UnitContacts == null)
		{
			o2.UnitContacts = new List<UnitMovementAgentBase>();
		}
		if (o1.UnitContacts.Contains(o2))
		{
			Debug.LogError("Adding obstacle twice");
		}
		if (o2.UnitContacts.Contains(o1))
		{
			Debug.LogError("Adding obstacle twice");
		}
		o1.UnitContacts.Add(o2);
		o2.UnitContacts.Add(o1);
	}

	private static void MergeGroups(UnitMovementAgentBase o1, UnitMovementAgentBase o2)
	{
		try
		{
			if (o1.ObstaclesGroup == null && o2.ObstaclesGroup == null)
			{
				List<UnitMovementAgentBase> obstaclesGroup = new List<UnitMovementAgentBase> { o1, o2 };
				o1.ObstaclesGroup = obstaclesGroup;
				o2.ObstaclesGroup = obstaclesGroup;
			}
			else
			{
				if (o1.ObstaclesGroup == o2.ObstaclesGroup)
				{
					return;
				}
				if (o1.ObstaclesGroup == null)
				{
					o2.ObstaclesGroup.Add(o1);
					o1.ObstaclesGroup = o2.ObstaclesGroup;
					return;
				}
				if (o2.ObstaclesGroup == null)
				{
					o1.ObstaclesGroup.Add(o2);
					o2.ObstaclesGroup = o1.ObstaclesGroup;
					return;
				}
				o1.ObstaclesGroup.AddRange(o2.ObstaclesGroup);
				o1.ObstaclesGroup.ForEach(delegate(UnitMovementAgentBase o)
				{
					o.ObstaclesGroup = o1.ObstaclesGroup;
				});
				o2.ObstaclesGroup = o1.ObstaclesGroup;
			}
		}
		finally
		{
			ValidateSameGroup(o1, o2);
		}
	}

	public static void ValidateSameGroup(UnitMovementAgentBase o1, UnitMovementAgentBase o2)
	{
		if (o1.ObstaclesGroup != o2.ObstaclesGroup)
		{
			PFLog.Default.Error("Groups are different after merging");
		}
		if (o1.ObstaclesGroup == null || o2.ObstaclesGroup == null)
		{
			return;
		}
		foreach (UnitMovementAgentBase item in o1.ObstaclesGroup)
		{
			if (item.ObstaclesGroup != o1.ObstaclesGroup)
			{
				PFLog.Default.Error("Different groups for obstacles in one group");
			}
		}
	}
}
