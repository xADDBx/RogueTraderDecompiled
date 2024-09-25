using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c140f075f8064f4191a3cc3d6e0dc510")]
public class ChangeVeilDamage : UnitFactComponentDelegate, ITurnEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInitiatorRulebookHandler<RuleCalculateVeilCount>, IRulebookHandler<RuleCalculateVeilCount>, IInitiatorRulebookSubscriber, IHashable
{
	public class Data : IEntityFactComponentSavableData, IHashable
	{
		public int UsedThisRound { get; set; }

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[HideIf("ReduceToZero")]
	public ContextValue VeilDamageIncrease = 0;

	public bool OnlyFirstPowerEachTurn;

	public bool ReduceToZero;

	public bool HasRandomChance;

	[ShowIf("HasRandomChance")]
	public ContextValue RandomChance;

	[SerializeField]
	private BlueprintAbilityGroupReference[] m_Groups;

	public ReferenceArrayProxy<BlueprintAbilityGroup> Groups
	{
		get
		{
			BlueprintReference<BlueprintAbilityGroup>[] groups = m_Groups;
			return groups;
		}
	}

	protected override void OnActivate()
	{
		base.OnActivate();
		base.Fact.RequestSavableData<Data>(this);
	}

	public int GetChange(AbilityData ability, bool isPrediction = false)
	{
		if (ability.Fact == null || (!Groups.Empty() && !Groups.Any((BlueprintAbilityGroup p) => ability.AbilityGroups.Contains(p))))
		{
			return 0;
		}
		Data data = base.Fact.RequestSavableData<Data>(this);
		if (OnlyFirstPowerEachTurn && data.UsedThisRound > 0)
		{
			return 0;
		}
		if (HasRandomChance)
		{
			if (isPrediction)
			{
				return 0;
			}
			if (Rulebook.Trigger(new RuleRollD100(base.Owner)).Result > RandomChance.Calculate(base.Context))
			{
				return 0;
			}
		}
		if (ReduceToZero)
		{
			return -ability.Blueprint.VeilThicknessPointsToAdd;
		}
		return VeilDamageIncrease.Calculate(base.Context);
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased && EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			RequestSavableData<Data>().UsedThisRound = 0;
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateVeilCount evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculateVeilCount evt)
	{
		RequestSavableData<Data>().UsedThisRound++;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
