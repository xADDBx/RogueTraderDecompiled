using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Achievements.Logic;

public class AchievementLogicUnlockableFlags : AchievementLogic, IUnlockValueHandler, ISubscriber
{
	public AchievementLogicUnlockableFlags(AchievementEntity entity)
		: base(entity)
	{
	}

	public void HandleFlagValue(BlueprintUnlockableFlag flag, int value)
	{
		if (Entity.Data.IsFlagsUnlocked)
		{
			Entity.Unlock();
		}
	}
}
