using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization.Rewards;

[AllowedOn(typeof(BlueprintColonyProject))]
[AllowMultipleComponents]
[TypeId("f1311382123a4b06a9954dae6bc822e3")]
public class RewardResourceProject : Reward
{
	[SerializeField]
	private ResourceData m_Resource;

	public BlueprintResource Resource => m_Resource?.Resource?.Get();

	public int Count => m_Resource?.Count ?? 0;

	public override void ReceiveReward(Colony colony = null)
	{
		if (colony != null && m_Resource != null)
		{
			BlueprintColonyProject projectBp = base.OwnerBlueprint as BlueprintColonyProject;
			colony.ProduceResource(projectBp, m_Resource);
		}
	}
}
