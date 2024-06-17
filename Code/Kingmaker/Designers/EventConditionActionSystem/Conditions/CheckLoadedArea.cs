using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Serializable]
[TypeId("77cf2f011fe687f4e86aaffaf465ee23")]
public class CheckLoadedArea : Condition
{
	[SerializeField]
	private BlueprintAreaReference m_Area;

	public BlueprintArea Area => m_Area?.Get();

	protected override string GetConditionCaption()
	{
		return $"Loaded area is ({Area})";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.CurrentlyLoadedArea == Area;
	}
}
