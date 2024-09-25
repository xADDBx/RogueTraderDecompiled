using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;

public class CharInfoCurrentCareerView : ViewBase<CareerPathVM>, IConsoleNavigationEntity, IConsoleEntity, IHasTooltipTemplate
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TextMeshProUGUI m_CareerLabel;

	[SerializeField]
	private TextMeshProUGUI m_CareerName;

	[SerializeField]
	private TextMeshProUGUI m_CareerValue;

	[SerializeField]
	private Image m_Icon;

	protected override void BindViewImplementation()
	{
		m_CareerLabel.text = UIStrings.Instance.CharacterSheet.Career;
		m_CareerName.text = base.ViewModel.Name;
		TextMeshProUGUI careerValue = m_CareerValue;
		string text = base.ViewModel.CurrentRank.Value.ToString();
		int maxRank = base.ViewModel.MaxRank;
		careerValue.text = text + "/" + maxRank;
		m_Icon.sprite = base.ViewModel.Icon.Value;
		AddDisposable(this.SetTooltip(base.ViewModel.CareerTooltip));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public TooltipBaseTemplate TooltipTemplate()
	{
		return base.ViewModel.CareerTooltip;
	}
}
