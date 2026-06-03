using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Items;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("5619d2e77313463a863b22c89f576c65")]
public class UnlockSpecialAugmentSlots : UnitFactComponentDelegate, IHashable
{
	[ValidateNotEmpty]
	[SerializeField]
	private BlueprintAugmentSlotReference[] m_Slots;

	protected override void OnActivateOrPostLoad()
	{
		if (m_Slots == null)
		{
			return;
		}
		UnitAugments augments = base.Owner.Body.Augments;
		if (augments == null)
		{
			return;
		}
		BlueprintAugmentSlotReference[] slots = m_Slots;
		foreach (BlueprintAugmentSlotReference blueprintAugmentSlotReference in slots)
		{
			if (blueprintAugmentSlotReference != null)
			{
				augments.RetainSpecialSlot(blueprintAugmentSlotReference);
			}
		}
	}

	protected override void OnDeactivate()
	{
		if (m_Slots == null)
		{
			return;
		}
		UnitAugments augments = base.Owner.Body.Augments;
		if (augments != null)
		{
			BlueprintAugmentSlotReference[] slots = m_Slots;
			foreach (BlueprintAugmentSlotReference blueprintAugmentSlotReference in slots)
			{
				augments.ReleaseSpecialSlot(blueprintAugmentSlotReference);
			}
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
