using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("95470379bdf243da8f2e91c2bc6c5409")]
public class UnitNotMovingTrigger : UnitFactComponentDelegate, IHashable
{
	public int TimerValue;

	[SerializeReference]
	[ValidateNotNull]
	public AbstractUnitEvaluator TargetUnit;

	public ActionList TimerElapsedActions;

	public ActionList AbortActions;

	public bool AllowDuringDialogue;

	public CutsceneReference ReactAnimationStart;

	public CutsceneReference ReactAnimationAbort;

	protected override void OnActivateOrPostLoad()
	{
		if (base.IsReapplying)
		{
			return;
		}
		using (ContextData<PropertyContextData>.Request().Setup(new PropertyContext(base.Owner, base.Fact, null, base.Context)))
		{
			TargetUnit.GetValue().GetOrCreate<UnitPartNotMoveTrigger>().Add(base.Fact, AllowDuringDialogue, this);
		}
	}

	protected override void OnDeactivate()
	{
		if (base.IsReapplying)
		{
			return;
		}
		using (ContextData<PropertyContextData>.Request().Setup(new PropertyContext(base.Owner, base.Fact, null, base.Context)))
		{
			TargetUnit.GetValue().GetOptional<UnitPartNotMoveTrigger>()?.Remove(base.Fact, this);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
