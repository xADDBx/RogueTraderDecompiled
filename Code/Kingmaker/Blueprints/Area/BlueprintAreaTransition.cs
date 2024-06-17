using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[TypeId("251b1bf89b36e384d8e452ce50ea1d8b")]
public class BlueprintAreaTransition : BlueprintScriptableObject
{
	[SerializeField]
	private ConditionAction[] m_Actions;

	public IEnumerable<ConditionAction> Actions => m_Actions;
}
