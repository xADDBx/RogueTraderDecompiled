using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.CharacterGender;
using Kingmaker.UnitLogic.Levelup.Selections.Doll;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance;

public class CharGenAppearanceComponentAppearancePhaseVM : CharGenPhaseBaseVM, ILevelUpDollHandler, ISubscriber, ICharGenPortraitSelectorHoverHandler, ICharGenAppearancePhaseHandler, ICharGenAppearanceComponentUpdateHandler
{
	public readonly SelectionGroupRadioVM<CharGenAppearancePageVM> PagesSelectionGroupRadioVM;

	public readonly ReactiveProperty<CharGenAppearancePageVM> CurrentPageVM = new ReactiveProperty<CharGenAppearancePageVM>();

	public readonly ReactiveCollection<VirtualListElementVMBase> VirtualListCollection = new ReactiveCollection<VirtualListElementVMBase>();

	public readonly ReactiveProperty<CharGenPortraitVM> PortraitVM = new ReactiveProperty<CharGenPortraitVM>();

	public readonly ReactiveCommand<CharGenAppearancePageType> OnPageChanged = new ReactiveCommand<CharGenAppearancePageType>();

	private readonly ReactiveCollection<CharGenAppearancePageVM> m_Pages = new ReactiveCollection<CharGenAppearancePageVM>();

	private SelectionStateDoll m_SelectionStateDoll;

	private SelectionStateGender m_SelectionStateGender;

	private bool m_Subscribed;

	private CompositeDisposable m_UpdateComponentsSubscription;

	public readonly BoolReactiveProperty CurrentPageIsFirst = new BoolReactiveProperty();

	public readonly BoolReactiveProperty CurrentPageIsLast = new BoolReactiveProperty();

	public DollState DollState => CharGenContext.Doll;

	public CharGenAppearanceComponentAppearancePhaseVM(CharGenContext charGenContext)
		: base(charGenContext, CharGenPhaseType.Appearance)
	{
		PagesSelectionGroupRadioVM = AddDisposableAndReturn(new SelectionGroupRadioVM<CharGenAppearancePageVM>(m_Pages, CurrentPageVM));
		AddDisposable(CurrentPageVM.Subscribe(delegate(CharGenAppearancePageVM value)
		{
			CurrentPageIsFirst.Value = m_Pages.FirstOrDefault() == value;
			CurrentPageIsLast.Value = m_Pages.LastOrDefault() == value;
		}));
		AddDisposable(CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
	}

	protected override void DisposeImplementation()
	{
		base.DisposeImplementation();
		m_Pages.Clear();
		VirtualListCollection.Clear();
		ClearPortrait();
	}

	protected override bool CheckIsCompleted()
	{
		if (base.IsInDetailedView.Value)
		{
			SelectionStateDoll selectionStateDoll = m_SelectionStateDoll;
			if (selectionStateDoll != null && selectionStateDoll.IsMade)
			{
				return selectionStateDoll.IsValid;
			}
			return false;
		}
		return false;
	}

	protected override void OnBeginDetailedView()
	{
		CurrentPageVM.Value?.BeginPageView();
		UpdateVisualSettings();
		ApplyDollState(CharGenContext.Doll);
		if (!m_Subscribed)
		{
			CreatePages();
			PagesSelectionGroupRadioVM.TrySelectFirstValidEntity();
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(CurrentPageVM.Subscribe(OnCurrentPageChanged));
			m_Subscribed = true;
		}
	}

	private void CreatePages()
	{
		foreach (CharGenAppearancePageType item in CharGenAppearancePages.PagesOrder.Where(IsPageEnabled))
		{
			CharGenAppearancePageVM disposable = new CharGenAppearancePageVM(CharGenContext, item, base.IsInDetailedView);
			m_Pages.Add(AddDisposableAndReturn(disposable));
		}
		foreach (CharGenAppearancePageVM page in m_Pages)
		{
			if (page.PageType != CharGenAppearancePageType.General)
			{
				page.CreateComponentsIfNeeded();
			}
		}
	}

	private bool IsPageEnabled(CharGenAppearancePageType pageType)
	{
		if (pageType != CharGenAppearancePageType.NavigatorMutations)
		{
			return true;
		}
		CharGenConfig charGenConfig = CharGenContext.CharGenConfig;
		if (charGenConfig.Mode == CharGenConfig.CharGenMode.NewCompanion)
		{
			return charGenConfig.CompanionType == CharGenConfig.CharGenCompanionType.Navigator;
		}
		return false;
	}

	private void OnCurrentPageChanged(CharGenAppearancePageVM pageVM)
	{
		if (pageVM != null)
		{
			Game.Instance.GameCommandQueue.CharGenChangeAppearancePage(pageVM.PageType);
		}
	}

	void ICharGenAppearancePhaseHandler.HandleAppearancePageChange(CharGenAppearancePageType pageType)
	{
		ClearPortrait();
		VirtualListCollection.Clear();
		CharGenAppearancePageVM charGenAppearancePageVM = m_Pages.FirstOrDefault((CharGenAppearancePageVM p) => p.PageType == pageType);
		if (charGenAppearancePageVM == null)
		{
			PFLog.UI.Error($"CharGenAppearancePageVM not found {pageType}");
			return;
		}
		if (!UINetUtility.IsControlMainCharacter())
		{
			CurrentPageVM.Value = charGenAppearancePageVM;
		}
		charGenAppearancePageVM.BeginPageView();
		foreach (BaseCharGenAppearancePageComponentVM component in charGenAppearancePageVM.Components)
		{
			VirtualListCollection.Add(component);
		}
		OnPageChanged.Execute(charGenAppearancePageVM.PageType);
		UpdateVisualSettings();
	}

	private void ApplyDollState(DollState dollState)
	{
		if (dollState != null)
		{
			m_SelectionStateDoll?.Select(dollState);
		}
		UpdateIsCompleted();
	}

	private void Clear()
	{
		m_UpdateComponentsSubscription?.Dispose();
		m_UpdateComponentsSubscription = null;
		ResetDetailedViewState();
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		Clear();
		if (manager == null)
		{
			return;
		}
		BlueprintSelectionDoll selectionByType = CharGenUtility.GetSelectionByType<BlueprintSelectionDoll>(manager.Path);
		BlueprintGenderSelection selectionByType2 = CharGenUtility.GetSelectionByType<BlueprintGenderSelection>(manager.Path);
		if (selectionByType == null || selectionByType2 == null)
		{
			return;
		}
		m_SelectionStateDoll = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStateDoll;
		m_SelectionStateGender = manager.GetSelectionState(manager.Path, selectionByType2, 0) as SelectionStateGender;
		m_UpdateComponentsSubscription = new CompositeDisposable();
		m_UpdateComponentsSubscription.Add(CharGenContext.Doll.GetReactiveProperty((DollState dollState) => dollState.Gender).Subscribe(delegate(Gender gender)
		{
			m_SelectionStateGender.SelectGender(gender);
			UpdateComponents();
		}));
		m_UpdateComponentsSubscription.Add(CharGenContext.Doll.GetReactiveProperty((DollState dollState) => dollState.Portrait).Subscribe(delegate
		{
			if (CharGenContext.Doll.TrackPortrait)
			{
				UpdateComponents();
			}
		}));
		PagesSelectionGroupRadioVM.TrySelectFirstValidEntity();
	}

	public void HandleDollStateUpdated(DollState dollState)
	{
		ApplyDollState(dollState);
	}

	private void UpdateComponents()
	{
		foreach (CharGenAppearancePageVM page in m_Pages)
		{
			page.UpdateComponents();
		}
	}

	private void UpdateVisualSettings()
	{
		if (CurrentPageVM.Value != null)
		{
			CharGenUtility.GetClothesColorsProfile(CharGenContext.Doll.Clothes, out var _);
			base.ShowVisualSettings.Value = false;
		}
	}

	private void ClearPortrait()
	{
		PortraitVM.Value?.Dispose();
		PortraitVM.Value = null;
	}

	public void HandleHoverStart(PortraitData portrait)
	{
		PortraitVM.Value = new CharGenPortraitVM(portrait);
	}

	public void HandleHoverStop()
	{
		ClearPortrait();
	}

	public bool GoNextPage()
	{
		return PagesSelectionGroupRadioVM.SelectNextValidEntity();
	}

	public bool GoPrevPage()
	{
		return PagesSelectionGroupRadioVM.SelectPrevValidEntity();
	}

	public void HandleAppearanceComponentUpdate(CharGenAppearancePageComponent component)
	{
		CurrentPageVM.Value.UpdateComponent(component);
	}
}
