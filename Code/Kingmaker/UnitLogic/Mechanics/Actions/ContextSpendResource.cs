using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility.Attributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("a933c328a94be1e4a95d7ce1d9dc2b8a")]
public class ContextSpendResource : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	public bool ContextValueSpendure;

	[ShowIf("ContextValueSpendure")]
	public ContextValue Value;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	public override string GetCaption()
	{
		return "Spend resourse";
	}

	protected override void RunAction()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster == null)
		{
			Element.LogError(this, "Caster is missing");
			return;
		}
		PartAbilityResourceCollection abilityResourcesOptional = maybeCaster.GetAbilityResourcesOptional();
		if (abilityResourcesOptional != null)
		{
			if (!ContextValueSpendure)
			{
				abilityResourcesOptional.Spend(Resource, 1);
			}
			else
			{
				abilityResourcesOptional.Spend(Resource, Value.Calculate(base.Context));
			}
		}
	}
}
