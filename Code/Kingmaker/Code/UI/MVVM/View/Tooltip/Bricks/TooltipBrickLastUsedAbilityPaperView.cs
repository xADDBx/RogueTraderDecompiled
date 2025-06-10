using Kingmaker.Blueprints.Root.Strings;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickLastUsedAbilityPaperView : TooltipBaseBrickView<TooltipBrickLastUsedAbilityPaperVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	protected Image m_AbilityIcon;

	[SerializeField]
	protected TextMeshProUGUI m_AbilityName;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Text.text = UIStrings.Instance.Tooltips.CycleAbilityLastRetargeted;
		m_AbilityName.text = base.ViewModel.AbilityName;
		m_AbilityIcon.sprite = base.ViewModel.Icon;
	}
}
