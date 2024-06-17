using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/CompanionInParty")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("7aafe88b061e08e44aa3e725e8d8ff00")]
public class CompanionInParty : AbstractUnitEvaluator
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("companion")]
	[FormerlySerializedAs("m_companion")]
	private BlueprintUnitReference m_Companion;

	[Tooltip("Зарекручен и находится в активной партии")]
	public bool IncludeActive = true;

	[Tooltip("Зарекручен, находится в партии, но не управляется игроком в данный момент (стоит на месте, где находился в момент детача, пропадает из панели партии)")]
	public bool IncludeDetached;

	[Tooltip("Зарекручен, но в данный момент не находится в активной партии")]
	public bool IncludeRemote;

	[Tooltip("Анрекручен, т.е. удален из ростера")]
	public bool IncludeExCompanions;

	[Tooltip("Мертв")]
	public bool IncludeDead;

	public BlueprintUnit Companion => m_Companion;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		BaseUnitEntity baseUnitEntity = Game.Instance.Player.AllCrossSceneUnits.FirstOrDefault((BaseUnitEntity unit) => IsCompanion(unit.Blueprint));
		if (baseUnitEntity == null)
		{
			return null;
		}
		CompanionState companionState = baseUnitEntity.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None;
		if (companionState == CompanionState.InParty && !IncludeActive)
		{
			return null;
		}
		if (companionState == CompanionState.Remote && !IncludeRemote)
		{
			return null;
		}
		if (companionState == CompanionState.ExCompanion && !IncludeExCompanions)
		{
			return null;
		}
		if (companionState == CompanionState.InPartyDetached && !IncludeDetached)
		{
			return null;
		}
		if (baseUnitEntity.LifeState.IsFinallyDead && !IncludeDead)
		{
			return null;
		}
		return baseUnitEntity;
	}

	private bool IsCompanion(BlueprintUnit unit)
	{
		if (!m_Companion.Is(unit))
		{
			if (unit.PrototypeLink != null)
			{
				return IsCompanion((BlueprintUnit)unit.PrototypeLink);
			}
			return false;
		}
		return true;
	}

	public override string GetCaption()
	{
		return $"Companion ({m_Companion.Get()})";
	}
}
