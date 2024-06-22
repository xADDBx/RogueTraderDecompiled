using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/SummonPoolUnits")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("5e4603a125002e449ba3cfc391334f5c")]
public class SummonPoolUnits : GameAction
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("SummonPool")]
	private BlueprintSummonPoolReference m_SummonPool;

	public ConditionsChecker Conditions;

	public ActionList Actions;

	private static readonly List<AbstractUnitEntity> UnitsList = new List<AbstractUnitEntity>(10);

	public BlueprintSummonPool SummonPool
	{
		get
		{
			return m_SummonPool?.Get();
		}
		set
		{
			m_SummonPool = SimpleBlueprintExtendAsObject.Or(value, null)?.ToReference<BlueprintSummonPoolReference>();
		}
	}

	protected override void RunAction()
	{
		IEnumerable<AbstractUnitEntity> enumerable = Game.Instance.SummonPools.Get(SummonPool)?.Units;
		if (enumerable == null)
		{
			return;
		}
		UnitsList.Clear();
		UnitsList.AddRange(enumerable);
		foreach (AbstractUnitEntity units in UnitsList)
		{
			using (ContextData<SummonPoolUnitData>.Request().Setup(units))
			{
				if (Conditions.Check())
				{
					Actions.Run();
				}
			}
		}
		UnitsList.Clear();
	}

	public override string GetCaption()
	{
		return $"Summon Pool Units ({SummonPool})";
	}
}
