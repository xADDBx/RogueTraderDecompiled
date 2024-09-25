using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Dialog.RewardWindows;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.RewardWindows;

public abstract class SoulMarkRewardBaseView : ViewBase<SoulMarkRewardVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_FeatureName;

	[SerializeField]
	protected OwlcatMultiButton m_MainButton;

	protected AccessibilityTextHelper TextHelper;

	protected override void BindViewImplementation()
	{
		TextHelper = new AccessibilityTextHelper(m_Title, m_FeatureName);
		m_Title.text = UIStrings.Instance.PopUps.SoulMarkRewardTitle;
		m_FeatureName.text = base.ViewModel.FeatureName;
		m_Icon.sprite = base.ViewModel.FeatureIcon;
		TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		TextHelper.Dispose();
	}
}
