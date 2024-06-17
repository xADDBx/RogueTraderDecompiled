using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.DialogSystem.Blueprints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.ElementsSystem;

[TypeId("5c84f2b0c2d44251b320ae180dcaecd8")]
public class Dialog : BlueprintEvaluator, IDialogReference
{
	[SerializeField]
	[FormerlySerializedAs("Value")]
	private BlueprintDialogReference m_Value;

	public BlueprintDialog Value => m_Value?.Get();

	protected override BlueprintScriptableObject GetValueInternal()
	{
		return Value;
	}

	public override string GetCaption()
	{
		return Value.NameSafe();
	}

	public DialogReferenceType GetUsagesFor(BlueprintDialog dialog)
	{
		if (dialog != Value)
		{
			return DialogReferenceType.None;
		}
		return DialogReferenceType.Start;
	}
}
