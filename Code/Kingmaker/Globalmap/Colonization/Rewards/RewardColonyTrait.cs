using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Blueprints.Quests;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("025c75e262144576a8d4c9ac67917352")]
[AllowedOn(typeof(BlueprintColonyProject))]
[AllowedOn(typeof(BlueprintQuestContract))]
public class RewardColonyTrait : Reward
{
	[SerializeField]
	private BlueprintColonyTrait.Reference m_Trait;

	[SerializeField]
	private bool m_ApplyToAllColonies;

	[SerializeField]
	[ShowIf("ShouldSpecifyColony")]
	private BlueprintColonyReference m_SpecificColony;

	public BlueprintColonyTrait Trait => m_Trait?.Get();

	public bool ApplyToAllColonies => m_ApplyToAllColonies;

	private bool ShouldSpecifyColony
	{
		get
		{
			if (!(base.OwnerBlueprint is BlueprintColonyProject))
			{
				return !m_ApplyToAllColonies;
			}
			return false;
		}
	}

	private BlueprintColony SpecificColony => m_SpecificColony?.Get();

	public override void ReceiveReward(Colony colony = null)
	{
		if (Trait == null)
		{
			PFLog.Default.Error("Empty trait in RewardColonyTrait");
			return;
		}
		ColoniesState coloniesState = Game.Instance.Player.ColoniesState;
		if (m_ApplyToAllColonies)
		{
			coloniesState.TraitsForAllColonies.Add(Trait);
			{
				foreach (ColoniesState.ColonyData colony2 in coloniesState.Colonies)
				{
					colony2.Colony.AddTrait(Trait);
				}
				return;
			}
		}
		if (colony != null)
		{
			colony.AddTrait(Trait);
		}
		else if (SpecificColony != null)
		{
			(coloniesState.Colonies.FirstOrDefault((ColoniesState.ColonyData data) => data.Colony.Blueprint == SpecificColony)?.Colony)?.AddTrait(Trait);
		}
	}
}
