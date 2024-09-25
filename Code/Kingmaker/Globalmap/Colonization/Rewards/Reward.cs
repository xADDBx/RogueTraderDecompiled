using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[TypeId("e9b07d76cb804c09b369deacfa0d1f40")]
[AllowedOn(typeof(BlueprintColonyChronicle))]
[AllowedOn(typeof(BlueprintColonyEventResult))]
public abstract class Reward : BlueprintComponent
{
	public bool HideInUI;

	private RewardUI m_RewardUI;

	public abstract void ReceiveReward([CanBeNull] Colony colony = null);

	protected BlueprintScriptableObject RewardOwner()
	{
		if (base.OwnerBlueprint is BlueprintColonyEventResult blueprintColonyEventResult)
		{
			return blueprintColonyEventResult.OwnerEvent;
		}
		return base.OwnerBlueprint;
	}
}
