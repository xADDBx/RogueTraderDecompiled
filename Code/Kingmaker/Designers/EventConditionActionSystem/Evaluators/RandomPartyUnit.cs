using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("4a0b4f8a27ed6e74c940f821cc147af9")]
public class RandomPartyUnit : AbstractUnitEvaluator
{
	public ConditionsChecker Conditions;

	[SerializeReference]
	public AbstractUnitEvaluator UnitIfNoVariants;

	[SerializeField]
	[FormerlySerializedAs("ForbiddenBlueprints")]
	private BlueprintUnitReference[] m_ForbiddenBlueprints;

	public ReferenceArrayProxy<BlueprintUnit> ForbiddenBlueprints
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] array = m_ForbiddenBlueprints ?? Array.Empty<BlueprintUnitReference>();
			return array;
		}
	}

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		List<BaseUnitEntity> list = (from u in Game.Instance.Player.Party
			where u != null
			where !ForbiddenBlueprints.Contains(u.Blueprint)
			select u).ToList();
		List<BaseUnitEntity> list2 = new List<BaseUnitEntity>();
		foreach (BaseUnitEntity item in list)
		{
			using (ContextData<PartyUnitData>.Request().Setup(item))
			{
				if (Conditions.Check())
				{
					list2.Add(item);
				}
			}
		}
		if (list2.Count == 0)
		{
			return UnitIfNoVariants.GetValue();
		}
		return list2.Random(PFStatefulRandom.Designers) ?? Game.Instance.Player.MainCharacterEntity;
	}

	public override string GetCaption()
	{
		return "Random Party Unit";
	}
}
