using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Parts;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[KDB("Можно ли устанавливать аугменты выбранного тира. Другими словами, проапгрейдили ли мы кресло для установки аугментов этого тира.")]
[TypeId("75fd2bd597112924e96b1c031ad93ef8")]
public class AvailableAugmentTierToInstall : Condition
{
	[SerializeField]
	private AugmentTier m_Tier;

	protected override string GetConditionCaption()
	{
		return "Available Augment Tier To Install";
	}

	protected override bool CheckCondition()
	{
		return !Game.Instance.Player.PartyAugmentManager.CanEquipAugment(m_Tier);
	}
}
