using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;

public class CareerPathSelectionsSummaryPCView : BaseCareerPathSelectionTabPCView<CareerPathVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetHeader(null);
		SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
		AddDisposable(base.ViewModel.CanCommit.CombineLatest(base.ViewModel.AllVisited, (bool canCommit, bool allVisited) => canCommit && allVisited).Subscribe(delegate(bool value)
		{
			string nextButtonLabel = (value ? UIStrings.Instance.CharGen.Complete : UIStrings.Instance.CharGen.Next);
			SetNextButtonLabel(nextButtonLabel);
		}));
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
		SetFirstSelectableVisibility(base.ViewModel.HasDifferentFirstSelectable);
		SetFirstSelectableInteractable(base.ViewModel.HasDifferentFirstSelectable);
		SetButtonVisibility(base.ViewModel.IsInLevelupProcess);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		SetButtonVisibility(value: false);
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel.CanCommit.Value && base.ViewModel.AllVisited.Value)
		{
			base.ViewModel.Commit();
			SetButtonVisibility(value: false);
		}
		else
		{
			base.ViewModel.SelectNextItem();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}

	protected override void HandleFirstSelectableClick()
	{
		base.ViewModel.SetFirstSelectableRankEntry();
	}
}
