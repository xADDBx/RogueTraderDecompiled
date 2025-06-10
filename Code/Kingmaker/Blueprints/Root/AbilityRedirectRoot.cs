using System;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("15f941a084104e84bdecb59b311e2c5d")]
public class AbilityRedirectRoot : BlueprintScriptableObject
{
	[Serializable]
	public sealed class Reference : BlueprintReference<AbilityRedirectRoot>
	{
	}

	[Tooltip("В какой области искать цели для редиректа.")]
	public AoEPattern Pattern;

	[Tooltip("FX проигрываемый на основной цели при редиректе.")]
	public PrefabLink Fx;

	[Tooltip("Абилка будет редиректиться если на таргене есть этот факт.")]
	[SerializeField]
	private BlueprintUnitFactReference m_AllowRedirectFact;

	public BlueprintUnitFact AllowRedirectFact => m_AllowRedirectFact;
}
