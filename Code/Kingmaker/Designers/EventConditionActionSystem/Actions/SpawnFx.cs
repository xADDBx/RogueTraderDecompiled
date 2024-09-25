using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Visual.Particles;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[AllowMultipleComponents]
[TypeId("c116651d4278e1143af7f7e9b229de90")]
public class SpawnFx : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public TransformEvaluator Target;

	public GameObject FxPrefab;

	protected override void RunAction()
	{
		if ((bool)Target)
		{
			Transform value = Target.GetValue();
			if ((bool)value && (bool)FxPrefab)
			{
				FxHelper.SpawnFxOnGameObject(FxPrefab, value.gameObject);
			}
		}
	}

	public override string GetCaption()
	{
		if (!FxPrefab)
		{
			return "Spawn FX ???";
		}
		return $"Spawn FX ({FxPrefab.name}) on ({Target})";
	}
}
