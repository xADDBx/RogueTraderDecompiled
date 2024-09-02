using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Visual.Animation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b2a17e3404004ded87b8179ff8634ef2")]
public class OverrideAnimationSet : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private AnimationSet m_AnimationSetMale;

	[SerializeField]
	private AnimationSet m_AnimationSetFemale;

	public AnimationSet AnimationSetMale => m_AnimationSetMale;

	public AnimationSet AnimationSeFemale => m_AnimationSetFemale;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<UnitPartVisualChange>().AnimationSetOverride = ((base.Owner.Gender == Gender.Male) ? AnimationSetMale : AnimationSeFemale);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitPartVisualChange>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
