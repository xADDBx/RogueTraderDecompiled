using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("649df37d87fcf314cb863c9a18113fa5")]
public class ContextRestoreResource : ContextAction
{
	public bool m_IsFullRestoreAllResources;

	[SerializeField]
	[FormerlySerializedAs("Resource")]
	[HideIf("m_IsFullRestoreAllResources")]
	private BlueprintAbilityResourceReference m_Resource;

	[HideIf("m_IsFullRestoreAllResources")]
	public bool ContextValueRestoration;

	[ShowIf("ContextValueRestoration")]
	public ContextValue Value;

	public bool RestoreToSummonerUnit;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public override string GetCaption()
	{
		if (m_IsFullRestoreAllResources)
		{
			return "Restore all resources";
		}
		return string.Format("Restore {0} of {1}", Value, m_Resource?.Get()?.name ?? "<null>");
	}

	public override void RunAction()
	{
		MechanicEntity mechanicEntity = ((!RestoreToSummonerUnit) ? base.Context.MainTarget?.Entity : base.Context.MainTarget?.Entity?.GetOptional<UnitPartSummonedMonster>()?.Summoner);
		if (mechanicEntity == null)
		{
			PFLog.Default.Error("Target is missing");
			return;
		}
		PartAbilityResourceCollection abilityResourcesOptional = mechanicEntity.GetAbilityResourcesOptional();
		if (abilityResourcesOptional != null)
		{
			if (m_IsFullRestoreAllResources)
			{
				abilityResourcesOptional.FullRestoreAll();
			}
			else if (!ContextValueRestoration)
			{
				abilityResourcesOptional.Restore(Resource, 1);
			}
			else
			{
				abilityResourcesOptional.Restore(Resource, Value.Calculate(base.Context));
			}
		}
	}
}
