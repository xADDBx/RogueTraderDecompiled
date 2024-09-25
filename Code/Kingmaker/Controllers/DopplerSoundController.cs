using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Sound.Base;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers;

public class DopplerSoundController : IControllerTick, IController, IProjectileLaunchedHandler, ISubscriber, IProjectileHitHandler
{
	private Dictionary<Projectile, uint> m_Projectiles = new Dictionary<Projectile, uint>();

	public void HandleProjectileLaunched(Projectile projectile)
	{
		if (!m_Projectiles.ContainsKey(projectile) && !(projectile.Ability == null) && projectile.Ability.FXSettings?.SoundFXSettings?.DopplerStart != null && !(projectile.Ability.FXSettings?.SoundFXSettings?.DopplerStart == ""))
		{
			uint value = SoundEventsManager.PostEvent(projectile.Ability.FXSettings.SoundFXSettings.DopplerStart, projectile.View);
			m_Projectiles.Add(projectile, value);
		}
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		List<Projectile> list = TempList.Get<Projectile>();
		foreach (KeyValuePair<Projectile, uint> projectile2 in m_Projectiles)
		{
			if (projectile2.Key == null || projectile2.Key.View == null || !projectile2.Key.View.activeSelf)
			{
				list.Add(projectile2.Key);
			}
		}
		foreach (Projectile item in list)
		{
			m_Projectiles.Remove(item);
		}
		list.Clear();
		foreach (KeyValuePair<Projectile, uint> projectile3 in m_Projectiles)
		{
			projectile3.Deconstruct(out var key, out var value);
			Projectile projectile = key;
			uint in_playingID = value;
			float in_value = Mathf.Clamp(projectile.PassedDistance / 30f, 0f, 1f);
			if (projectile.Ability?.FXSettings?.SoundFXSettings != null && projectile.Ability.FXSettings.SoundFXSettings.DopplerRTPC != "" && AkSoundEngine.SetRTPCValueByPlayingID(projectile.Ability.FXSettings.SoundFXSettings.DopplerRTPC, in_value, in_playingID) == AKRESULT.AK_PlayingIDNotFound)
			{
				list.Add(projectile);
			}
		}
		foreach (Projectile item2 in list)
		{
			m_Projectiles.Remove(item2);
		}
		list.Clear();
	}

	public void HandleProjectileHit(Projectile projectile)
	{
		if (projectile.Ability?.FXSettings == null || (m_Projectiles.ContainsKey(projectile) && !(projectile.Ability == null) && projectile.Ability.FXSettings.SoundFXSettings?.DopplerStart != null))
		{
			if (projectile.Ability?.FXSettings?.SoundFXSettings != null)
			{
				SoundEventsManager.PostEvent(projectile.Ability.FXSettings.SoundFXSettings.DopplerFinish, projectile.View);
			}
			m_Projectiles.Remove(projectile);
		}
	}
}
