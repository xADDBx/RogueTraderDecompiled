using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Assets.Designers.EventConditionActionSystem.Conditions;

[TypeId("35f771a6518258c40a363e43b96ed56c")]
public class BarkBanterPlayed : Condition
{
	[SerializeField]
	[FormerlySerializedAs("Banter")]
	private BlueprintBarkBanterReference m_Banter;

	public BlueprintBarkBanter Banter => m_Banter?.Get();

	protected override string GetConditionCaption()
	{
		return $"Bark Banter Played ({Banter})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.Player.PlayedBanters.Contains(Banter);
	}
}
