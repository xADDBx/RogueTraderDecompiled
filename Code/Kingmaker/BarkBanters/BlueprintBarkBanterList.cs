using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.BarkBanters;

[Serializable]
[TypeId("8f395591128949bd93f12075ad556eac")]
public class BlueprintBarkBanterList : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintBarkBanterList>
	{
	}

	[SerializeField]
	private BlueprintBarkBanterReference[] m_BarkBanters = new BlueprintBarkBanterReference[0];

	[SerializeField]
	private float m_Weight;

	public float Weight => m_Weight;

	public IEnumerable<BlueprintBarkBanter> GetBarkBanters()
	{
		return m_BarkBanters.Dereference();
	}
}
