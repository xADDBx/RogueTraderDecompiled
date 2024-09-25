using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Selectable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickPortraitAndNameView : TooltipBaseBrickView<TooltipBrickPortraitAndNameVM>
{
	[SerializeField]
	protected OwlcatMultiSelectable m_MultiSelectableIcon;

	[SerializeField]
	protected OwlcatMultiSelectable m_MultiSelectableBorderAndDifficulty;

	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	protected TooltipBrickTitleView m_TitleView;

	[SerializeField]
	protected TextMeshProUGUI m_DifficultyText;

	[SerializeField]
	private float m_DefaultFontSize = 24f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 24f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Line;
		TooltipBrickTitleVM tooltipBrickTitleVM = base.ViewModel.BrickTitle?.GetVM() as TooltipBrickTitleVM;
		AddDisposable(tooltipBrickTitleVM);
		m_TitleView.Bind(tooltipBrickTitleVM);
		m_TitleView.gameObject.SetActive(base.ViewModel.BrickTitle != null);
		string text = (base.ViewModel.IsEnemy ? "Enemy" : (base.ViewModel.IsFriend ? "Friend" : "Default"));
		m_MultiSelectableBorderAndDifficulty.SetActiveLayer(text);
		m_MultiSelectableIcon.SetActiveLayer(base.ViewModel.IsUsedSubtypeIcon ? text : "Default");
		m_DifficultyText.gameObject.SetActive(base.ViewModel.Difficulty > 0);
		m_DifficultyText.text = UIUtility.ArabicToRoman(base.ViewModel.Difficulty);
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * FontMultiplier;
	}
}
