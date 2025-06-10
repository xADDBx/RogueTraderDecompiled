using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("adf1a82937e4408caec626709bd54a92")]
public class PetOwnerByPetType : AbstractUnitEvaluator
{
	[SerializeField]
	private PetType m_PetType;

	[SerializeField]
	private bool m_UsePrioritySelection;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return (from u in Game.Instance.Player.Party
			where u != null
			where u.GetOptional<UnitPartPetOwner>()?.PetType == m_PetType
			select u).ToList().Random(PFStatefulRandom.Designers);
	}

	public override string GetCaption()
	{
		return $"Pet owner of type {m_PetType}";
	}
}
