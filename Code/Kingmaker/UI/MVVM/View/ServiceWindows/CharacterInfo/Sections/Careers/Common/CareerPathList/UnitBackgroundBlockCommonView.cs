using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathList;

public class UnitBackgroundBlockCommonView : ViewBase<UnitBackgroundBlockVM>
{
	[Header("Homeworld")]
	[SerializeField]
	private TextMeshProUGUI m_HomeworldTitle;

	[SerializeField]
	private TextMeshProUGUI m_HomeworldLabel;

	[SerializeField]
	private OwlcatMultiButton m_HomeworldButton;

	[Header("Occupation")]
	[SerializeField]
	private TextMeshProUGUI m_OccupationTitle;

	[SerializeField]
	private TextMeshProUGUI m_OccupationLabel;

	[SerializeField]
	private OwlcatMultiButton m_OccupationButton;

	[Header("MomentOfTriumph")]
	[SerializeField]
	private TextMeshProUGUI m_MomentOfTriumphTitle;

	[SerializeField]
	private TextMeshProUGUI m_MomentOfTriumphLabel;

	[SerializeField]
	private OwlcatMultiButton m_MomentOfTriumphButton;

	[Header("DarkestHour")]
	[SerializeField]
	private TextMeshProUGUI m_DarkestHourTitle;

	[SerializeField]
	private TextMeshProUGUI m_DarkestHourLabel;

	[SerializeField]
	private OwlcatMultiButton m_DarkestHourButton;

	private AccessibilityTextHelper m_TextHelper;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public void Initialize()
	{
		m_TextHelper = new AccessibilityTextHelper(m_HomeworldTitle, m_HomeworldLabel, m_OccupationTitle, m_OccupationLabel, m_MomentOfTriumphTitle, m_MomentOfTriumphLabel, m_DarkestHourTitle, m_DarkestHourLabel);
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Homeworld.Subscribe(delegate(BlueprintFeature f)
		{
			SetBackgroundName(m_HomeworldLabel, f);
		}));
		AddDisposable(base.ViewModel.Occupation.Subscribe(delegate(BlueprintFeature f)
		{
			SetBackgroundName(m_OccupationLabel, f);
		}));
		AddDisposable(base.ViewModel.MomentOfTriumph.Subscribe(delegate(BlueprintFeature f)
		{
			m_MomentOfTriumphButton.gameObject.SetActive(f != null);
			SetBackgroundName(m_MomentOfTriumphLabel, f);
		}));
		AddDisposable(base.ViewModel.DarkestHour.Subscribe(delegate(BlueprintFeature f)
		{
			m_DarkestHourButton.gameObject.SetActive(f != null);
			SetBackgroundName(m_DarkestHourLabel, f);
		}));
		m_HomeworldTitle.text = UIStrings.Instance.CharGen.Homeworld;
		m_OccupationTitle.text = UIStrings.Instance.CharGen.Occupation;
		m_MomentOfTriumphTitle.text = UIStrings.Instance.CharGen.MomentOfTriumph;
		m_DarkestHourTitle.text = UIStrings.Instance.CharGen.DarkestHour;
		AddDisposable(m_HomeworldButton.SetTooltip(base.ViewModel.HomeworldTooltip));
		AddDisposable(m_OccupationButton.SetTooltip(base.ViewModel.OccupationTooltip));
		AddDisposable(m_MomentOfTriumphButton.SetTooltip(base.ViewModel.MomentOfTriumphTooltip));
		AddDisposable(m_DarkestHourButton.SetTooltip(base.ViewModel.DarkestHourTooltip));
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}

	private void SetBackgroundName(TextMeshProUGUI textField, BlueprintFeature feature)
	{
		textField.text = feature?.Name ?? string.Empty;
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		AddDisposable(m_NavigationBehaviour ?? (m_NavigationBehaviour = new GridConsoleNavigationBehaviour()));
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(new SimpleConsoleNavigationEntity(m_HomeworldButton, base.ViewModel.HomeworldTooltip.Value), new SimpleConsoleNavigationEntity(m_OccupationButton, base.ViewModel.OccupationTooltip.Value), new SimpleConsoleNavigationEntity(m_MomentOfTriumphButton, base.ViewModel.MomentOfTriumphTooltip.Value), new SimpleConsoleNavigationEntity(m_DarkestHourButton, base.ViewModel.DarkestHourTooltip.Value));
		return m_NavigationBehaviour;
	}
}
