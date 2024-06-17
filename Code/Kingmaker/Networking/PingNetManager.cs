using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Networking.NetGameFsm;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.InputSystems;
using UnityEngine;

namespace Kingmaker.Networking;

public sealed class PingNetManager
{
	public void PingPosition(Vector3 position)
	{
		Game.Instance.GameCommandQueue.PingPosition(position);
	}

	public void PingPositionLocally(NetPlayer player, Vector3 position)
	{
		EventBus.RaiseEvent(delegate(INetPingPosition h)
		{
			h.HandlePingPosition(player, position);
		});
	}

	public void PingEntity(Entity entity)
	{
		if (entity == null)
		{
			PFLog.Net.Error("[PingEntity] Entity is null!");
		}
		else
		{
			Game.Instance.GameCommandQueue.PingEntity(entity);
		}
	}

	public void PingEntityLocally(NetPlayer player, EntityRef entityRef)
	{
		Entity entity = entityRef;
		if (entity == null)
		{
			PFLog.Net.Error("[PingEntityLocally] Entity is null! Id=" + entityRef.Id);
			return;
		}
		EventBus.RaiseEvent(delegate(INetPingEntity h)
		{
			h.HandlePingEntity(player, entity);
		});
	}

	public void PingDialogAnswer(string answer, bool isHover)
	{
		if (!ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current)
		{
			Game.Instance.GameCommandQueue.PingDialogAnswer(answer, isHover);
		}
	}

	public void PingDialogAnswerLocally(NetPlayer player, string answer, bool isHover)
	{
		EventBus.RaiseEvent(delegate(INetPingDialogAnswer h)
		{
			h.HandleDialogAnswerHover(answer, isHover);
		});
	}

	public void PingDialogAnswerVote(string answer)
	{
		if (!ContextData<GameCommandContext>.Current && !ContextData<UnitCommandContext>.Current)
		{
			Game.Instance.GameCommandQueue.PingDialogAnswerVote(answer);
		}
	}

	public void PingDialogAnswerVoteLocally(NetPlayer player, string answer)
	{
		EventBus.RaiseEvent(delegate(INetPingDialogAnswer h)
		{
			h.HandleDialogAnswerVote(player, answer);
		});
	}

	public void PingActionBarAbility(string keyName, Entity characterEntityRef, int slotIndex)
	{
		Game.Instance.GameCommandQueue.PingActionBarAbility(keyName, characterEntityRef, slotIndex);
	}

	public void PingActionBarAbilityLocally(NetPlayer player, string keyName, EntityRef characterEntityRef, int slotIndex)
	{
		EventBus.RaiseEvent(delegate(INetPingActionBarAbility h)
		{
			h.HandlePingActionBarAbility(player, keyName, characterEntityRef, slotIndex);
		});
	}

	public bool CheckPingCoop(Action pingAction)
	{
		if (PhotonManager.NetGame.CurrentState == NetGame.State.Playing && ((Game.Instance.IsControllerMouse && KeyboardAccess.IsAltHold()) || (Game.Instance.IsControllerGamepad && Input.GetKey(KeyCode.JoystickButton4))))
		{
			pingAction();
			return true;
		}
		return false;
	}
}
