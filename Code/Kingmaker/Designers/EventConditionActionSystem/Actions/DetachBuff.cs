using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("eefa4c5532569b74d9ae18b954372790")]
[PlayerUpgraderAllowed(true)]
public class DetachBuff : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Buff")]
	private BlueprintBuffReference m_Buff;

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public BlueprintBuff Buff => m_Buff?.Get();

	public override string GetDescription()
	{
		return $"Снимает бафф {Buff} с цели {Target}";
	}

	protected override void RunAction()
	{
		GameHelper.RemoveBuff(Target.GetValue(), Buff);
	}

	public override string GetCaption()
	{
		return $"Detach Buff ({Buff})";
	}
}
