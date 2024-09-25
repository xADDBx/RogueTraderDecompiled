using System;
using Kingmaker.Achievements;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("1074a113c72026c4082ec3dd9c085cf4")]
public class BlueprintAchievementsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintAchievementsRoot>
	{
	}

	[SerializeField]
	private AchievementDataReference[] m_List = new AchievementDataReference[0];

	public ReferenceArrayProxy<AchievementData> List
	{
		get
		{
			BlueprintReference<AchievementData>[] list = m_List;
			return list;
		}
	}
}
