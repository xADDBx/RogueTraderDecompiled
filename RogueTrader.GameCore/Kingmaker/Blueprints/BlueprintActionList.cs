using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("a80970ca06938034d8c58815e415690e")]
public class BlueprintActionList : BlueprintScriptableObject
{
	[Serializable]
	[HashRoot]
	public class Reference : BlueprintReference<BlueprintActionList>
	{
	}

	[SerializeField]
	private ActionList m_Actions;

	public void Run()
	{
		m_Actions.Run();
	}
}
