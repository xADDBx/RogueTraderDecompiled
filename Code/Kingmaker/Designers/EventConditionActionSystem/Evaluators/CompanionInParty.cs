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

	[Tooltip("Индекс юнита, подпадающего под условия")]
	public int Index;

	public BlueprintUnit Companion => m_Companion;

	protected override AbstractUnitEntity GetAbstractUnitEntityInternal()
	{
		return Game.Instance.Player.AllCrossSceneUnits.Where(IsMatchingFilters).Skip(Index).FirstOrDefault();
	}

	private bool IsMatchingFilters(BaseUnitEntity unit)
	{
		if (!IsCompanion(unit.Blueprint))
		{
			return false;
		}
		switch (unit.GetOptional<UnitPartCompanion>()?.State ?? CompanionState.None)
		{
		case CompanionState.InParty:
			if (IncludeActive)
			{
				break;
			}
			goto IL_0063;
		case CompanionState.Remote:
			if (IncludeRemote)
			{
				break;
			}
			goto IL_0063;
		case CompanionState.ExCompanion:
			if (IncludeExCompanions)
			{
				break;
			}
			goto IL_0063;
		case CompanionState.InPartyDetached:
			{
				if (IncludeDetached)
				{
					break;
				}
				goto IL_0063;
			}
			IL_0063:
			return false;
		}
		if (unit.LifeState.IsFinallyDead)
		{
			return IncludeDead;
		}
		return true;
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
		return string.Format("Companion ({0}){1}", m_Companion.Get(), (Index > 0) ? $" #{Index}" : "");
	}
}
