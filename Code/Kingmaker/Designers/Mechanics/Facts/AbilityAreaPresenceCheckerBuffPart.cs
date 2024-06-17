using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("d1220b217b8a4dc38ad25ae7c625e75b")]
public class AbilityAreaPresenceCheckerBuffPart : UnitBuffComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		AreaEffectEntity areaEffectEntity = Game.Instance.State.AreaEffects.FirstOrDefault((AreaEffectEntity p) => base.Buff.Sources.HasItem((EntityFactSource i) => i.Entity == p));
		AbilityAreaEffectUnitPresenceChecker abilityAreaEffectUnitPresenceChecker = areaEffectEntity?.Blueprint.GetComponent<AbilityAreaEffectUnitPresenceChecker>();
		if (areaEffectEntity == null || abilityAreaEffectUnitPresenceChecker == null)
		{
			return;
		}
		MechanicEntity maybeCaster = areaEffectEntity.Context.MaybeCaster;
		if (AbilityAreaEffectUnitPresenceChecker.CheckTargetType(maybeCaster, evt.ConcreteTarget, abilityAreaEffectUnitPresenceChecker.CheckForNoTargetsOfType))
		{
			foreach (BaseUnitEntity item in areaEffectEntity.InGameUnitsInside)
			{
				if (AbilityAreaEffectUnitPresenceChecker.CheckTargetType(maybeCaster, item, abilityAreaEffectUnitPresenceChecker.CheckForNoTargetsOfType))
				{
					return;
				}
				using (ContextData<AreaEffectContextData>.Request().Setup(areaEffectEntity))
				{
					using (areaEffectEntity.Context.GetDataScope(item.ToTargetWrapper()))
					{
						abilityAreaEffectUnitPresenceChecker.ActionsOnAllUnitsInside.Run();
					}
				}
			}
		}
		foreach (BaseUnitEntity item2 in areaEffectEntity.InGameUnitsInside)
		{
			if (!item2.LifeState.IsDead && item2.CombatGroup.IsEnemy(base.Owner))
			{
				break;
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
