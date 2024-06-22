using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
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
		SetNextButtonLabel(UIStrings.Instance.CharGen.Next);
		SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
		SetFinishButtonLabel(UIStrings.Instance.Tutorial.Complete);
		SetNextButtonInteractable(value: true);
		SetBackButtonInteractable(value: false);
		AddDisposable(base.ViewModel.CanCommit.CombineLatest(base.ViewModel.PointerItem, (bool canCommit, IRankEntrySelectItem pointerItem) => canCommit && pointerItem == null).Subscribe(delegate(bool value)
		{
			base.CanCommit = value;
			SetFinishInteractable(value);
		}));
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
		SetButtonVisibility(base.ViewModel.IsInLevelupProcess);
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		SetButtonVisibility(value: false);
	}

	public override void UpdateState()
	{
		SetButtonVisibility(base.ViewModel.IsInLevelupProcess && (!base.ViewModel.IsSelected.Value || base.ViewModel.CurrentRank.Value != 0));
	}

	protected override void HandleClickNext()
	{
		base.ViewModel.SelectNextItem();
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}

	protected override void HandleClickFinish()
	{
		base.ViewModel.Commit();
	}
}
