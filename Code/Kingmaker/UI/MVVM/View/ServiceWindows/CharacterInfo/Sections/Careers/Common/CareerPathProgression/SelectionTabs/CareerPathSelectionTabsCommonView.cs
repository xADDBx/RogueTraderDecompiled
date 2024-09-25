using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.SelectionTabs;

public abstract class CareerPathSelectionTabsCommonView : ViewBase<CareerPathVM>
{
	public enum SelectionTab
	{
		CareerPathDescription,
		FeatureDescription,
		FeatureSelection
	}

	protected List<ICareerPathSelectionTabView> Tabs;

	protected SelectionTab? SavedTab;

	protected IRankEntrySelectItem SavedItem;

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Subscribe(delegate
		{
			UpdateActiveTab();
		}));
		AddDisposable(base.ViewModel.OnUpdateData.Subscribe(delegate
		{
			UpdateActiveTab();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		UnbindTabs();
		SavedTab = null;
	}

	protected void UnbindTabs()
	{
		Tabs.ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.Unbind();
		});
	}

	protected virtual void UpdateActiveTab()
	{
		IRankEntrySelectItem value = base.ViewModel.UnitProgressionVM.CurrentRankEntryItem.Value;
		SelectionTab activeTab = GetActiveTab(value);
		if (SavedTab != activeTab || value != SavedItem)
		{
			SavedTab = activeTab;
			SavedItem = value;
			UnbindTabs();
			SetNewTab(activeTab, value);
		}
		UpdateState();
	}

	protected void UpdateState()
	{
		Tabs.Where((ICareerPathSelectionTabView tab) => tab.IsTabActive()).ForEach(delegate(ICareerPathSelectionTabView tab)
		{
			tab.UpdateState();
		});
	}

	private SelectionTab GetActiveTab(IRankEntrySelectItem currentItem)
	{
		if (!(currentItem is RankEntryFeatureItemVM))
		{
			if (currentItem is RankEntrySelectionVM)
			{
				return SelectionTab.FeatureSelection;
			}
			return SelectionTab.CareerPathDescription;
		}
		return SelectionTab.FeatureDescription;
	}

	protected abstract void SetNewTab(SelectionTab newTab, IRankEntrySelectItem currentItem);
}
