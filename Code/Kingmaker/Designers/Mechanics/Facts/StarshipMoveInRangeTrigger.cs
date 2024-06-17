using Code.GameCore.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4ffdad852e91e7b44b39a84c90cd9978")]
public class StarshipMoveInRangeTrigger : UnitFactComponentDelegate, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public int minDistance;

	public int maxDistance;

	public bool CheckFaction;

	[SerializeField]
	[ShowIf("CheckFaction")]
	private BlueprintFactionReference m_Faction;

	public ActionList ActionOnSelf;

	public ActionList ActionsOnUnit;

	public BlueprintFaction Faction => m_Faction?.Get();

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (!CheckFaction || unit.GetFactionOptional()?.Blueprint == Faction)
		{
			int num = base.Owner.DistanceToInCells(unit);
			if (num >= minDistance && num <= maxDistance)
			{
				base.Fact.RunActionInContext(ActionOnSelf, base.Owner.ToITargetWrapper());
				base.Fact.RunActionInContext(ActionsOnUnit, unit.ToITargetWrapper());
			}
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
