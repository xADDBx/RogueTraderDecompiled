using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.QA;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("cea12929263741faadf5bfc2bcfb92d2")]
public class PretendUnit : UnitFactComponentDelegate, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		[JsonProperty]
		public bool Applied { get; set; }
	}

	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitReference m_Unit;

	public BlueprintUnit Unit => m_Unit?.Get();

	protected override void OnActivate()
	{
		if (base.Owner.Blueprint != base.Owner.OriginalBlueprint)
		{
			PFLog.Default.ErrorWithReport("PretendUnit.OnActivate: Owner.Blueprint != Owner.OriginalBlueprint");
			return;
		}
		base.Owner.SetFakeBlueprint(Unit);
		RequestTransientData<ComponentData>().Applied = true;
	}

	protected override void OnDeactivate()
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.Applied)
		{
			base.Owner.SetFakeBlueprint(null);
			componentData.Applied = false;
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
