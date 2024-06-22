using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("61054535efad8f742a9423ddbbb7a21f")]
public class UnitDeathTrigger : UnitFactComponentDelegate, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public enum FactionType
	{
		Any,
		Ally,
		Enemy,
		Companions
	}

	public bool AnyRadius;

	[HideIf("AnyRadius")]
	public ContextValue RadiusInMeters;

	public FactionType Faction;

	public ActionList Actions;

	public void HandleUnitSpawned()
	{
	}

	public void HandleUnitDestroyed()
	{
	}

	public void HandleUnitDeath()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity == base.Owner || baseUnitEntity == null)
		{
			return;
		}
		if (Faction != 0)
		{
			bool flag = base.Owner.CombatGroup.IsEnemy(baseUnitEntity);
			if ((flag && Faction == FactionType.Ally) || (!flag && Faction == FactionType.Enemy) || (Faction == FactionType.Companions && (!baseUnitEntity.IsPlayerFaction || !baseUnitEntity.IsInCompanionRoster())))
			{
				return;
			}
		}
		if (AnyRadius || base.Owner.DistanceTo(baseUnitEntity) <= (float)RadiusInMeters.Calculate(base.Context))
		{
			base.Fact.RunActionInContext(Actions, base.OwnerTargetWrapper);
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
