using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.ElementsSystem;

[TypeId("1273add9ac97d9b45a4744747bbc65cb")]
public class EtudeBlueprint : BlueprintEvaluator
{
	[SerializeField]
	[FormerlySerializedAs("Value")]
	private BlueprintEtudeReference m_Value;

	public BlueprintEtude Value => m_Value?.Get();

	protected override BlueprintScriptableObject GetValueInternal()
	{
		return Value;
	}

	public override string GetCaption()
	{
		return Value.NameSafe();
	}
}
