using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.RewardWindows;

public class SoulMarkRewardPCView : SoulMarkRewardBaseView
{
	[Header("PC Part")]
	[SerializeField]
	protected OwlcatButton m_AcceptButton;

	[SerializeField]
	protected TextMeshProUGUI m_AcceptText;

	[SerializeField]
	protected OwlcatButton m_DeclineButton;

	[SerializeField]
	protected TextMeshProUGUI m_DeclineText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		TextHelper.AppendTexts(m_AcceptText, m_DeclineText);
		AddDisposable(m_AcceptButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnAcceptPressed();
		}));
		AddDisposable(m_DeclineButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.OnDeclinePressed();
		}));
		AddDisposable(m_MainButton.SetTooltip(base.ViewModel.Tooltip));
		m_AcceptText.text = UIStrings.Instance.PopUps.SeeOtherRanks;
		m_DeclineText.text = UIStrings.Instance.CommonTexts.Accept;
	}
}
