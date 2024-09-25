using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Commands;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("b557f8c8bd8e67440b84e93292d2e370")]
public class AbilityActionCamera : BlueprintComponent
{
	private enum CameraTargetType
	{
		Target,
		Caster
	}

	[Header("Сейчас выбор таргет или кастер не влияет ни на что")]
	[SerializeField]
	private CameraTargetType CameraFollow;

	[SerializeField]
	[Range(0f, 100f)]
	private int TriggerActionCameraChance = 30;

	public AbilityActionCameraSettings GetSettings(UnitUseAbility abilityCommand)
	{
		Transform caster = abilityCommand.Executor?.View?.ViewTransform;
		Transform target = abilityCommand.Target?.Entity?.View?.ViewTransform;
		return new AbilityActionCameraSettings(caster, target, TriggerActionCameraChance);
	}
}
