using UnityEngine;

namespace Kingmaker.Achievements;

public class GogAchievementsManager : MonoBehaviour
{
	public AchievementsManager Achievements { get; set; }

	public static GogAchievementsManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}
}
