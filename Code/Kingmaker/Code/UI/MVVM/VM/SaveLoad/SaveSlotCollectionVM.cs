using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveSlotCollectionVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly IReactiveProperty<SaveLoadMode> Mode;

	public readonly IReadOnlyReactiveProperty<SaveSlotVM> SelectedSaveSlot;

	private readonly ReactiveCollection<SaveSlotVM> m_AllSlots = new ReactiveCollection<SaveSlotVM>();

	public readonly ReactiveCollection<VirtualListElementVMBase> AllTitlesAndSlots = new ReactiveCollection<VirtualListElementVMBase>();

	private readonly Action m_BackButtonAction;

	private ReactiveCollection<SaveSlotGroupVM> SaveSlotGroups { get; } = new ReactiveCollection<SaveSlotGroupVM>();


	public SaveSlotCollectionVM(IReactiveProperty<SaveLoadMode> mode, IReadOnlyReactiveProperty<SaveSlotVM> selectedSaveSlot = null, Action backButtonAction = null)
	{
		Mode = mode;
		SelectedSaveSlot = selectedSaveSlot;
		m_BackButtonAction = backButtonAction;
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleNewSave(SaveSlotVM slot)
	{
		SaveSlotGroupVM saveSlotGroupVM = SaveSlotGroups.FirstOrDefault((SaveSlotGroupVM group) => group.GameId == slot.GameId.Value && group.CharacterName == slot.Reference.PlayerCharacterName);
		if (saveSlotGroupVM != null)
		{
			int index = AllTitlesAndSlots.IndexOf(saveSlotGroupVM.LastElement) + 1;
			AllTitlesAndSlots.Insert(index, slot);
			saveSlotGroupVM.HandleNewSave(slot);
		}
		else
		{
			saveSlotGroupVM = new SaveSlotGroupVM(slot, DeleteAllSlots(slot.GameId.Value));
			AddDisposable(saveSlotGroupVM);
			if (SaveSlotGroups.Empty())
			{
				saveSlotGroupVM.IsFirst = true;
			}
			SaveSlotGroups.Add(saveSlotGroupVM);
			saveSlotGroupVM.HandleNewSave(slot);
			AllTitlesAndSlots.Add(saveSlotGroupVM.ExpandableTitleVM);
			AllTitlesAndSlots.Add(slot);
		}
		m_AllSlots.Add(slot);
	}

	private Action DeleteAllSlots(string groupID)
	{
		return delegate
		{
			List<SaveSlotVM> slotsInGroup = m_AllSlots.Where((SaveSlotVM s) => s.GameId.Value == groupID).ToList();
			SaveSlotGroupVM saveSlotGroupVM = SaveSlotGroups.FirstOrDefault((SaveSlotGroupVM g) => g.GameId == slotsInGroup.FirstOrDefault()?.GameId.Value);
			if (saveSlotGroupVM != null)
			{
				for (int num = saveSlotGroupVM.SaveLoadSlots.Count - 1; num >= 0; num--)
				{
					saveSlotGroupVM.SaveLoadSlots[num].DeleteWithoutBox();
				}
				foreach (SaveSlotVM item in slotsInGroup)
				{
					saveSlotGroupVM.HandleDeleteSave(item);
					AllTitlesAndSlots.Remove(item);
					m_AllSlots.Remove(item);
				}
				AllTitlesAndSlots.Remove(saveSlotGroupVM.ExpandableTitleVM);
				saveSlotGroupVM.Dispose();
				SaveSlotGroups.Remove(saveSlotGroupVM);
				if (saveSlotGroupVM.IsFirst && SaveSlotGroups.Any())
				{
					SaveSlotGroups.First().IsFirst = true;
				}
				Game.Instance.SaveManager.UpdateSaveListAsync();
			}
		};
	}

	public void HandleDeleteSave(SaveSlotVM slot)
	{
		SaveSlotGroupVM saveSlotGroupVM = SaveSlotGroups.FirstOrDefault((SaveSlotGroupVM group) => group.GameId == slot.GameId.Value);
		if (saveSlotGroupVM != null)
		{
			saveSlotGroupVM.HandleDeleteSave(slot);
			if (saveSlotGroupVM.IsEmpty)
			{
				AllTitlesAndSlots.Remove(saveSlotGroupVM.ExpandableTitleVM);
				saveSlotGroupVM.Dispose();
				SaveSlotGroups.Remove(saveSlotGroupVM);
				if (saveSlotGroupVM.IsFirst && SaveSlotGroups.Any())
				{
					SaveSlotGroups.First().IsFirst = true;
				}
			}
		}
		AllTitlesAndSlots.Remove(slot);
		m_AllSlots.Remove(slot);
		slot.Dispose();
	}

	public void OnBack()
	{
		m_BackButtonAction?.Invoke();
	}

	public void RefreshDLC()
	{
		foreach (SaveSlotVM allSlot in m_AllSlots)
		{
			allSlot.CheckDLC();
		}
	}
}
