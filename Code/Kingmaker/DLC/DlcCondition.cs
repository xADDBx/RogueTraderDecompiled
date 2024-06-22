using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.DLC;

[TypeId("24656828b2e6440fa945c36b4e854c1e")]
public class DlcCondition : BlueprintComponent
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintDlcRewardReference m_DlcReward;

	[SerializeField]
	private bool m_HideInstead;

	public BlueprintDlcReward DlcReward => m_DlcReward;

	public bool HideInstead => m_HideInstead;

	public bool IsFullFilled()
	{
		return m_HideInstead ^ DlcReward.IsActive;
	}
}
