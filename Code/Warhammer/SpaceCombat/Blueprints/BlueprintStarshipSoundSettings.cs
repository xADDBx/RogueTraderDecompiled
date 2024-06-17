using System;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("31037214c438fde4ea8ace268b5db905")]
public class BlueprintStarshipSoundSettings : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintStarshipSoundSettings>
	{
	}

	[SerializeField]
	[AkEventReference]
	private string m_PlayEvent;

	[SerializeField]
	[AkEventReference]
	private string m_StopEvent;

	[SerializeField]
	[AkSwitchGroupReference]
	private string m_SwitchGroupEngineStatus;

	[SerializeField]
	[AkGameParameterReference]
	private string m_RtpcMovementStatus;

	[SerializeField]
	[AkGameParameterReference]
	private string m_RtpcStarshipHealth;

	public uint Play(StarshipEntity entity)
	{
		uint num = SoundEventsManager.PostEvent(m_PlayEvent, entity.View.gameObject);
		PFLog.Audio.Log($"PLAY (id={num}): {m_PlayEvent} on {entity}");
		return num;
	}

	public void Stop(uint playingId)
	{
		PFLog.Audio.Log($"STOP (id={playingId})");
		SoundEventsManager.StopPlayingById(playingId);
	}

	public void SetParamEngineStatus(StarshipEntity entity, bool isTurnedOn)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetSwitch(m_SwitchGroupEngineStatus, isTurnedOn ? "On" : "Off", entity.View.gameObject);
		PFLog.Audio.Log(string.Format("Set {0} to {1} on {2}, result={3}", m_SwitchGroupEngineStatus, isTurnedOn ? "On" : "Off", entity, aKRESULT));
	}

	public void SetParamMovementStatus(uint playingId, bool isMoving)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetRTPCValueByPlayingID(m_RtpcMovementStatus, isMoving ? 1f : 0f, playingId);
		PFLog.Audio.Log($"Set {m_RtpcMovementStatus} to {(isMoving ? 1f : 0f)} on {playingId}, result={aKRESULT}");
	}

	public void SetParamStarshipHealth(uint playingId, float healthPercent)
	{
		AKRESULT aKRESULT = AkSoundEngine.SetRTPCValueByPlayingID(m_RtpcStarshipHealth, healthPercent, playingId);
		PFLog.Audio.Log($"Set {m_RtpcStarshipHealth} to {healthPercent} on {playingId}, result={aKRESULT}");
	}
}
