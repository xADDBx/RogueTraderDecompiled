using System;
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
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.VirtualListSystem;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

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
	private RankEntryStatItemCommonView m_RankEntryStatItemCommonView;

	[SerializeField]
	private RankEntryFeatureItemCommonView m_RankEntryFeatureItemCommonView;

	[SerializeField]
	private RankEntryUltimateFeatureUpgradeItemCommonView m_RankEntryUltimateFeatureUpgradeItemCommonView;

	[SerializeField]
	private RankEntryDescriptionView m_RankEntryDescriptionView;

	private Action<bool> m_ReturnAction;

	private readonly ReactiveCollection<VirtualListElementVMBase> m_VMCollection = new ReactiveCollection<VirtualListElementVMBase>();

	private GridConsoleNavigationBehaviour m_Navigation;

	private RankEntrySelectionFeatureVM m_IsFocusedSelection;

	RectTransform IUIHighlighter.RectTransform => RectTransform;

	public override void Initialize()
	{
		base.Initialize();
		m_VirtualList.Initialize(new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryFeatureItemCommonView, 0), new VirtualListElementTemplate<RankEntrySelectionFeatureVM>(m_RankEntryUltimateFeatureUpgradeItemCommonView, 1), new VirtualListElementTemplate<RankEntrySelectionStatVM>(m_RankEntryStatItemCommonView, 0), new VirtualListElementTemplate<SeparatorElementVM>(m_SeparatorElementView), new VirtualListElementTemplate<RankEntryDescriptionVM>(m_RankEntryDescriptionView));
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
				bool isFocused2 = m_Navigation.IsFocused;
				m_FeaturesFilter.Or(null)?.SetPrevFilter();
				if (isFocused2)
				{
					UpdateFocus();
				}
			}, 14);
			AddDisposable(m_PrevFilterHint.Bind(inputBindStruct));
			AddDisposable(inputBindStruct);
		}
		if (!m_NextFilterHint)
		{
			return;
		}
		InputBindStruct inputBindStruct2 = inputLayer.AddButton(delegate
		{
			bool isFocused = m_Navigation.IsFocused;
			m_FeaturesFilter.Or(null)?.SetNextFilter();
			if (isFocused)
			{
				UpdateFocus();
			}
		}, 15);
		AddDisposable(m_NextFilterHint.Bind(inputBindStruct2));
		AddDisposable(inputBindStruct2);
		void UpdateFocus()
		{
			m_Navigation.FocusOnFirstValidEntity();
			DelayedInvoker.InvokeInFrames(delegate
			{
				m_VirtualList.GetNavigationBehaviour().SetCurrentEntity(m_VirtualList.ActiveElements.FirstOrDefault((VirtualListElement i) => !(i.Data is ExpandableTitleVM)));
			}, 1);
			EventBus.RaiseEvent(delegate(IUpdateFocusHandler h)
			{
				h.HandleFocus();
			});
		}
	}

	private void UpdateCollection()
	{
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
		m_Navigation.AddEntityVertical(vListNav);
		DelayedInvoker.InvokeInFrames(delegate
		{
			VirtualListElement virtualListElement = m_VirtualList.Elements.FirstOrDefault((VirtualListElement e) => ((e.ConsoleEntityProxy as IHasViewModel)?.GetViewModel() as BaseRankEntryFeatureVM)?.FeatureState.Value == RankFeatureState.Selected);
			if (virtualListElement != null)
			{
				vListNav.SetCurrentEntity(virtualListElement);
			}
			else
			{
				vListNav.SetCurrentEntity(m_VirtualList.ActiveElements.FirstOrDefault((VirtualListElement i) => !(i.Data is ExpandableTitleVM)));
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
