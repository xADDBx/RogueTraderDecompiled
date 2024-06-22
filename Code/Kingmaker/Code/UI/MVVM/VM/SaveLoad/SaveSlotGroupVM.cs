using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SaveLoad;

public class SaveSlotGroupVM : VirtualListElementVMBase
{
	public readonly ReactiveCommand<SaveSlotVM> OnAddSave = new ReactiveCommand<SaveSlotVM>();

	public readonly SaveSlotsExpandableTitleVM ExpandableTitleVM;

	public readonly string GameId;

	public readonly string GameName;

	public readonly string CharacterName;

	private bool m_IsFirst;

	public readonly BoolReactiveProperty IsExpanded = new BoolReactiveProperty(initialValue: false);

	private readonly Action m_DeleteAll;

	public List<SaveSlotVM> SaveLoadSlots { get; private set; }

	public bool IsEmpty => base.IsDisposed;

	public bool IsFirst
	{
		get
		{
			return m_IsFirst;
		}
		set
		{
			m_IsFirst = value;
			if (m_IsFirst)
			{
				ExpandableTitleVM.Expand();
			}
			else
			{
				ExpandableTitleVM.Collapse();
			}
		}
	}

	public VirtualListElementVMBase LastElement
	{
		get
		{
			if (!SaveLoadSlots.Any())
			{
				return ExpandableTitleVM;
			}
			return SaveLoadSlots.Last();
		}
	}

	public SaveSlotGroupVM(SaveSlotVM slot, Action deleteAll)
	{
		GameId = slot.GameId.Value;
		GameName = slot.GameName.Value;
		CharacterName = slot.CharacterName.Value;
		m_DeleteAll = deleteAll;
		AddDisposable(IsExpanded.Subscribe(ExpandChanged));
		string characterName = CharacterName;
		AddDisposable(ExpandableTitleVM = new SaveSlotsExpandableTitleVM(characterName, SwitchExpand, defaultExpanded: true, HandleDeleteAllSlotsInGroup, (slot.Reference.Type == SaveInfo.SaveType.IronMan) ? slot.Reference : null));
	}

	protected override void DisposeImplementation()
	{
	}

	public void HandleDeleteAllSlotsInGroup()
	{
		m_DeleteAll?.Invoke();
	}

	public List<VirtualListElementVMBase> GetAll()
	{
		List<VirtualListElementVMBase> list = new List<VirtualListElementVMBase>();
		list.Add(ExpandableTitleVM);
		list.AddRange(SaveLoadSlots);
		return list;
	}

	public void HandleNewSave(SaveSlotVM slot)
	{
		if (SaveLoadSlots == null)
		{
			List<SaveSlotVM> list2 = (SaveLoadSlots = new List<SaveSlotVM>());
		}
		slot.Active.Value = IsExpanded.Value;
		SaveLoadSlots.Add(slot);
		OnAddSave.Execute(slot);
	}

	public void HandleDeleteSave(SaveSlotVM slot)
	{
		if (SaveLoadSlots != null)
		{
			SaveLoadSlots.Remove(slot);
			if (SaveLoadSlots.Count == 0)
			{
				Dispose();
			}
		}
	}

	private void SwitchExpand(bool expand)
	{
		IsExpanded.Value = expand;
	}

	private void ExpandChanged(bool expand)
	{
		SaveLoadSlots?.ForEach(delegate(SaveSlotVM f)
		{
			f.SetAvailableAndActive(expand);
		});
	}
}
