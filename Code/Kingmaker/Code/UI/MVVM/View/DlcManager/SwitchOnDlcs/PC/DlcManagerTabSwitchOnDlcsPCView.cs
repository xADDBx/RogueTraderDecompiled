using Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.DlcManager.SwitchOnDlcs.PC;

public class DlcManagerTabSwitchOnDlcsPCView : DlcManagerTabSwitchOnDlcsBaseView
{
	[Header("PC Part")]
	[SerializeField]
	private DlcManagerTabSwitchOnDlcsDlcSelectorPCView m_DlcSelectorPCView;

	[SerializeField]
	private float m_DefaultFontInstalledDlcsHeaderPCSize = 26f;

	[SerializeField]
	private float m_DefaultFontYouDontHaveAnyDlcsPCSize = 22f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DlcSelectorPCView.Bind(base.ViewModel.SelectionGroup);
	}

	protected override void SetTextFontSize(float multiplier)
	{
		base.SetTextFontSize(multiplier);
		m_InstalledDlcsHeaderLabel.fontSize = m_DefaultFontInstalledDlcsHeaderPCSize * multiplier;
		m_YouDontHaveAnyDlcsLabel.fontSize = m_DefaultFontYouDontHaveAnyDlcsPCSize * multiplier;
	}
}
