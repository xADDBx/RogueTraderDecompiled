using JetBrains.Annotations;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateDodgePenetration : RulebookEvent
{
	public int ResultDodgePenetration { get; private set; }

	public RuleCalculateDodgePenetration([NotNull] IMechanicEntity initiator)
		: base(initiator)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (base.InitiatorUnit == null)
		{
			ResultDodgePenetration = 0;
			return;
		}
		int warhammerPerception = base.InitiatorUnit.Blueprint.WarhammerPerception;
		warhammerPerception += base.InitiatorUnit.GetFirstWeapon()?.GetWeaponStats().ResultDodgePenetration ?? 0;
		warhammerPerception += base.InitiatorUnit.GetSecondWeapon()?.GetWeaponStats().ResultDodgePenetration ?? 0;
		ResultDodgePenetration = warhammerPerception;
	}
}
