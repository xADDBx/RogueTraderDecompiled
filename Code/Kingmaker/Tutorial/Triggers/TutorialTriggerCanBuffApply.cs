using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Tutorial.Solvers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[ClassInfoBox("Triggers on enter one of the specified areas, if buff could be applied\n`t|SolutionAbility` - ability with buff\n`t|SolutionUnit` - unit who can cast ability\n`t|TargetUnit` - tank unit (if `Check Tank Stat` is on)")]
[TypeId("2dde235fa57b5dd49b1b947dc1ee47cd")]
public class TutorialTriggerCanBuffApply : TutorialTrigger, IAreaHandler, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintAreaReference[] m_TriggerAreas;

	[SerializeField]
	private BlueprintAbilityReference m_Ability;

	[SerializeField]
	[InfoBox("Check if stat modificator from abilityBuff is greater than current modificator on top-AC unit (Tank)\nDesigned for Barkskin buff. Could work badly with some specific stat modifiers. Contact Dev, if not sure")]
	private bool m_CheckTankStat;

	[SerializeField]
	private bool m_AllowItemsWithSpell;

	public ReferenceArrayProxy<BlueprintArea> TriggerAreas
	{
		get
		{
			BlueprintReference<BlueprintArea>[] triggerAreas = m_TriggerAreas;
			return triggerAreas;
		}
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		using (ProfileScope.New("Tutor trigger. Pre-Buff"))
		{
			if (!TriggerAreas.HasReference(Game.Instance.CurrentlyLoadedArea))
			{
				return;
			}
			BaseUnitEntity tank = GetTank();
			BlueprintAbility blueprintAbility = m_Ability.Get();
			AddContextStatBonus addContextStatBonus = null;
			BlueprintBuff blueprintBuff = null;
			if (m_CheckTankStat)
			{
				blueprintBuff = (blueprintAbility.GetComponent<AbilityEffectRunAction>()?.Actions.Actions.FirstOrDefault((GameAction x) => x is ContextActionApplyBuff) as ContextActionApplyBuff)?.Buff;
				if (blueprintBuff != null)
				{
					addContextStatBonus = blueprintBuff.GetComponent<AddContextStatBonus>();
				}
			}
			IEnumerator<AbilityData> enumerator = PartySpellsEnumerator.Get(withAbilities: true);
			while (enumerator.MoveNext())
			{
				AbilityData current = enumerator.Current;
				if (current == null || blueprintAbility != current.Blueprint || (!m_AllowItemsWithSpell && current.SourceItem != null))
				{
					continue;
				}
				if (m_CheckTankStat && addContextStatBonus != null && !addContextStatBonus.Descriptor.IsStackable())
				{
					MechanicsContext mechanicsContext = current.CreateExecutionContext(tank).CloneFor(blueprintBuff, tank);
					mechanicsContext.Recalculate();
					ModifiableValue stat = tank.Stats.GetStat(addContextStatBonus.Stat);
					int num = addContextStatBonus.CalculateValue(mechanicsContext);
					int num2 = 0;
					foreach (ModifiableValue.Modifier modifier in stat.GetModifiers(addContextStatBonus.Descriptor))
					{
						if (!modifier.Stacks && num2 < modifier.ModValue)
						{
							num2 = modifier.ModValue;
						}
					}
					if (num2 < num)
					{
						Trigger(current, tank);
						break;
					}
					continue;
				}
				Trigger(current);
				break;
			}
		}
	}

	private void Trigger(AbilityData ability, BaseUnitEntity tankUnit = null)
	{
		TryToTrigger(null, delegate(TutorialContext context)
		{
			if (tankUnit != null)
			{
				context.TargetUnit = tankUnit;
			}
			context.SolutionAbility = ability;
			context.SolutionUnit = ability.Caster as BaseUnitEntity;
		});
	}

	private BaseUnitEntity GetTank()
	{
		if (m_CheckTankStat)
		{
			List<BaseUnitEntity> list = TempList.Get<BaseUnitEntity>();
			list.AddRange(Game.Instance.Player.PartyAndPets);
			list.Sort((BaseUnitEntity unit1, BaseUnitEntity unit2) => 0);
			return list[0];
		}
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
