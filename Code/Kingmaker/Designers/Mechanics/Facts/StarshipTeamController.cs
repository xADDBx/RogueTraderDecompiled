using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintStarship))]
[TypeId("c679873c7dfdc494bbf909ab871de9cb")]
public class StarshipTeamController : UnitFactComponentDelegate, IUnitCombatHandler<EntitySubscriber>, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, EntitySubscriber>, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ITargetRulebookSubscriber, IUnitMovementHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	[SerializeField]
	private int HpPerUnit = 1;

	[SerializeField]
	private bool CheckLanding;

	[SerializeField]
	[ShowIf("CheckLanding")]
	private int LandingDistance;

	[SerializeField]
	[ShowIf("CheckLanding")]
	private ActionList LandingActions;

	private StarshipEntity Carrier => (base.Owner as StarshipEntity)?.GetSummonedMonsterOption()?.Summoner as StarshipEntity;

	private void RemoveExcessTeamObjects(BaseUnitEntity unit)
	{
		Transform transform = unit.View.gameObject.transform.Find("Team");
		if (!(transform == null))
		{
			int num = UnitsAlive(unit);
			for (int num2 = transform.childCount - 1; num2 >= num; num2--)
			{
				Object.DestroyImmediate(transform.GetChild(num2).gameObject);
			}
		}
	}

	private List<GameObject> GetTeamObjects(BaseUnitEntity unit)
	{
		GameObject gameObject = unit.View.gameObject;
		Transform transform = gameObject.transform.Find("Team");
		if (transform == null)
		{
			return new List<GameObject> { gameObject };
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in transform)
		{
			list.Add(item.gameObject);
		}
		return list;
	}

	public int UnitsAlive(BaseUnitEntity unit)
	{
		PartHealth partHealth = unit?.Health;
		if (partHealth == null)
		{
			return 1;
		}
		ModifiableValue stat = unit.Stats.GetStat(StatType.MilitaryRating);
		int num = HpPerUnit + stat.ModifiedValue - stat.BaseValue;
		int num2 = partHealth.HitPointsLeft / num;
		if (partHealth.HitPointsLeft % num != 0)
		{
			num2++;
		}
		return num2;
	}

	private void UpdateVisible(BaseUnitEntity unit)
	{
		List<GameObject> teamObjects = GetTeamObjects(unit);
		int num = UnitsAlive(unit);
		for (int i = 0; i < teamObjects.Count; i++)
		{
			teamObjects[i].SetActive(i < num);
		}
	}

	public void HandleUnitJoinCombat()
	{
		if (EventInvokerExtensions.Entity == base.Owner)
		{
			RemoveExcessTeamObjects(base.Owner);
			UpdateVisible(base.Owner);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			UpdateVisible(base.Owner);
		}
	}

	public void HandleUnitLeaveCombat()
	{
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		UpdateVisible(base.Owner);
	}

	public void HandleWaypointUpdate(int index)
	{
	}

	public void HandleMovementComplete()
	{
		if (CheckLanding && (EventInvokerExtensions.StarshipEntity == base.Owner || EventInvokerExtensions.StarshipEntity == Carrier) && DistanceToCarrier() <= LandingDistance)
		{
			base.Fact.RunActionInContext(LandingActions, Carrier.ToITargetWrapper());
		}
	}

	private int DistanceToCarrier()
	{
		CustomGridNodeBase a = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(base.Owner.Position).node;
		CustomGridNodeBase b = (CustomGridNodeBase)ObstacleAnalyzer.GetNearestNode(Carrier.Position).node;
		return CustomGraphHelper.GetWarhammerCellDistance(a, b);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
