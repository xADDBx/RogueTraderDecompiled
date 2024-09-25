using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;

public class CharInfoProfitFactorItemPCView : CharInfoProfitFactorItemBaseView
{
	[Header("PC")]
	[SerializeField]
	private TextMeshProUGUI m_ButtonLabel;

	[SerializeField]
	private OwlcatButton m_InfoButton;

	[SerializeField]
	private OwlcatMultiButton m_ValueContainer;

	private AccessibilityTextHelper m_PCTextHelper;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ButtonLabel.text = UIStrings.Instance.ContextMenu.Information;
		AddDisposable(m_InfoButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			if (RootUIContext.Instance.CommonVM.TooltipContextVM.InfoWindowVM.Value?.MainTemplate == Tooltip)
			{
				TooltipHelper.HideInfo();
			}
			else
			{
				TooltipHelper.ShowInfo(Tooltip);
			}
		}));
		TextHelper.AppendTexts(m_ButtonLabel);
	}
}
