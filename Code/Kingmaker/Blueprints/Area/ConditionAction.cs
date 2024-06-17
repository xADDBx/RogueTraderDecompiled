using System;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[Serializable]
public class ConditionAction
{
	[SerializeReference]
	public Condition Condition;

	public ActionList Actions;
}
