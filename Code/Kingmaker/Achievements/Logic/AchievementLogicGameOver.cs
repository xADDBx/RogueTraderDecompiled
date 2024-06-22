using JetBrains.Annotations;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;

namespace Kingmaker.Achievements.Logic;

public class AchievementLogicGameOver : AchievementLogic, IGameOverHandler, ISubscriber
{
	private readonly DifficultyPresetAsset m_MinDifficultyForAchievement;

	private readonly bool m_IronMan;

	public AchievementLogicGameOver(AchievementEntity entity, [CanBeNull] DifficultyPresetAsset minDifficultyForAchievement, bool ironMan)
		: base(entity)
	{
		m_MinDifficultyForAchievement = minDifficultyForAchievement;
		m_IronMan = ironMan;
	}

	public void HandleGameOver(Player.GameOverReasonType reason)
	{
		if (reason == Player.GameOverReasonType.Won && Game.Instance.Player.Campaign.DlcReward == null)
		{
			DifficultyPreset minDifficulty = Game.Instance.Player.MinDifficultyController.MinDifficulty;
			if ((!m_MinDifficultyForAchievement || minDifficulty.CompareTo(m_MinDifficultyForAchievement.Preset) >= 0) && (!m_IronMan || (bool)SettingsRoot.Difficulty.OnlyOneSave))
			{
				Entity.Unlock();
			}
		}
	}
}
