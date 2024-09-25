using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbilityAreaEffect))]
[TypeId("cb2b03882dc24aeb8fe64ff7fd35f8d8")]
public class AreaEffectClusterComponent : MechanicEntityFactComponentDelegate, IHashable
{
	[InfoBox("Presence of AreaEffectClusterComponent overrides all other components. For overriden logic look at ClusterLogicBlueprint field")]
	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintAbilityAreaEffectClusterLogicReference m_ClusterLogicBlueprint;

	public BlueprintAbilityAreaEffectClusterLogic ClusterLogicBlueprint => m_ClusterLogicBlueprint.Get();

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
