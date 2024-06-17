using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Parts;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("2fcb3a86db6cbf846bb0252ef04ba9b8")]
public class SpellImmunityToSpellDescriptor : UnitFactComponentDelegate, IHashable
{
	public class ComponentData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public int? AppliedId;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			if (AppliedId.HasValue)
			{
				int val2 = AppliedId.Value;
				result.Append(ref val2);
			}
			return result;
		}
	}

	public SpellDescriptorWrapper Descriptor;

	[SerializeField]
	private BlueprintUnitFactReference m_CasterIgnoreImmunityFact;

	public BlueprintUnitFact CasterIgnoreImmunityFact => m_CasterIgnoreImmunityFact?.Get();

	protected override void OnActivate()
	{
		RequestSavableData<ComponentData>().AppliedId = base.Owner.GetOrCreate<UnitPartSpellResistance>().AddImmunity(SpellImmunityType.SpellDescriptor, null, Descriptor, CasterIgnoreImmunityFact);
	}

	protected override void OnDeactivate()
	{
		ComponentData componentData = RequestSavableData<ComponentData>();
		if (componentData.AppliedId.HasValue)
		{
			base.Owner.GetOptional<UnitPartSpellResistance>()?.Remove(componentData.AppliedId.Value);
			componentData.AppliedId = null;
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
