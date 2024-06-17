using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Progression.Paths;

[Serializable]
[TypeId("f70bef04068c49c58baf98907530f89d")]
public class BlueprintCareerPath : BlueprintPath
{
	[Serializable]
	public new class Reference : BlueprintReference<BlueprintCareerPath>
	{
	}

	public CareerPathTier Tier;

	[Header("State UI Icons")]
	public Sprite InProgressIcon;

	public Sprite FinishedIcon;

	public Sprite NotAvailableIcon;

	public bool IsHunter;
}
