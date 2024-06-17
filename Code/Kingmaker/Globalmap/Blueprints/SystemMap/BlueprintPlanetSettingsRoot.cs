using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[TypeId("9d7193b600f3410d9ad21f134de3dad6")]
public class BlueprintPlanetSettingsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintPlanetSettingsRoot>
	{
	}

	[SerializeField]
	public ActionList ActionsOnScan;

	[SerializeField]
	public int GainedExp;
}
