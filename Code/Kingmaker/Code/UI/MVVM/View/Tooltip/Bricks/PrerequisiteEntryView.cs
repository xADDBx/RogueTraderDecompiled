using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class PrerequisiteEntryView : ViewBase<PrerequisiteEntryVM>, IWidgetView, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private OwlcatMultiButton m_Focus;

	[Header("Colors")]
	[SerializeField]
	private Color32 m_DoneBGColor;

	[SerializeField]
	private Color32 m_RequiredBGColor;

	[SerializeField]
	private Color32 m_DoneTextColor;

	[SerializeField]
	private Color32 m_RequiredTextColor;

	[SerializeField]
	private float m_DefaultFontSizeText = 18f;

	[SerializeField]
	private float m_DefaultFontSizeValue = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeText = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSizeValue = 22f;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_Text.text = base.ViewModel.Text;
		m_Value.text = base.ViewModel.Value;
		TextMeshProUGUI text = m_Text;
		Color color2 = (m_Value.color = (base.ViewModel.Done ? m_DoneTextColor : m_RequiredTextColor));
		text.color = color2;
		m_Background.color = (base.ViewModel.Done ? m_DoneBGColor : m_RequiredBGColor);
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_Text.fontSize = (isControllerMouse ? m_DefaultFontSizeText : m_DefaultConsoleFontSizeText) * base.ViewModel.FontMultiplier;
		m_Value.fontSize = (isControllerMouse ? m_DefaultFontSizeValue : m_DefaultConsoleFontSizeValue) * base.ViewModel.FontMultiplier;
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind(vm as PrerequisiteEntryVM);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is PrerequisiteEntryVM;
	}

	public void SetFocus(bool value)
	{
		m_Focus.Or(null)?.SetFocus(value);
	}

	public bool IsValid()
	{
		if (m_Focus != null)
		{
			return m_Focus.IsValid();
		}
		return false;
	}
}
