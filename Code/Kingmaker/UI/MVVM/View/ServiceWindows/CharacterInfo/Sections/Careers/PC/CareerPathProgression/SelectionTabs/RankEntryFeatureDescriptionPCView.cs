using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.Sound;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;

public class RankEntryFeatureDescriptionPCView : BaseCareerPathSelectionTabPCView<RankEntryFeatureItemVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_InfoView.Bind(base.ViewModel.InfoVM);
		AddDisposable(base.ViewModel.CareerPathVM.CanCommit.Subscribe(delegate(bool canCommit)
		{
			bool flag = canCommit && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel;
			SetNextButtonLabel(flag ? UIStrings.Instance.CharacterSheet.ToSummaryTab : UIStrings.Instance.CharGen.Next);
			SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
			SetButtonSound(flag ? UISounds.ButtonSoundsEnum.NormalSound : UISounds.ButtonSoundsEnum.DoctrineNextSound);
		}));
		AddDisposable(base.ViewModel.CareerPathVM.CurrentRank.Subscribe(delegate(int value)
		{
			string header = ((base.ViewModel.Rank.HasValue && base.ViewModel.Rank.Value <= value) ? UIStrings.Instance.CharacterSheet.HeaderFeatureDescriptionTab : UIStrings.Instance.CharacterSheet.HeaderImprovement);
			SetHeader(header);
		}));
		AddDisposable(base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			SetButtonVisibility(!ro);
		}));
		SetFirstSelectableVisibility(base.ViewModel.CareerPathVM.FirstSelectable != null);
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel.CareerPathVM.CanCommit.Value && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
		{
			base.ViewModel.CareerPathVM.SetRankEntry(null);
		}
		else
		{
			base.ViewModel.CareerPathVM.SelectNextItem();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	protected override void HandleFirstSelectableClick()
	{
		base.ViewModel.CareerPathVM.SetFirstSelectableRankEntry();
	}
}
