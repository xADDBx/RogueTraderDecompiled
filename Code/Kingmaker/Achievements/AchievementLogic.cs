using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.Difficulty;

namespace Kingmaker.Achievements;

public abstract class AchievementLogic
{
	public readonly AchievementEntity Entity;

	private bool m_Active;

	public AchievementType Type => Entity.Data.Type;

	protected AchievementLogic(AchievementEntity entity)
	{
		Entity = entity;
	}

	public void Activate()
	{
		if (!m_Active)
		{
			EventBus.Subscribe(this);
			m_Active = true;
		}
	}

	public void Deactivate()
	{
		if (m_Active)
		{
			EventBus.Unsubscribe(this);
			m_Active = false;
		}
	}

	public virtual bool IsDifficultyDisableAchievement(DifficultyPresetAsset difficulty)
	{
		return false;
	}
}
