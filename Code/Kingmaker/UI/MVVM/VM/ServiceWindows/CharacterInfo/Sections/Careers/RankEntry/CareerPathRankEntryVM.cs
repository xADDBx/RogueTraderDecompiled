using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.CareerPath;
using Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry.Feature;
using Kingmaker.UnitLogic.Levelup;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Levelup.Selections.Feature;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Careers.RankEntry;

public class CareerPathRankEntryVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly int Rank;

	public readonly bool IsEmpty;

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> IsFirstSelectable = new ReactiveProperty<bool>();

	public readonly AutoDisposingList<RankEntryFeatureItemVM> Features = new AutoDisposingList<RankEntryFeatureItemVM>();

	public readonly AutoDisposingList<RankEntrySelectionVM> Selections = new AutoDisposingList<RankEntrySelectionVM>();

	private readonly CareerPathVM m_CareerPathVM;

	public CareerPathRankEntryVM(int rank, CareerPathVM careerPathVM, BlueprintPath.RankEntry rankEntry)
	{
		CareerPathRankEntryVM careerPathRankEntryVM = this;
		Rank = rank;
		IsEmpty = rankEntry.Features.Length == 0 && rankEntry.Selections.Length == 0;
		m_CareerPathVM = careerPathVM;
		foreach (BlueprintFeature feature in rankEntry.Features)
		{
			RankEntryFeatureItemVM rankEntryFeatureItemVM = new RankEntryFeatureItemVM(rank, careerPathVM, new UIFeature(feature), SelectRankEntryItem);
			AddDisposable(rankEntryFeatureItemVM);
			Features.Add(rankEntryFeatureItemVM);
		}
		foreach (BlueprintSelection selection in rankEntry.Selections)
		{
			if (selection is BlueprintSelectionFeature selectionFeature)
			{
				RankEntrySelectionVM rankEntrySelectionVM = new RankEntrySelectionVM(rank, careerPathVM, selectionFeature, SelectRankEntryItem);
				AddDisposable(rankEntrySelectionVM);
				Selections.Add(rankEntrySelectionVM);
			}
		}
		AddDisposable(careerPathVM.CurrentRank.Subscribe(delegate(int currentRank)
		{
			careerPathRankEntryVM.IsSelected.Value = rank <= currentRank;
			careerPathRankEntryVM.IsFirstSelectable.Value = currentRank == rank;
		}));
	}

	public void SelectRankEntryItem(IRankEntrySelectItem item)
	{
		m_CareerPathVM.SetRankEntry(item);
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	public void UpdateState(LevelUpManager levelUpManager)
	{
		Features.ForEach(delegate(RankEntryFeatureItemVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
		Selections.ForEach(delegate(RankEntrySelectionVM vm)
		{
			vm.UpdateState(levelUpManager);
		});
	}

	private void Clear()
	{
		Features.Clear();
		Selections.Clear();
	}

	public IRankEntrySelectItem GetNextFor(IRankEntrySelectItem item)
	{
		int num = Features.IndexOf(item);
		if (num < 0)
		{
			if (num == -1)
			{
				int num2 = Selections.IndexOf(item);
				if (num2 >= Selections.Count - 1)
				{
					return null;
				}
				return Selections.ElementAt(num2 + 1);
			}
			return null;
		}
		if (num >= Features.Count - 1)
		{
			return Selections.FirstOrDefault();
		}
		return Features.ElementAt(num + 1);
	}

	public IRankEntrySelectItem GetPreviousFor(IRankEntrySelectItem item)
	{
		int num = Selections.IndexOf(item);
		int num2 = num;
		if (num2 <= 0)
		{
			if (num2 == 0 && Features.Count > 0)
			{
				return Features.LastItem();
			}
			int num3 = Features.IndexOf(item);
			if (num3 <= 0)
			{
				return null;
			}
			return Features.ElementAt(num3 - 1);
		}
		return Selections.ElementAt(num - 1);
	}

	public IRankEntrySelectItem GetFirstItem()
	{
		if (Features.Count > 0)
		{
			return Features.FirstItem();
		}
		return Selections.FirstItem();
	}

	public IRankEntrySelectItem GetLastItem()
	{
		if (Selections.Count > 0)
		{
			return Selections.LastItem();
		}
		return Features.LastItem();
	}

	public List<IRankEntrySelectItem> GetRankSlice()
	{
		List<IRankEntrySelectItem> list = new List<IRankEntrySelectItem>();
		IRankEntrySelectItem rankEntrySelectItem = GetFirstItem();
		if (rankEntrySelectItem == null)
		{
			return list;
		}
		while (rankEntrySelectItem != null)
		{
			list.Add(rankEntrySelectItem);
			rankEntrySelectItem = GetNextFor(rankEntrySelectItem);
		}
		return list;
	}
}
