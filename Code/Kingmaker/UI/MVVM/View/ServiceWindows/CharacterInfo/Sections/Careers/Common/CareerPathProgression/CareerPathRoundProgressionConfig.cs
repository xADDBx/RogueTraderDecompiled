using System;
using Kingmaker.UnitLogic.Progression.Paths;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression;

[Serializable]
public class CareerPathRoundProgressionConfig
{
	public CareerPathTier Tier;

	public int ItemsRadius;

	public int ProgressBarSize;

	public Sprite Icon;
}
