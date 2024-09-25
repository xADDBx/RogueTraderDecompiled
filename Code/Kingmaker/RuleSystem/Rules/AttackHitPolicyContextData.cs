using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.RuleSystem.Rules;

public class AttackHitPolicyContextData : ContextData<AttackHitPolicyContextData>
{
	public AttackHitPolicyType Policy { get; private set; }

	public new static AttackHitPolicyType Current => ContextData<AttackHitPolicyContextData>.Current?.Policy ?? AttackHitPolicyType.Default;

	public AttackHitPolicyContextData Setup(AttackHitPolicyType policy)
	{
		Policy = policy;
		return this;
	}

	protected override void Reset()
	{
		Policy = AttackHitPolicyType.Default;
	}
}
