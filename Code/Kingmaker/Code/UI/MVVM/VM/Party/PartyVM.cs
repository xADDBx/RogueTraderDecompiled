using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Selection;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Party;

public class PartyVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	public readonly List<PartyCharacterVM> CharactersVM = new List<PartyCharacterVM>();

	public readonly ReactiveProperty<bool> NextEnable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> PrevEnable = new ReactiveProperty<bool>(initialValue: false);

	private int m_StartIndex;

	private List<BaseUnitEntity> m_ActualGroupCopy = new List<BaseUnitEntity>();

	private List<BaseUnitEntity> ActualGroup => Game.Instance.SelectionCharacter.ActualGroup;

	private int StartIndex
	{
		get
		{
			return m_StartIndex;
		}
		set
		{
			int num = ActualGroup.Count - 6;
			if (num < 0)
			{
				num = 0;
			}
			value = Mathf.Clamp(value, 0, num);
			m_StartIndex = value;
			PrevEnable.Value = value > 0;
			NextEnable.Value = ActualGroup.Count > m_StartIndex + 6;
			for (int i = 0; i < CharactersVM.Count; i++)
			{
				int num2 = m_StartIndex + i;
				CharactersVM[i].SetUnitData((ActualGroup.Count > num2) ? ActualGroup[num2] : null);
			}
		}
	}

	public PartyVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		for (int i = 0; i < 6; i++)
		{
			CharactersVM.Add(new PartyCharacterVM(NextPrev, i));
		}
		AddDisposable(Game.Instance.SelectionCharacter.ActualGroupUpdated.Subscribe(delegate
		{
			if (!m_ActualGroupCopy.SequenceEqual(ActualGroup))
			{
				SetGroup();
				m_ActualGroupCopy = new List<BaseUnitEntity>(ActualGroup);
			}
		}));
	}

	protected override void DisposeImplementation()
	{
		CharactersVM.ForEach(delegate(PartyCharacterVM vm)
		{
			vm.Dispose();
		});
		CharactersVM.Clear();
	}

	private void SetGroup()
	{
		StartIndex = 0;
	}

	private void NextPrev(bool dir)
	{
		if (dir)
		{
			Next();
		}
		else
		{
			Prev();
		}
	}

	public void Next()
	{
		StartIndex++;
	}

	public void Prev()
	{
		StartIndex--;
	}

	public void SelectNeighbour(bool next)
	{
		GetNeighbour(next)?.HandleUnitClick(isDoubleClick: true);
	}

	private PartyCharacterVM GetNeighbour(bool next)
	{
		int num = CharactersVM.FindIndex((PartyCharacterVM c) => c.IsSingleSelected.Value);
		if (num == -1)
		{
			return null;
		}
		int neighbourIndex = GetNeighbourIndex(num, CharactersVM.Count, next);
		while (neighbourIndex != num && !CharactersVM[neighbourIndex].UnitEntityData.IsDirectlyControllable())
		{
			neighbourIndex = GetNeighbourIndex(neighbourIndex, CharactersVM.Count, next);
		}
		return CharactersVM[neighbourIndex];
	}

	private static int GetNeighbourIndex(int current, int total, bool next)
	{
		return (current + total + (next ? 1 : (-1))) % total;
	}

	public void SwitchCharacter(bool next)
	{
		PartyCharacterVM partyCharacterVM = CharactersVM.FirstItem((PartyCharacterVM c) => c.IsSingleSelected.Value);
		if (partyCharacterVM != null)
		{
			PartyCharacterVM neighbour = GetNeighbour(next);
			if (partyCharacterVM != neighbour && neighbour != null)
			{
				SwitchCharacter(partyCharacterVM.UnitEntityData, neighbour.UnitEntityData);
			}
		}
	}

	public void SelectPrevCharacter()
	{
		SelectShiftedCharacter(-1);
	}

	public void SelectNextCharacter()
	{
		SelectShiftedCharacter(1);
	}

	private void SelectShiftedCharacter(int shift)
	{
		if (SelectionManagerBase.Instance == null || (TurnController.IsInTurnBasedCombat() && !Game.Instance.TurnController.IsPreparationTurn))
		{
			return;
		}
		ReactiveProperty<BaseUnitEntity> selectedUnit = Game.Instance.SelectionCharacter.SelectedUnit;
		List<BaseUnitEntity> list = GetSelectableUnits(Game.Instance.SelectionCharacter.ActualGroup).ToList();
		if (!list.Empty())
		{
			int num = (list.IndexOf(selectedUnit.Value) + shift) % list.Count;
			if (num < 0)
			{
				num += list.Count;
			}
			SelectionManagerBase.Instance.SelectUnit(list[num].View);
		}
	}

	private static IEnumerable<BaseUnitEntity> GetSelectableUnits(IEnumerable<BaseUnitEntity> units)
	{
		if (TurnController.IsInTurnBasedCombat() && !Game.Instance.TurnController.IsPreparationTurn)
		{
			return Enumerable.Empty<BaseUnitEntity>();
		}
		if (Game.Instance.CurrentlyLoadedArea.IsShipArea)
		{
			units = units.Where((BaseUnitEntity u) => u.IsMainCharacter);
		}
		return units.Where((BaseUnitEntity u) => u.IsInGame && u.IsDirectlyControllable());
	}

	public void SwitchCharacter(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		Game.Instance.GameCommandQueue.AddCommand(new SwitchPartyCharactersGameCommand(unit1, unit2));
	}

	public void HandleOnShowBark(string text)
	{
		Entity entity = EventInvokerExtensions.Entity;
		Entity entity2 = entity;
		if (entity2 == null || !entity2.IsInCameraFrustum)
		{
			CharactersVM.FindOrDefault((PartyCharacterVM o) => o.UnitEntityData == entity)?.ShowBark(text);
		}
	}

	public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
	}

	public void HandleOnHideBark()
	{
		Entity entity = EventInvokerExtensions.Entity;
		CharactersVM.FindOrDefault((PartyCharacterVM o) => o.UnitEntityData == entity)?.HideBark();
	}

	public void SetMassLink()
	{
		SelectionManagerConsole.Instance.SetMassLink((from c in CharactersVM
			where c.UnitEntityData.IsDirectlyControllable()
			select c.UnitEntityData).ToList());
		CharactersVM.Where((PartyCharacterVM c) => c.UnitEntityData.IsDirectlyControllable()).ForEach(delegate(PartyCharacterVM c)
		{
			c.UpdateLink();
		});
	}

	public void UpdateConsoleGroup()
	{
		for (int i = CharactersVM.Count; i < ActualGroup.Count; i++)
		{
			CharactersVM.Add(new PartyCharacterVM(NextPrev, i));
			CharactersVM[i].SetUnitData(ActualGroup[i]);
		}
	}
}
