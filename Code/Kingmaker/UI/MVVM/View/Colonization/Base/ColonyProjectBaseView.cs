using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.VM.Colonization.Projects;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UI.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Colonization.Base;

public class ColonyProjectBaseView : ViewBase<ColonyProjectVM>, IWidgetView
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_OtherProjectIsBuildingLabel;

	[SerializeField]
	private TextMeshProUGUI m_RequirementsNotMetLabel;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private Slider m_ProgressBar;

	[SerializeField]
	private RectTransform m_TooltipPlace;

	public MonoBehaviour MonoBehaviour => this;

	protected override void BindViewImplementation()
	{
		m_OtherProjectIsBuildingLabel.text = UIStrings.Instance.ColonyProjectsTexts.OtherProjectIsBuilding.Text;
		m_RequirementsNotMetLabel.text = UIStrings.Instance.ColonyProjectsTexts.ProjectRequirementsNotMet.Text;
		AddDisposable(base.ViewModel.Title.Subscribe(delegate(string val)
		{
			m_Label.text = val;
		}));
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite val)
		{
			m_Icon.sprite = val;
		}));
		AddDisposable(base.ViewModel.IsExcluded.CombineLatest(base.ViewModel.OtherProjectIsBuilding, base.ViewModel.IsNotMeetRequirements, base.ViewModel.IsFinished, base.ViewModel.IsBuilding, (bool isExcluded, bool otherProjectIsBuilding, bool isNotMeetRequirements, bool isFinished, bool isBuilding) => new { isExcluded, otherProjectIsBuilding, isNotMeetRequirements, isFinished, isBuilding }).Subscribe(value =>
		{
			m_OtherProjectIsBuildingLabel.gameObject.SetActive(!value.isExcluded && !value.isBuilding && value.otherProjectIsBuilding);
			m_RequirementsNotMetLabel.gameObject.SetActive(!value.isExcluded && !value.otherProjectIsBuilding && value.isNotMeetRequirements);
			SetVisualState(value.isExcluded, value.otherProjectIsBuilding, value.isNotMeetRequirements, value.isFinished, value.isBuilding);
		}));
		AddDisposable(base.ViewModel.SegmentsToBuild.Subscribe(delegate(int val)
		{
			m_ProgressBar.maxValue = val;
		}));
		AddDisposable(base.ViewModel.Progress.Subscribe(delegate(int val)
		{
			m_ProgressBar.value = val;
		}));
		AddDisposable(base.ViewModel.IsBuilding.Subscribe(m_ProgressBar.gameObject.SetActive));
		AddDisposable(base.ViewModel.ShouldShow.Subscribe(base.gameObject.SetActive));
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		AddDisposable(m_MultiButton.SetTooltip(new TooltipTemplateColonyProject(base.ViewModel.BlueprintColonyProject, base.ViewModel.Colony), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_TooltipPlace)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void SelectPage()
	{
		base.ViewModel?.SelectPage();
	}

	public void BindWidgetVM(IViewModel vm)
	{
		Bind((ColonyProjectVM)vm);
	}

	public bool CheckType(IViewModel viewModel)
	{
		return viewModel is ColonyProjectVM;
	}

	private void SetVisualState(bool isExcluded, bool otherProjectIsBuilding, bool isNotMeetRequirements, bool isFinished, bool isBuilding)
	{
		string activeLayer = "Default";
		if (isBuilding)
		{
			activeLayer = "InProgress";
		}
		else if (isExcluded)
		{
			activeLayer = "Unavailable";
		}
		else if (otherProjectIsBuilding)
		{
			activeLayer = "UnavailableTemporary";
		}
		else if (isNotMeetRequirements)
		{
			activeLayer = "NotEnoughResources";
		}
		else if (isFinished)
		{
			activeLayer = "Finished";
		}
		m_MultiButton.SetActiveLayer(activeLayer);
	}
}
