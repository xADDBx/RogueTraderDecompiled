using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("6132a0a1d20184f408ddae21ae475cee")]
public class RestrictedFiringAreaComponent : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	private AoEPattern m_Pattern;

	public int RestrictedAngleDegrees => m_Pattern.Angle;

	[NotNull]
	public HashSet<CustomGridNodeBase> GetRestrictedArea(CustomGridNodeBase casterNode, RestrictedFiringArc arc, Vector3 dir)
	{
		HashSet<CustomGridNodeBase> hashSet = TempHashSet.Get<CustomGridNodeBase>();
		if (m_Pattern == null)
		{
			return hashSet;
		}
		CustomGridNodeBase customGridNodeBase = casterNode;
		int forwardDirection = CustomGraphHelper.GuessDirection(dir);
		int directionId = GetDirectionId(forwardDirection, arc);
		switch (arc)
		{
		case RestrictedFiringArc.Port:
			dir = Quaternion.AngleAxis(-90f, Vector3.up) * dir;
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(directionId);
			break;
		case RestrictedFiringArc.Starboard:
			dir = Quaternion.AngleAxis(90f, Vector3.up) * dir;
			customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(directionId);
			break;
		case RestrictedFiringArc.Aft:
			dir = Quaternion.AngleAxis(180f, Vector3.up) * dir;
			break;
		}
		dir = CustomGraphHelper.AdjustDirection(dir);
		foreach (CustomGridNodeBase node in m_Pattern.GetOriented(customGridNodeBase, dir).Nodes)
		{
			hashSet.Add(node);
		}
		return hashSet;
	}

	private int GetDirectionId(int forwardDirection, RestrictedFiringArc side)
	{
		return side switch
		{
			RestrictedFiringArc.Port => CustomGraphHelper.LeftNeighbourDirection[CustomGraphHelper.LeftNeighbourDirection[forwardDirection]], 
			RestrictedFiringArc.Starboard => CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[forwardDirection]], 
			RestrictedFiringArc.Aft => CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[CustomGraphHelper.RightNeighbourDirection[forwardDirection]]]], 
			_ => forwardDirection, 
		};
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
