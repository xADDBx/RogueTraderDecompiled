using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;

[TypeId("8fb5bcce71094cbf9fd312c67dff7e86")]
public class AbilityTargetCanTurn : BlueprintComponent, IAbilityTargetRestriction
{
	[SerializeField]
	private PropertyCalculator Angle;

	[SerializeField]
	private int MaximalTargetInertiaToApplyLowInertiaAngle = -1;

	[SerializeField]
	private PropertyCalculator LowInertiaAngle;

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		StarshipEntity starshipEntity = target.Entity as StarshipEntity;
		int resultOrientation = GetResultOrientation(ability, starshipEntity, out var _);
		resultOrientation = GetAlignedOrientation(resultOrientation);
		return CheckPositionAfterRotation(starshipEntity, resultOrientation);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.UnavailableGeneric;
	}

	public static bool CheckPositionAfterRotation(StarshipEntity entity, int desiredOrientation)
	{
		return UnitHelper.FindPositionForUnit(entity, desiredOrientation) != null;
	}

	private int GetResultOrientation(AbilityData ability, StarshipEntity target, out int angle)
	{
		AbilityExecutionContext mechanicContext = ability.CreateExecutionContext(target);
		PropertyContext context = new PropertyContext(ability.Caster, null, null, mechanicContext);
		angle = (((int)target.Stats.GetStat(StatType.Inertia) > MaximalTargetInertiaToApplyLowInertiaAngle) ? Angle.GetValue(context) : LowInertiaAngle.GetValue(context));
		return (int)target.Orientation + angle;
	}

	private int GetAlignedOrientation(int orientation)
	{
		return (orientation + 360 + 22) / 45 * 45 % 360;
	}
}
