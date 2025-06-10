using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.UI.MVVM;
using Photon.Realtime;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.GroupChanger;

public abstract class GroupChangerVM : VMBase, IAcceptChangeGroupHandler, ISubscriber, IChangeGroupHandler, ICloseChangeGroupHandler, INetLobbyPlayersHandler
{
	private Action m_ActionGo;

	private Action m_ActionClose;

	public readonly bool IsCapital;

	protected readonly List<UnitReference> LastUnits = new List<UnitReference>();

	protected readonly List<BlueprintUnit> RequiredUnits = new List<BlueprintUnit>();

	protected readonly ReactiveCollection<GroupChangerCharacterVM> m_PartyCharacter = new ReactiveCollection<GroupChangerCharacterVM>();

	protected readonly ReactiveCollection<GroupChangerCharacterVM> m_RemoteCharacter = new ReactiveCollection<GroupChangerCharacterVM>();

	protected readonly ReactiveProperty<bool> m_CloseEnabled = new ReactiveProperty<bool>(initialValue: true);

	private List<UnitReference> m_OverridePartyCharacters;

	private List<UnitReference> m_OverrideRemoteCharacters;

	public readonly BoolReactiveProperty CloseActionsIsSame = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsMainCharacter = new BoolReactiveProperty();

	public IReadOnlyReactiveCollection<GroupChangerCharacterVM> PartyCharacter => m_PartyCharacter;

	public IReadOnlyReactiveCollection<GroupChangerCharacterVM> RemoteCharacter => m_RemoteCharacter;

	public IEnumerable<UnitReference> PartyCharacterRef => m_PartyCharacter.Select((GroupChangerCharacterVM p) => p.UnitRef);

	public IEnumerable<UnitReference> PartyNavigatorsCharacterRef => PartyCharacterRef.Where((UnitReference p) => p.Entity.ToBaseUnitEntity().IsNavigatorCompanion());

	public IEnumerable<UnitReference> RemoteCharacterRef => m_RemoteCharacter.Select((GroupChangerCharacterVM p) => p.UnitRef);

	public IEnumerable<BlueprintUnitReference> RequiredCharactersRef => RequiredUnits.Select((BlueprintUnit p) => p.ToReference<BlueprintUnitReference>());

	public IReadOnlyReactiveProperty<bool> CloseEnabled => m_CloseEnabled;

	protected GroupChangerVM(Action go, Action close, List<UnitReference> lastUnits, List<BlueprintUnit> requiredUnits, bool isCapital = false, bool sameFinishActions = false, bool closeEnabled = true)
	{
		m_ActionGo = go;
		m_ActionClose = close;
		CloseActionsIsSame.Value = sameFinishActions;
		LastUnits.AddRange(lastUnits);
		RequiredUnits.AddRange(requiredUnits);
		IsCapital = isCapital;
		m_CloseEnabled.Value = closeEnabled;
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		LastUnits.Clear();
		RequiredUnits.Clear();
		m_PartyCharacter.ForEach(delegate(GroupChangerCharacterVM c)
		{
			c.Dispose();
		});
		m_PartyCharacter.Clear();
		m_RemoteCharacter.ForEach(delegate(GroupChangerCharacterVM c)
		{
			c.Dispose();
		});
		m_RemoteCharacter.Clear();
	}

	public void AddActionGo(Action go)
	{
		m_ActionGo = (Action)Delegate.Combine(m_ActionGo, go);
	}

	public void AddActionClose(Action go)
	{
		m_ActionClose = (Action)Delegate.Combine(m_ActionClose, go);
	}

	void IAcceptChangeGroupHandler.HandleAcceptChangeGroup()
	{
		try
		{
			m_ActionGo?.Invoke();
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex);
		}
	}

	void ICloseChangeGroupHandler.HandleCloseChangeGroup()
	{
		Close();
	}

	public void Close()
	{
		if (!CloseCondition())
		{
			return;
		}
		try
		{
			m_ActionClose?.Invoke();
		}
		catch (Exception ex)
		{
			LogChannel.Default.Exception(ex);
		}
	}

	public virtual bool CloseCondition()
	{
		return false;
	}

	public void OnSelectedClick()
	{
		foreach (GroupChangerCharacterVM item in RemoteCharacter)
		{
			if (item.IsFocused)
			{
				item.OnClick();
				return;
			}
		}
		foreach (GroupChangerCharacterVM item2 in PartyCharacter)
		{
			if (item2.IsFocused)
			{
				item2.OnClick();
				break;
			}
		}
	}

	protected void MoveCharacter(UnitReference unitReference)
	{
		if (UINetUtility.IsControlMainCharacterWithWarning())
		{
			Game.Instance.GameCommandQueue.GroupChanger(unitReference);
		}
	}

	void IChangeGroupHandler.HandleChangeGroup(string unitUniqueId)
	{
		UnitReference unitReference = new UnitReference(unitUniqueId);
		GroupChangerCharacterVM groupChangerCharacterVM = m_PartyCharacter.FirstOrDefault((GroupChangerCharacterVM vm) => vm.UnitRef == unitReference);
		if (groupChangerCharacterVM != null)
		{
			if (!groupChangerCharacterVM.IsLock.Value)
			{
				MoveCharacterFromPartyToRemote(groupChangerCharacterVM);
			}
			return;
		}
		string cantMove = CanMoveCharacterFromRemoteToParty(unitReference);
		if (!string.IsNullOrEmpty(cantMove))
		{
			if (IsMainCharacter.Value)
			{
				EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
				{
					h.HandleWarning(cantMove);
				});
			}
			return;
		}
		groupChangerCharacterVM = m_RemoteCharacter.FirstOrDefault((GroupChangerCharacterVM vm) => vm.UnitRef == unitReference);
		if (groupChangerCharacterVM != null)
		{
			MoveCharacterFromRemoteToParty(groupChangerCharacterVM);
		}
	}

	protected virtual string CanMoveCharacterFromRemoteToParty(UnitReference unitReference)
	{
		if (m_PartyCharacter.Count == 6)
		{
			return UIStrings.Instance.GroupChangerTexts.MaxGroupCountWarning;
		}
		if (unitReference.Entity.ToBaseUnitEntity().IsNavigatorCompanion() && PartyNavigatorsCharacterRef.Count() >= 1)
		{
			return UIStrings.Instance.GroupChangerTexts.MaxNavigatorsCountWarning;
		}
		return null;
	}

	protected virtual void MoveCharacterFromPartyToRemote(GroupChangerCharacterVM characterVm)
	{
		m_PartyCharacter.Remove(characterVm);
		m_RemoteCharacter.Add(characterVm);
		characterVm.SetIsInParty(isInParty: false);
	}

	protected virtual void MoveCharacterFromRemoteToParty(GroupChangerCharacterVM characterVm)
	{
		m_RemoteCharacter.Remove(characterVm);
		m_PartyCharacter.Add(characterVm);
		characterVm.SetIsInParty(isInParty: true);
	}

	public void HandlePlayerEnteredRoom(Photon.Realtime.Player player)
	{
	}

	public void HandlePlayerLeftRoom(Photon.Realtime.Player player)
	{
		IsMainCharacter.Value = UINetUtility.IsControlMainCharacter();
	}

	public void HandlePlayerChanged()
	{
	}

	public void HandleLastPlayerLeftLobby()
	{
	}

	public void HandleRoomOwnerChanged()
	{
	}
}
