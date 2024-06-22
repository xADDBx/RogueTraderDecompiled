using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.DLC;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Base;
using Kingmaker.UI.MVVM.VM.CharGen.Portrait;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections.Portrait;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.SelectionGroup;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;

public class CharGenPortraitsSelectorVM : BaseCharGenAppearancePageComponentVM, ICharGenPortraitHandler, ISubscriber
{
	private const int FirstScreenAmount = 40;

	public readonly IReadOnlyReactiveProperty<CharGenPortraitVM> PortraitVM;

	public readonly SelectionGroupRadioVM<CharGenPortraitTabVM> TabSelector;

	public readonly ReactiveProperty<CharGenPortraitTabVM> CurrentTab = new ReactiveProperty<CharGenPortraitTabVM>();

	public readonly Dictionary<PortraitCategory, CharGenPortraitGroupVM> PortraitGroupVms = new Dictionary<PortraitCategory, CharGenPortraitGroupVM>();

	public readonly CharGenPortraitGroupVM CustomPortraitGroup = new CharGenPortraitGroupVM();

	public readonly ReactiveProperty<CharGenCustomPortraitCreatorVM> CustomPortraitCreatorVM = new ReactiveProperty<CharGenCustomPortraitCreatorVM>();

	private readonly SelectionGroupRadioVM<CharGenPortraitSelectorItemVM> m_SelectorGroupVM;

	private readonly ReactiveCollection<CharGenPortraitSelectorItemVM> m_AllPortraitsCollection = new ReactiveCollection<CharGenPortraitSelectorItemVM>();

	private readonly ReactiveProperty<CharGenPortraitSelectorItemVM> m_SelectedPortrait = new ReactiveProperty<CharGenPortraitSelectorItemVM>();

	private readonly CharGenContext m_CharGenContext;

	private SelectionStatePortrait m_SelectionStatePortrait;

	public CharGenPortraitSelectorItemVM SelectedPortrait => m_SelectedPortrait.Value;

	public CharGenPortraitsSelectorVM(CharGenContext ctx)
	{
		m_CharGenContext = ctx;
		AddDisposable(EventBus.Subscribe(this));
		List<CharGenPortraitTabVM> visibleCollection = (from CharGenPortraitTab tab in Enum.GetValues(typeof(CharGenPortraitTab))
			select AddDisposableAndReturn(new CharGenPortraitTabVM(tab))).ToList();
		AddDisposable(TabSelector = new SelectionGroupRadioVM<CharGenPortraitTabVM>(visibleCollection, CurrentTab, cyclical: true));
		TabSelector.TrySelectFirstValidEntity();
		AddDisposable(CurrentTab.Subscribe(delegate(CharGenPortraitTabVM tab)
		{
			if (tab != null && tab.Tab != CharGenPortraitTab.Custom)
			{
				OnCustomPortraitCreatorClose();
			}
		}));
		AddDisposable(CustomPortraitGroup);
		CollectCharacterPortraits();
		CollectCustomPortraits();
		AddDisposable(m_SelectorGroupVM = new SelectionGroupRadioVM<CharGenPortraitSelectorItemVM>(m_AllPortraitsCollection, m_SelectedPortrait));
		AddDisposable(m_CharGenContext.LevelUpManager.Subscribe(HandleLevelUpManager));
		PortraitVM = m_SelectedPortrait.NotNull().Select(delegate(CharGenPortraitSelectorItemVM itemVM)
		{
			CharGenPortraitVM charGenPortraitVM = new CharGenPortraitVM(itemVM.PortraitData);
			AddDisposable(charGenPortraitVM);
			return charGenPortraitVM;
		}).ToReadOnlyReactiveProperty();
		AddDisposable(m_SelectedPortrait.NotNull().Subscribe(delegate(CharGenPortraitSelectorItemVM portraitVM)
		{
			Game.Instance.GameCommandQueue.CharGenSetPortrait(portraitVM.GetBlueprintPortrait());
		}));
		TrySelectPortrait();
	}

	protected override void DisposeImplementation()
	{
	}

	public override void OnBeginView()
	{
		TrySelectPortrait();
	}

	protected virtual void CollectCharacterPortraits()
	{
		foreach (BlueprintPortrait portrait in Game.Instance.BlueprintRoot.CharGenRoot.Portraits)
		{
			if (IsPortraitAllowed(portrait))
			{
				CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = AddDisposableAndReturn(new CharGenPortraitSelectorItemVM(portrait));
				m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
				PortraitCategory portraitCategory = portrait.Data.PortraitCategory;
				if (!PortraitGroupVms.ContainsKey(portraitCategory))
				{
					PortraitGroupVms.Add(portraitCategory, AddDisposableAndReturn(new CharGenPortraitGroupVM(portraitCategory)));
					PortraitGroupVms[portraitCategory].Expanded.Value = true;
				}
				PortraitGroupVms[portraitCategory].Add(charGenPortraitSelectorItemVM);
			}
		}
	}

	private bool IsPortraitAllowed(BlueprintPortrait portrait)
	{
		if (portrait.IsDlcRestricted())
		{
			return false;
		}
		CharGenConfig charGenConfig = m_CharGenContext.CharGenConfig;
		if (charGenConfig.Mode == CharGenConfig.CharGenMode.NewCompanion && charGenConfig.CompanionType == CharGenConfig.CharGenCompanionType.Navigator)
		{
			return portrait.Data.PortraitCategory == PortraitCategory.Navigator;
		}
		return portrait.Data.PortraitCategory != PortraitCategory.Navigator;
	}

	private void CollectCustomPortraits()
	{
		using (ProfileScope.New("Collect custom portraits"))
		{
			CustomPortraitGroup.Add(AddDisposableAndReturn(new CharGenPortraitSelectorItemVM(delegate
			{
				OnCustomPortraitCreate();
			})));
			foreach (PortraitData item in CustomPortraitsManager.Instance.LoadAllPortraits(40))
			{
				CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = AddDisposableAndReturn(new CharGenPortraitSelectorItemVM(item, OnCustomPortraitChange));
				m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
				CustomPortraitGroup.Add(charGenPortraitSelectorItemVM);
			}
		}
	}

	private bool TryUpdatePortraitFromState()
	{
		if (m_CharGenContext.Doll.Portrait != null)
		{
			CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = m_AllPortraitsCollection.FirstOrDefault((CharGenPortraitSelectorItemVM p) => p.PortraitData == m_CharGenContext.Doll.Portrait.Data);
			if (charGenPortraitSelectorItemVM != null)
			{
				m_SelectedPortrait.SetValueAndForceNotify(charGenPortraitSelectorItemVM);
				return true;
			}
		}
		return false;
	}

	private void TrySelectPortrait()
	{
		if (TryUpdatePortraitFromState())
		{
			return;
		}
		foreach (PortraitCategory category in new List<PortraitCategory>
		{
			PortraitCategory.None,
			PortraitCategory.Navigator
		})
		{
			if (PortraitGroupVms.ContainsKey(category))
			{
				m_SelectedPortrait.SetValueAndForceNotify(m_AllPortraitsCollection.FirstOrDefault((CharGenPortraitSelectorItemVM p) => PortraitGroupVms[category].PortraitCollection.Contains(p)));
				break;
			}
		}
	}

	private void OnCustomPortraitCreate(BlueprintPortrait blueprintPortrait = null)
	{
		CustomPortraitGroup.RemoveNonexistentItems();
		CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = new CharGenPortraitSelectorItemVM(blueprintPortrait?.Data ?? CustomPortraitsManager.Instance.CreateNew(), OnCustomPortraitChange);
		m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
		CustomPortraitGroup.Add(charGenPortraitSelectorItemVM);
		m_SelectedPortrait.Value = charGenPortraitSelectorItemVM;
		OpenCustomPortraitCreator();
	}

	private void OnCustomPortraitChange(CharGenPortraitSelectorItemVM changeItem)
	{
		m_SelectedPortrait.Value = changeItem;
		OpenCustomPortraitCreator();
	}

	public void SelectPortraitAndOpenCreator(CharGenPortraitSelectorItemVM changeItem)
	{
		OnCustomPortraitChange(changeItem);
	}

	public void OpenCustomPortraitCreator()
	{
		if (CustomPortraitCreatorVM.Value == null && PortraitVM.Value.PortraitData.IsCustom && CustomPortraitsManager.Instance.EnsureCustomPortraits(PortraitVM.Value.PortraitData.CustomId) && UINetUtility.IsControlMainCharacter())
		{
			CustomPortraitCreatorVM.Value = new CharGenCustomPortraitCreatorVM(PortraitVM, OnOpenFolderClick, OnRefreshPortraitsClick, OnCustomPortraitCreatorClose);
		}
	}

	public void SelectNextPortraitsTab()
	{
		TabSelector.SelectNextValidEntity();
	}

	private void OnCustomPortraitCreatorClose()
	{
		CustomPortraitCreatorVM.Value?.Dispose();
		CustomPortraitCreatorVM.Value = null;
	}

	private void OnOpenFolderClick()
	{
		CustomPortraitsManager.Instance.OpenPortraitFolder(m_SelectedPortrait.Value.PortraitData.CustomId);
	}

	private void OnRefreshPortraitsClick()
	{
		string customId = m_SelectedPortrait.Value.PortraitData.CustomId;
		m_AllPortraitsCollection.RemoveAll((CharGenPortraitSelectorItemVM item) => item.PortraitData?.IsCustom ?? true);
		CustomPortraitGroup.RemoveCustomItems();
		CustomPortraitsManager.Instance.UpdateGuid(customId);
		CustomPortraitsManager.Instance.Cleanup();
		foreach (PortraitData item in CustomPortraitsManager.Instance.LoadAllPortraits(40))
		{
			CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = new CharGenPortraitSelectorItemVM(item, OnCustomPortraitChange);
			m_AllPortraitsCollection.Add(charGenPortraitSelectorItemVM);
			CustomPortraitGroup.Add(charGenPortraitSelectorItemVM);
		}
		m_SelectedPortrait.SetValueAndForceNotify(CustomPortraitGroup.GetByIdOrFirstValid(customId));
	}

	private void HandleLevelUpManager(LevelUpManager manager)
	{
		if (manager != null)
		{
			BlueprintPortraitSelection selectionByType = CharGenUtility.GetSelectionByType<BlueprintPortraitSelection>(manager.Path);
			if (selectionByType != null)
			{
				m_SelectionStatePortrait = manager.GetSelectionState(manager.Path, selectionByType, 0) as SelectionStatePortrait;
				m_SelectedPortrait.Value = null;
			}
		}
	}

	void ICharGenPortraitHandler.HandleSetPortrait(BlueprintPortrait portrait)
	{
		m_SelectionStatePortrait?.SelectPortrait(portrait);
		m_CharGenContext.Doll.SetPortrait(portrait);
		if (!UINetUtility.IsControlMainCharacter())
		{
			CharGenPortraitSelectorItemVM charGenPortraitSelectorItemVM = m_AllPortraitsCollection.FirstOrDefault(portrait.Data.IsCustom ? ((Func<CharGenPortraitSelectorItemVM, bool>)((CharGenPortraitSelectorItemVM p) => portrait.Data.CustomId.Equals(p.PortraitData?.CustomId, StringComparison.Ordinal))) : ((Func<CharGenPortraitSelectorItemVM, bool>)((CharGenPortraitSelectorItemVM p) => portrait.Data == p.PortraitData)));
			if (charGenPortraitSelectorItemVM != null)
			{
				m_SelectorGroupVM.TrySelectEntity(charGenPortraitSelectorItemVM);
			}
			else
			{
				OnCustomPortraitCreate(portrait);
			}
		}
		Changed();
	}
}
