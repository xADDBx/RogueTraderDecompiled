using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using Kingmaker.Sound.Base;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.UnitLogic.Levelup;

public class LevelUpFxController : IController, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ICloseLoadingScreenHandler, IUnitGainExperienceHandler
{
	private bool m_UpdateRequired;

	private float m_LastSFXPlayTime;

	private const float m_SoundTimeOutSeconds = 3f;

	private PrefabLink m_FxPrefab => BlueprintWarhammerRoot.Instance.LevelUpFxLibrary.LevelUpFx;

	private string m_SoundFx => BlueprintWarhammerRoot.Instance.LevelUpFxLibrary.SoundFx;

	private void CheckIsLevelUpAvailable()
	{
		foreach (BaseUnitEntity item in Game.Instance.Player.Party)
		{
			CheckUnitLevelUpAvailable(item);
		}
	}

	private void CheckUnitLevelUpAvailable(BaseUnitEntity unitEntity)
	{
		if (!unitEntity.IsDeadOrUnconscious && unitEntity.IsInPlayerParty && unitEntity.Progression.CanLevelUp && !unitEntity.CombatState.IsInCombat)
		{
			SpawnLevelUpFx(unitEntity);
			if (Time.time - m_LastSFXPlayTime > 3f)
			{
				PlaySoundFx(Game.Instance.Player.MainCharacterEntity.View);
			}
		}
	}

	private void PlaySoundFx(IEntityViewBase playerView)
	{
		if (string.IsNullOrEmpty(m_SoundFx))
		{
			PFLog.Default.Warning("LevelUpManager doesnt have SoundFx");
			return;
		}
		m_LastSFXPlayTime = Time.time;
		SoundEventsManager.PostEvent(m_SoundFx, playerView.GO);
	}

	private void SpawnLevelUpFx(BaseUnitEntity unitEntity)
	{
		if (string.IsNullOrEmpty(m_FxPrefab.AssetId))
		{
			PFLog.Default.Warning("LevelUpManager doesnt have FxPrefab");
		}
		else
		{
			FxHelper.SpawnFxOnEntity(m_FxPrefab.Load(), unitEntity.View);
		}
	}

	public void HandleUnitJoinCombat()
	{
	}

	public void HandleUnitLeaveCombat()
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity unitEntity)
		{
			CheckUnitLevelUpAvailable(unitEntity);
		}
	}

	public void HandleUnitGainExperience(int _, bool withSound = false)
	{
		if (EventInvokerExtensions.Entity is BaseUnitEntity unitEntity)
		{
			CheckUnitLevelUpAvailable(unitEntity);
		}
	}

	public void HandleCloseLoadingScreen()
	{
		CheckIsLevelUpAvailable();
	}
}
