using JetBrains.Annotations;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/Kill")]
[AllowMultipleComponents]
[TypeId("abb0dcfdb51f3594ab0d2b1d28ecc782")]
public class Kill : GameAction
{
	public class SilentDeathUnitPart : BaseUnitPart, IHashable
	{
		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	[SerializeReference]
	public MechanicEntityEvaluator Killer;

	[Tooltip("Works only if the Killer is set. If 0, body just falls on the ground, 1 is a standard impulse. For bigger impulse, try set it up a bit higher.")]
	public int ImpulseMultiplier = 1;

	public UnitDismemberType Dismember;

	[ShowIf("LimpsApartSelected")]
	[SerializeField]
	private DismembermentLimbsApartType m_DismemberingAnimation;

	public bool DisableBattleLog;

	public bool RemoveExp = true;

	private bool LimpsApartSelected => Dismember == UnitDismemberType.LimbsApart;

	[UsedImplicitly]
	private bool HasKiller => Killer;

	public override string GetDescription()
	{
		return $"Убивает цель {Target}";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Target.GetValue();
		if (DisableBattleLog)
		{
			value.GetOrCreate<SilentDeathUnitPart>();
		}
		if (RemoveExp && value is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.GiveExperienceOnDeath = false;
		}
		GameHelper.KillUnit(value, Killer ? Killer.GetValue() : null, ImpulseMultiplier, Dismember, LimpsApartSelected ? new DismembermentLimbsApartType?(m_DismemberingAnimation) : null);
	}

	public override string GetCaption()
	{
		return $"Kill ({Target})";
	}
}
