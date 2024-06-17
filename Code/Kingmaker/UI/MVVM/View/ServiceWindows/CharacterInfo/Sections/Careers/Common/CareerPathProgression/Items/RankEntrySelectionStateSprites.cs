using System;
using Kingmaker.UnitLogic.Levelup.Selections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;

[Serializable]
public class RankEntrySelectionStateSprites
{
	public FeatureGroup FeatureGroup;

	[FormerlySerializedAs("Selectable")]
	public Sprite Icon;
}
