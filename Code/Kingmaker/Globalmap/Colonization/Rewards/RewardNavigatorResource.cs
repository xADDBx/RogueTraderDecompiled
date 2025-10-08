using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[TypeId("4cff9d7173864dc287b3c3da6030219e")]
public class RewardNavigatorResource : Reward
{
	[SerializeField]
	private int m_NavigationResourceCount;

	public int NavigatorResourceCount => m_NavigationResourceCount;

	public override void ReceiveReward(Colony colony = null)
	{
		Game.Instance.SectorMapController.ChangeNavigatorResourceCount(m_NavigationResourceCount);
	}
}
