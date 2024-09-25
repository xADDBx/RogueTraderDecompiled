using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic.Levelup;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;

public class CareerPathDescriptionPCView : BaseCareerPathSelectionTabPCView<CareerPathVM>
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
		SetButtonSound(UISounds.ButtonSoundsEnum.DoctrineNextSound);
		base.ViewModel.OnUpdateData.Subscribe(delegate
		{
			if (base.ViewModel != null)
			{
				SetButtonVisibility(base.ViewModel.IsInLevelupProcess && (!base.ViewModel.IsSelected.Value || base.ViewModel.CurrentRank.Value != 0));
			}
		});
		AddDisposable(base.ViewModel.IsSelected.CombineLatest(base.ViewModel.CurrentRank, (bool selected, int rank) => selected && rank == 0).Subscribe(delegate(bool value)
		{
			SetButtonVisibility(!value && base.ViewModel.IsInLevelupProcess);
		}));
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	public override void UpdateState()
	{
		SetBackButtonInteractable(value: false);
		SetNextButtonInteractable(value: true);
		HintText.Value = GetHintText();
	}

	protected override void HandleClickNext()
	{
		if (base.IsBinded)
		{
			base.ViewModel.SelectNextItem();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}

	protected override void HandleClickFinish()
	{
		base.ViewModel.Commit();
	}

	private string GetHintText()
	{
		if (base.ViewModel.IsAvailableToUpgrade)
		{
			return string.Empty;
		}
		if (base.ViewModel.Unit.IsInCombat)
		{
			return UIStrings.Instance.CharacterSheet.UnitIsInCombatButtonHint.Text;
		}
		LevelUpManager levelUpManager = base.ViewModel.UnitProgressionVM.LevelUpManager;
		if (levelUpManager != null)
		{
			if (levelUpManager.TargetUnit != base.ViewModel.Unit)
			{
				return UIStrings.Instance.CharacterSheet.LevelUpOnOtherUnitButtonHint.Text;
			}
			if (levelUpManager.Path == base.ViewModel.CareerPath)
			{
				return string.Empty;
			}
		}
		return UIStrings.Instance.CharacterSheet.NoRanksForUpgradeButtonHint.Text;
	}
}
