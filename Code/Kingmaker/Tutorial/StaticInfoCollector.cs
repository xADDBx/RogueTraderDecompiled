using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Tutorial;

public class StaticInfoCollector : IGameOverHandler, ISubscriber
{
	private const string BattleDeathCountKey = "BattleDeathCount";

	public int BattleDeathCount
	{
		get
		{
			return PlayerPrefs.GetInt("BattleDeathCount", 0);
		}
		set
		{
			PlayerPrefs.SetInt("BattleDeathCount", value);
		}
	}

	public StaticInfoCollector()
	{
		EventBus.Subscribe(this);
	}

	public void Quit()
	{
		EventBus.Unsubscribe(this);
	}

	public void HandleGameOver(Player.GameOverReasonType reason)
	{
		if ((reason == Player.GameOverReasonType.EssentialUnitIsDead || reason == Player.GameOverReasonType.PartyIsDefeated) && ((bool)Game.Instance.Player.Group.IsInCombat || Game.Instance.Player.Group.Memory.Enemies.Count > 0))
		{
			BattleDeathCount++;
		}
	}
}
