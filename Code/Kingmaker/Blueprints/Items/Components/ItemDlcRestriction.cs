using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DLC;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItem))]
[TypeId("2d6660c186dc4694b6b4374977d84557")]
public class ItemDlcRestriction : BlueprintComponent
{
	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	[SerializeField]
	[FormerlySerializedAs("ChangeTo")]
	private BlueprintItemReference m_ChangeTo;

	public bool HideInVendors = true;

	public BlueprintDlcReward DlcReward => m_DlcReward;

	public BlueprintItem ChangeTo => m_ChangeTo?.Get();

	public bool IsRestricted => !(DlcReward?.IsAvailable ?? true);
}
