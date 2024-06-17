using DG.Tweening;
using Kingmaker.Code.UI.MVVM.VM.Vendor;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorReputationLevelView : VirtualListElementViewBase<VendorReputationLevelVM>
{
	[SerializeField]
	private TextMeshProUGUI m_ReputationPointsLabel;

	[SerializeField]
	private TextMeshProUGUI m_LevelLabel;

	[SerializeField]
	protected GameObject m_LockedObject;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	[SerializeField]
	private DOTweenAnimation m_Highlight;

	protected override void BindViewImplementation()
	{
		m_ReputationPointsLabel.text = base.ViewModel.ReputationPoints.ToString();
		m_LevelLabel.text = base.ViewModel.ReputationLevel.ToString();
		m_LockedObject.SetActive(base.ViewModel.Locked);
		m_Button.SetActiveLayer(base.ViewModel.Locked ? 1 : 0);
		AddDisposable(base.ViewModel.OnHighlight.Subscribe(delegate
		{
			m_Highlight.DOPlay();
		}));
	}

	protected override void DestroyViewImplementation()
	{
	}
}
