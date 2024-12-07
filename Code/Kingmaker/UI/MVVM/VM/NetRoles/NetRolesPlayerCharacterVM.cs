using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.NetRoles;

public class NetRolesPlayerCharacterVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INetRoleSetHandler, ISubscriber, INetRoleRegisterChangeHandler
{
	public readonly BoolReactiveProperty CanUp = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty CanDown = new BoolReactiveProperty(initialValue: false);

	public readonly BoolReactiveProperty PlayerRoleMe = new BoolReactiveProperty(initialValue: false);

	public readonly ReactiveProperty<Sprite> Portrait = new ReactiveProperty<Sprite>();

	private readonly bool m_CanUp;

	private readonly bool m_CanDown;

	private readonly bool m_IsEmpty;

	public UnitReference Character { get; }

	public PhotonActorNumber PlayerOwner { get; }

	public NetRolesPlayerCharacterVM(UnitReference character, PhotonActorNumber playerOwner)
	{
		AddDisposable(EventBus.Subscribe(this));
		Character = character;
		PlayerOwner = playerOwner;
		m_IsEmpty = character == null;
		if (!m_IsEmpty && PhotonManager.Instance.IsRoomOwner)
		{
			ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
			int num = activePlayers.FindIndex((PlayerInfo info) => info.Player.Equals(playerOwner));
			m_CanUp = num > 0;
			m_CanDown = num < activePlayers.Count - 1;
		}
		if (!m_IsEmpty)
		{
			BaseUnitEntity baseUnitEntity = (BaseUnitEntity)character.Entity;
			Portrait.Value = baseUnitEntity.Portrait.SmallPortrait;
			UpdateMoveAbility();
		}
	}

	protected override void DisposeImplementation()
	{
	}

	public void MoveRoleCharacterUp()
	{
		InternalMoveCharacter(direction: false);
	}

	public void MoveRoleCharacterDown()
	{
		InternalMoveCharacter(direction: true);
	}

	private void InternalMoveCharacter(bool direction)
	{
		ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
		int num = activePlayers.FindIndex((PlayerInfo info) => info.Player == PlayerOwner);
		MoveCharacter(activePlayers[num + (direction ? 1 : (-1))].Player);
	}

	public void MoveCharacter(PhotonActorNumber player)
	{
		if (!player.Equals(PlayerOwner))
		{
			UpdateMoveAbility(player);
		}
	}

	public void HandleRoleSet(string entityId)
	{
		if (!(Character == null) && !(Character.Id != entityId))
		{
			UpdateMoveAbility();
		}
	}

	private void UpdateMoveAbility()
	{
		PlayerRoleMe.Value = !m_IsEmpty && Game.Instance.CoopData.PlayerRole.Can(Character.Id, PlayerOwner.ToNetPlayer(NetPlayer.Empty));
		if (PhotonManager.Instance.IsRoomOwner)
		{
			CanUp.Value = m_CanUp && PlayerRoleMe.Value;
			CanDown.Value = m_CanDown && PlayerRoleMe.Value;
		}
	}

	private void UpdateMoveAbility(PhotonActorNumber player)
	{
		EventBus.RaiseEvent(delegate(INetRoleRegisterChangeHandler h)
		{
			h.HandleRoleRegisterChange(Character.Id, player);
		});
	}

	public void HandleRoleRegisterChange(string characterId, PhotonActorNumber newPlayerOwner)
	{
		if (!(characterId != Character.Id))
		{
			PlayerRoleMe.Value = !m_IsEmpty && newPlayerOwner.Equals(PlayerOwner);
			if (PhotonManager.Instance.IsRoomOwner)
			{
				CanUp.Value = m_CanUp && PlayerRoleMe.Value;
				CanDown.Value = m_CanDown && PlayerRoleMe.Value;
			}
		}
	}
}
