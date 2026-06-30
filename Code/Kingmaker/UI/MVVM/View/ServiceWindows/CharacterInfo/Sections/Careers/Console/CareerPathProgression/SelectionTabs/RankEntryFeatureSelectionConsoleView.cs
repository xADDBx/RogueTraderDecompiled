using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;
using Kingmaker.Code.UI.MVVM.VM.SaveLoad;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.PC.CareerPathProgression.SelectionTabs;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Console.CareerPathProgression.SelectionTabs;

public class RankEntryFeatureSelectionConsoleView : BaseCareerPathSelectionTabConsoleView<RankEntrySelectionVM>, IUIHighlighter, ISubscriber
{
	[Header("UltimateFeatures")]
	[SerializeField]
	private CharInfoFeatureConsoleView m_UltimateFeatureConsoleView;

	[Header("Filters")]
	[SerializeField]
	private FeaturesFilterBaseView m_FeaturesFilter;

	[SerializeField]
	private TextMeshProUGUI m_NoFeaturesText;

	[Header("Selector")]
	[SerializeField]
	private VirtualListVertical m_VirtualList;

	[Header("Elements")]
	[SerializeField]
	private SeparatorElementView m_SeparatorElementView;

	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_PrevFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	[SerializeField]
	private OwlcatMultiButton m_GroupByTypeButton;

	[SerializeField]
	private TextMeshProUGUI m_GroupByTypeButtonText;

	[SerializeField]
	private OwlcatMultiButton m_GroupBySourceButton;

	[SerializeField]
	private TextMeshProUGUI m_GroupBySourceButtonText;

	[SerializeField]
	private OwlcatMultiButton m_FavouriteGroupingButton;

	[SerializeField]
	private RankEntryStatItemCommonView m_RankEntryStatItemCommonView;

	[SerializeField]
	private RankEntryFeatureItemCommonView m_RankEntryFeatureItemCommonView;

	[SerializeField]
	private RankEntryUltimateFeatureUpgradeItemCommonView m_RankEntryUltimateFeatureUpgradeItemCommonView;

	[SerializeField]
	private RankEntryDescriptionView m_RankEntryDescriptionView;

	[FormerlySerializedAs("m_AvailableTalentsDropDown")]
	[SerializeField]
	private AvailableTalentsDropDownCommonView m_AvailableTalentsDropDown;

	private Action<bool> m_ReturnAction;

	private readonly ReactiveCollection<VirtualListElementVMBase> m_VMCollection = new ReactiveCollection<VirtualListElementVMBase>();

	private GridConsoleNavigationBehaviour m_Navigation;

	private CompositeDisposable m_DropdownDisposables;

	private AvailableTalentsDropDownVM m_DropdownToFocusAfterRebuild;

	private bool m_FocusFirstItemAfterDropdown;

	private RankEntrySelectionFeatureVM m_IsFocusedSelection;

	RectTransform IUIHighlighter.RectTransform => RectTransform;

	public override void Initialize()
	{
		base.Initialize();
		m_VirtualList.Initialize(new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryFeatureItemCommonView, 0), new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryUltimateFeatureUpgradeItemCommonView, 1), new VirtualListElementTemplate<RankEntrySelectionStatVM>(m_RankEntryStatItemCommonView, 0), new VirtualListElementTemplate<SeparatorElementVM>(m_SeparatorElementView), new VirtualListElementTemplate<AvailableTalentsDropDownVM>(m_AvailableTalentsDropDown), new VirtualListElementTemplate<RankEntryDescriptionVM>(m_RankEntryDescriptionView));
		m_FeaturesFilter.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_UltimateFeatureConsoleView.SetActiveState(base.ViewModel.UltimateFeature != null);
		m_UltimateFeatureConsoleView.Bind(base.ViewModel.UltimateFeature);
		AddDisposable(base.ViewModel.EntryState.Subscribe(delegate
		{
			SetHeader(UIStrings.Instance.CharacterSheet.GetFeatureGroupHint(base.ViewModel.FeatureGroup, base.ViewModel.CanChangeSelection));
		}));
		AddDisposable(m_VirtualList.Subscribe(m_VMCollection));
		m_FeaturesFilter.Or(null)?.Bind(base.ViewModel.FeaturesFilterVM);
		if (base.ViewModel.IsShipContext)
		{
			m_FeaturesFilter.gameObject.SetActive(base.ViewModel.FeaturesFilterVM != null);
			m_GroupByTypeButton.gameObject.SetActive(value: false);
			m_GroupBySourceButton.gameObject.SetActive(value: false);
			if (m_FavouriteGroupingButton != null)
			{
				m_FavouriteGroupingButton.gameObject.SetActive(value: false);
			}
		}
		else
		{
			m_FeaturesFilter.gameObject.SetActive(value: false);
			m_GroupByTypeButton.gameObject.SetActive(value: true);
			m_GroupBySourceButton.gameObject.SetActive(value: true);
			if (m_FavouriteGroupingButton != null)
			{
				m_FavouriteGroupingButton.gameObject.SetActive(value: true);
			}
			m_GroupByTypeButtonText.text = UIStrings.Instance.CharGen.OrderByType;
			m_GroupBySourceButtonText.text = UIStrings.Instance.CharGen.OrderBySource;
			AddDisposable(base.ViewModel.GroupingMode.Subscribe(delegate(FeatureGroupingMode mode)
			{
				m_GroupByTypeButton.SetActiveLayer((mode == FeatureGroupingMode.ByType) ? "On" : "Off");
				m_GroupBySourceButton.SetActiveLayer((mode == FeatureGroupingMode.BySource) ? "On" : "Off");
				if (m_FavouriteGroupingButton != null)
				{
					m_FavouriteGroupingButton.SetActiveLayer((mode == FeatureGroupingMode.Favourites) ? "On" : "Off");
				}
			}));
		}
		AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.OnFilterChange, delegate
		{
			UpdateCollection();
		}));
		if (base.ViewModel.SelectedFeature.Value != null)
		{
			m_VirtualList.ScrollController.ForceScrollToElement(base.ViewModel.SelectedFeature.Value);
		}
		AddDisposable(base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate
		{
			UpdateState();
		}));
		m_NoFeaturesText.text = UIStrings.Instance.CharacterSheet.NoFeaturesInFilter;
		UpdateCollection();
		CreateNavigation();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_VMCollection.Clear();
		m_DropdownDisposables?.Dispose();
		m_DropdownDisposables = null;
		m_DropdownToFocusAfterRebuild = null;
		m_FocusFirstItemAfterDropdown = false;
		m_IsFocusedSelection = null;
		m_Navigation?.Clear();
		m_Navigation = null;
		m_FeaturesFilter.Or(null)?.Unbind();
	}

	public override void AddInput(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		if (base.ViewModel.FeaturesFilterVM == null)
		{
			m_PrevFilterHint?.Dispose();
			m_NextFilterHint?.Dispose();
			return;
		}
		if ((bool)m_PrevFilterHint)
		{
			InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
			{
				bool isFocused3 = m_Navigation.IsFocused;
				if (base.ViewModel.IsShipContext)
				{
					m_FeaturesFilter.Or(null)?.SetPrevFilter();
				}
				else
				{
					base.ViewModel.SetGroupingMode(FeatureGroupingMode.BySource);
				}
				if (isFocused3)
				{
					UpdateFocus();
				}
			}, 14);
			AddDisposable(m_PrevFilterHint.Bind(inputBindStruct));
			AddDisposable(inputBindStruct);
		}
		if ((bool)m_NextFilterHint)
		{
			InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
			{
				bool isFocused2 = m_Navigation.IsFocused;
				if (base.ViewModel.IsShipContext)
				{
					m_FeaturesFilter.Or(null)?.SetNextFilter();
				}
				else
				{
					base.ViewModel.SetGroupingMode(FeatureGroupingMode.ByType);
				}
				if (isFocused2)
				{
					UpdateFocus();
				}
			}, 15);
			AddDisposable(m_NextFilterHint.Bind(inputBindStruct2));
			AddDisposable(inputBindStruct2);
		}
		if (base.ViewModel.IsShipContext)
		{
			return;
		}
		InputBindStruct inputBindStruct3 = inputLayer.AddButton(delegate
		{
			bool isFocused = m_Navigation.IsFocused;
			base.ViewModel.ToggleFavouritesMode();
			if (isFocused)
			{
				UpdateFocus();
			}
		}, 17, InputActionEventType.ButtonJustLongPressed);
		AddDisposable(hintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.InventoryScreen.FavoriteCategory));
		AddDisposable(inputBindStruct3);
		void UpdateFocus()
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VirtualList.GetNavigationBehaviour().SetCurrentEntity(m_VirtualList.ActiveElements.FirstOrDefault(delegate(VirtualListElement i)
				{
					IVirtualListElementData data = i.Data;
					return !(data is ExpandableTitleVM) && !(data is AvailableTalentsDropDownVM);
				}));
			}, 1);
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
		}
	}

	private void UpdateCollection()
	{
		m_IsFocusedSelection = m_VirtualList.ActiveElements.Select((VirtualListElement e) => (e.ConsoleEntityProxy as IHasViewModel)?.GetViewModel() as RankEntrySelectionFeatureVM).FirstOrDefault((RankEntrySelectionFeatureVM vm) => vm?.FocusedState.Value ?? false);
		m_VMCollection.Clear();
		if (base.ViewModel.FilteredGroupList == null)
		{
			return;
		}
		foreach (VirtualListElementVMBase filteredGroup in base.ViewModel.FilteredGroupList)
		{
			m_VMCollection.Add(filteredGroup);
		}
		m_NoFeaturesText.gameObject.SetActive(!base.ViewModel.FilteredGroupList.Any());
		SubscribeToDropdowns();
		RebuildNavigation();
	}

	public override void UpdateState()
	{
		ButtonActive.Value = base.ViewModel.SelectionMadeAndValid && !base.ViewModel.CareerPathVM.ReadOnly.Value;
	}

	protected override void HandleClickNext()
	{
		if (base.IsBinded)
		{
			if (base.ViewModel.CareerPathVM.CanCommit.Value && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
			{
				base.ViewModel.CareerPathVM.SetRankEntry(null);
			}
			else if (base.ViewModel.SelectionMade && base.ViewModel.SelectedFeature.Value.FocusedState.Value)
			{
				base.ViewModel.CareerPathVM.SelectNextItem();
				UISounds.Instance.Sounds.Buttons.DoctrineNextButtonClick.Play();
			}
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
		EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
		{
			h.SetFocusOn(null);
		});
	}

	private void CreateNavigation()
	{
		m_Navigation = new GridConsoleNavigationBehaviour();
		if (base.ViewModel.UltimateFeature != null)
		{
			m_Navigation.AddEntityVertical(m_UltimateFeatureConsoleView);
		}
		GridConsoleNavigationBehaviour vListNav = m_VirtualList.GetNavigationBehaviour();
		vListNav.SetEntitiesVertical(m_VirtualList.ActiveElements);
		m_Navigation.AddEntityVertical(vListNav);
		DelayedInvoker.InvokeInFrames(delegate
		{
			VirtualListElement virtualListElement = m_VirtualList.ActiveElements.FirstOrDefault((VirtualListElement e) => ((e.ConsoleEntityProxy as IHasViewModel)?.GetViewModel() as BaseRankEntryFeatureVM)?.FeatureState.Value == RankFeatureState.Selected);
			if (virtualListElement != null)
			{
				vListNav.SetCurrentEntity(virtualListElement);
			}
			else
			{
				vListNav.SetCurrentEntity(m_VirtualList.ActiveElements.FirstOrDefault(delegate(VirtualListElement i)
				{
					IVirtualListElementData data = i.Data;
					return !(data is ExpandableTitleVM) && !(data is AvailableTalentsDropDownVM);
				}));
			}
			m_Navigation?.SetCurrentEntity(vListNav);
		}, 1);
		AddDisposable(m_Navigation.DeepestFocusAsObservable.Subscribe(delegate(IConsoleEntity value)
		{
			if (value != null)
			{
				EventBus.RaiseEvent(delegate(IRankEntryFocusHandler h)
				{
					h.SetFocusOn((value as IHasViewModel)?.GetViewModel() as BaseRankEntryFeatureVM);
				});
			}
		}));
	}

	private void RebuildNavigation()
	{
		if (m_Navigation == null)
		{
			return;
		}
		m_Navigation.Clear();
		if (base.ViewModel.UltimateFeature != null)
		{
			m_Navigation.AddEntityVertical(m_UltimateFeatureConsoleView);
		}
		GridConsoleNavigationBehaviour vListNav = m_VirtualList.GetNavigationBehaviour();
		vListNav.SetEntitiesVertical(m_VirtualList.ActiveElements);
		m_Navigation.AddEntityVertical(vListNav);
		DelayedInvoker.InvokeInFrames(delegate
		{
			if (m_Navigation != null)
			{
				AvailableTalentsDropDownVM dropdownToFocus = m_DropdownToFocusAfterRebuild;
				m_DropdownToFocusAfterRebuild = null;
				bool focusFirstItemAfterDropdown = m_FocusFirstItemAfterDropdown;
				m_FocusFirstItemAfterDropdown = false;
				if (dropdownToFocus != null)
				{
					List<VirtualListElement> activeElements = m_VirtualList.ActiveElements;
					VirtualListElement virtualListElement = activeElements.FirstOrDefault((VirtualListElement e) => e.Data == dropdownToFocus);
					if (virtualListElement != null)
					{
						VirtualListElement currentEntity = virtualListElement;
						if (focusFirstItemAfterDropdown)
						{
							int num = activeElements.IndexOf(virtualListElement);
							VirtualListElement virtualListElement2 = activeElements.Skip(num + 1).FirstOrDefault(delegate(VirtualListElement i)
							{
								IVirtualListElementData data2 = i.Data;
								return !(data2 is ExpandableTitleVM) && !(data2 is AvailableTalentsDropDownVM);
							});
							if (virtualListElement2 != null)
							{
								currentEntity = virtualListElement2;
							}
						}
						vListNav.SetCurrentEntity(currentEntity);
						m_Navigation.SetCurrentEntity(vListNav);
						return;
					}
				}
				object obj = ((m_IsFocusedSelection != null) ? m_VirtualList.ActiveElements.FirstOrDefault((VirtualListElement e) => (e.ConsoleEntityProxy as IHasViewModel)?.GetViewModel() == m_IsFocusedSelection) : null);
				m_IsFocusedSelection = null;
				if (obj == null)
				{
					obj = m_VirtualList.ActiveElements.FirstOrDefault((VirtualListElement e) => ((e.ConsoleEntityProxy as IHasViewModel)?.GetViewModel() as BaseRankEntryFeatureVM)?.FeatureState.Value == RankFeatureState.Selected);
				}
				VirtualListElement virtualListElement3 = (VirtualListElement)obj;
				vListNav.SetCurrentEntity(virtualListElement3 ?? m_VirtualList.ActiveElements.FirstOrDefault(delegate(VirtualListElement i)
				{
					IVirtualListElementData data = i.Data;
					return !(data is ExpandableTitleVM) && !(data is AvailableTalentsDropDownVM);
				}));
				m_Navigation.SetCurrentEntity(vListNav);
				m_Navigation.FocusOnCurrentEntity();
			}
		}, 1);
	}

	private void SubscribeToDropdowns()
	{
		m_DropdownDisposables?.Dispose();
		m_DropdownDisposables = new CompositeDisposable();
		if (base.ViewModel.FilteredGroupList == null)
		{
			return;
		}
		foreach (VirtualListElementVMBase filteredGroup in base.ViewModel.FilteredGroupList)
		{
			AvailableTalentsDropDownVM dropdown = filteredGroup as AvailableTalentsDropDownVM;
			if (dropdown != null)
			{
				m_DropdownDisposables.Add(dropdown.IsExpanded.Skip(1).Subscribe(delegate(bool isExpanded)
				{
					m_DropdownToFocusAfterRebuild = dropdown;
					m_FocusFirstItemAfterDropdown = isExpanded;
					DelayedInvoker.InvokeInFrames(RebuildNavigation, 2);
				}));
			}
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour()
	{
		if (m_Navigation == null)
		{
			CreateNavigation();
		}
		return m_Navigation;
	}

	public void StartHighlight(string key)
	{
	}

	public void StopHighlight(string key)
	{
	}

	public void Highlight(string key)
	{
	}

	public void HighlightOnce(string key)
	{
		if (m_VMCollection == null)
		{
			return;
		}
		int itemId = m_VMCollection.FindIndex((VirtualListElementVMBase vm) => (vm as RankEntrySelectionFeatureVM)?.Feature.AssetGuid == key);
		if (itemId < 0)
		{
			return;
		}
		m_VirtualList.ScrollController.ForceScrollToElement(m_VMCollection.ElementAt(itemId));
		DelayedInvoker.InvokeInFrames(delegate
		{
			RankEntryFeatureItemCommonView rankEntryFeatureItemCommonView = m_VirtualList.Elements.ElementAt(itemId).View as RankEntryFeatureItemCommonView;
			if (rankEntryFeatureItemCommonView != null)
			{
				rankEntryFeatureItemCommonView.StartHighlight(key);
				m_VirtualList.GetNavigationBehaviour().FocusOnEntityManual(rankEntryFeatureItemCommonView);
			}
			EventBus.RaiseEvent(delegate(IUIHighlighter h)
			{
				h.StopHighlight(key);
			});
		}, 1);
	}
}
