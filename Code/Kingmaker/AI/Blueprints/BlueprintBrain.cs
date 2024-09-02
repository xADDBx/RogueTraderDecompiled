using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AI.Blueprints;

[TypeId("d0e20ba43f1689d4a964ffe958a5fa1a")]
public class BlueprintBrain : BlueprintBrainBase
{
	public ScoreOrder ScoreOrder;

	[SerializeField]
	private PropertyCalculator[] HatedTargetConditions;

	[Tooltip("The earlier the ability in the list, the higher its priority. Abilities that are not listed here follow priority abilities")]
	public AbilityOrder AbilityPriorityOrder;

	[Tooltip("Abilities that the unit considers when choosing a movement strategy. It tries to choose the best position for casting one of the abilities from the list.")]
	public AbilitySourceWrapper[] MovementInfluentAbilities = new AbilitySourceWrapper[0];

	[SerializeField]
	private AbilitySettings[] AbilitySettings = new AbilitySettings[0];

	public float HitUnintendedTargetPenalty = 1f;

	[Tooltip("If true, unit will use scatter shots more carefully")]
	public bool IsCarefulShooter;

	[Tooltip("If true, will cast abilities even if he would get AoO for it")]
	public bool IgnoreAoOForCasting;

	[Tooltip("If true, unit will try to escape from AoO threat before other actions")]
	public bool ResponseToAoOThreat;

	[Tooltip("If true, unit will try to escape from AoO threat after other actions")]
	public bool ResponseToAoOThreatAfterAbilities;

	[Tooltip("Usual melee units walk straight to the closest target, but smart ones also try to position wisely")]
	public MeleeBrainType MeleeBrainType;

	public AbilitySourceWrapper[] BeforeMoveAbilities = new AbilitySourceWrapper[0];

	[Tooltip("Will try to use this abilities as main one for the turn even if they are listed in BeforeMove or AfterMove\nChanges nothing if ability isn't listed in BeforeMove or AfterMove.")]
	public AbilitySourceWrapper[] MoveAndCastAbilities = new AbilitySourceWrapper[0];

	public AbilitySourceWrapper[] AfterMoveAbilities = new AbilitySourceWrapper[0];

	public TargetWrapper[] PriorityDestroyTargets = new TargetWrapper[0];

	public IReadOnlyList<PropertyCalculator> HatedTargetConditionsReadOnly => HatedTargetConditions;

	public IReadOnlyList<AbilitySettings> AbilitySettingsReadOnly => AbilitySettings;

	public override List<TargetInfo> GetHatedTargets(PropertyContext context, List<TargetInfo> enemies)
	{
		List<TargetInfo> list = TempList.Get<TargetInfo>();
		if (HatedTargetConditions == null)
		{
			return list;
		}
		PropertyCalculator[] hatedTargetConditions = HatedTargetConditions;
		foreach (PropertyCalculator propertyCalculator in hatedTargetConditions)
		{
			foreach (TargetInfo enemy in enemies)
			{
				if (propertyCalculator.GetValue(context.WithCurrentTarget(enemy.Entity)) > 0)
				{
					list.Add(enemy);
				}
			}
			if (list.Count > 0)
			{
				break;
			}
		}
		return list;
	}

	public override AbilitySettings GetCustomAbilitySettings(BlueprintAbility ability)
	{
		AbilitySettings[] abilitySettings = AbilitySettings;
		foreach (AbilitySettings abilitySettings2 in abilitySettings)
		{
			if (abilitySettings2.AbilitySource.Type == AbilitySourceWrapper.AbilitySourceType.Ability && abilitySettings2.AbilitySource.Ability == ability)
			{
				return abilitySettings2;
			}
		}
		abilitySettings = AbilitySettings;
		foreach (AbilitySettings abilitySettings3 in abilitySettings)
		{
			if (abilitySettings3.AbilitySource.Type == AbilitySourceWrapper.AbilitySourceType.Equipment && abilitySettings3.AbilitySource.Abilities.Contains(ability))
			{
				return abilitySettings3;
			}
		}
		return null;
	}

	public override List<MechanicEntity> GetPriorityDestroyTarget()
	{
		return PriorityDestroyTargets.Select((TargetWrapper wrapper) => wrapper.Entity).ToList();
	}
}
