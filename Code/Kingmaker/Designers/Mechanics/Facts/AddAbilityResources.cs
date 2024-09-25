using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add ability resources")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("fd82ca085bd60c04fb03d1091acc66cb")]
public class AddAbilityResources : UnitFactComponentDelegate, IUnitReapplyFeaturesOnLevelUpHandler<EntitySubscriber>, IUnitReapplyFeaturesOnLevelUpHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitReapplyFeaturesOnLevelUpHandler, EntitySubscriber>, IHashable
{
	public bool UseThisAsResource;

	[HideIf("UseThisAsResource")]
	[SerializeField]
	[FormerlySerializedAs("Resource")]
	private BlueprintAbilityResourceReference m_Resource;

	[ShowIf("UseThisAsResource")]
	public int Amount;

	public bool RestoreAmount;

	public bool RestoreOnLevelUp;

	public BlueprintAbilityResource Resource => m_Resource?.Get();

	protected override void OnActivate()
	{
		BlueprintScriptableObject blueprint = (UseThisAsResource ? ((BlueprintScriptableObject)base.Fact.Blueprint) : ((BlueprintScriptableObject)Resource));
		if (!base.IsReapplying)
		{
			base.Owner.AbilityResources.Add(blueprint, RestoreAmount);
		}
	}

	protected override void OnDeactivate()
	{
		BlueprintScriptableObject blueprint = (UseThisAsResource ? ((BlueprintScriptableObject)base.Fact.Blueprint) : ((BlueprintScriptableObject)Resource));
		if (!base.IsReapplying)
		{
			base.Owner.AbilityResources.Remove(blueprint);
		}
	}

	public void HandleUnitReapplyFeaturesOnLevelUp()
	{
		if (RestoreOnLevelUp)
		{
			BlueprintScriptableObject blueprint = (UseThisAsResource ? ((BlueprintScriptableObject)base.Fact.Blueprint) : ((BlueprintScriptableObject)Resource));
			base.Owner.AbilityResources.Restore(blueprint);
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
