using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI;
using Kingmaker.AI.Blueprints;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Warhammer.SpaceCombat.AI;

[TypeId("932f7051c5b3e14458ad57af12803b85")]
public class BlueprintStarshipBrain : BlueprintBrainBase
{
	public AbilitySettings[] AbilitySettings;

	public int AiDesiredDistanceToEnemies = 3;

	public float TrajectoryScoreMinThreshold = 1f;

	[Tooltip("Use own summoner as only target instead of usual list of enemy targets")]
	[SerializeField]
	private bool m_IsStrikecraftReturningBrain;

	[Tooltip("Ship should try to stay behind enemies")]
	[SerializeField]
	private bool m_TryToStayBehind;

	[Tooltip("How much ship scared of meteors. Values are compared with values of Weapon values settings.")]
	public float FearOfMeteors = 100f;

	[Tooltip("If there is no trajectory with a good enough score, starship tries to use one of specified abilities in that order")]
	[SerializeField]
	private List<BlueprintAbilityReference> m_ExtraMeasures = new List<BlueprintAbilityReference>();

	public List<BlueprintAbility> ExtraMeasures => (from x in m_ExtraMeasures
		where x?.Get() != null
		select x?.Get()).ToList();

	public bool IsStrikecraftReturningBrain => m_IsStrikecraftReturningBrain;

	public bool TryToStayBehind => m_TryToStayBehind;

	public StarshipEntity GetOverrideTarget(BaseUnitEntity unit)
	{
		if (!IsStrikecraftReturningBrain)
		{
			return null;
		}
		return unit.Facts.GetComponents<StarshipStrikecraftLogic>().FirstOrDefault()?.GetShipToLand(unit);
	}

	public bool TryOverrideTargets(DecisionContext context, ref List<TargetInfo> targets)
	{
		StarshipEntity overrideTarget = GetOverrideTarget(context.Unit);
		if (overrideTarget != null)
		{
			TargetInfo targetInfo = new TargetInfo();
			targetInfo.Init(overrideTarget);
			targets = new List<TargetInfo> { targetInfo };
			return true;
		}
		if (context.HatedFaction != null)
		{
			IEnumerable<TargetInfo> source = targets.Where((TargetInfo t) => (t.Entity as StarshipEntity)?.Blueprint.Faction == context.HatedFaction);
			if (source.Any())
			{
				targets = source.ToList();
				return true;
			}
		}
		return false;
	}

	public override int GetAbilityValue(BlueprintAbility ability, PropertyContext context)
	{
		return (GetAbilitySettings(ability)?.AbilityValue?.GetValue(context)).GetValueOrDefault();
	}

	public AbilitySettings GetAbilitySettings(BlueprintAbility ability)
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
}
