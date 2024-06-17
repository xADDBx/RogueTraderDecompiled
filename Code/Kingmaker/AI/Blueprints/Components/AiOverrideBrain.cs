using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Blueprints.Components;

[TypeId("fc89cb965d6937143bee436ce34948d7")]
public class AiOverrideBrain : EntityFactComponentDelegate, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public BlueprintBrainBaseReference OriginalBrainRef;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(OriginalBrainRef);
			result.Append(ref val2);
			return result;
		}
	}

	[SerializeField]
	private BlueprintBrainBaseReference m_Brain;

	public BlueprintBrainBase Brain => m_Brain?.Get();

	protected override void OnActivate()
	{
		PartUnitBrain optional = base.Owner.GetOptional<PartUnitBrain>();
		RequestSavableData<SavableData>().OriginalBrainRef = optional?.Blueprint?.ToReference<BlueprintBrainBaseReference>() ?? null;
		optional.SetBrain(Brain);
	}

	protected override void OnApplyPostLoadFixes()
	{
		PartUnitBrain optional = base.Owner.GetOptional<PartUnitBrain>();
		RequestSavableData<SavableData>().OriginalBrainRef = optional?.Blueprint?.ToReference<BlueprintBrainBaseReference>() ?? null;
		optional.SetBrain(Brain);
	}

	protected override void OnDeactivate()
	{
		PartUnitBrain optional = base.Owner.GetOptional<PartUnitBrain>();
		SavableData savableData = RequestSavableData<SavableData>();
		optional.SetBrain(savableData.OriginalBrainRef);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
