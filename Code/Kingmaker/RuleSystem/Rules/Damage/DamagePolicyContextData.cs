using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.RuleSystem.Rules.Damage;

public class DamagePolicyContextData : ContextData<DamagePolicyContextData>
{
	public DamagePolicyType Policy { get; private set; }

	public new static DamagePolicyType Current => ContextData<DamagePolicyContextData>.Current?.Policy ?? DamagePolicyType.Default;

	public DamagePolicyContextData Setup(DamagePolicyType policy)
	{
		Policy = policy;
		return this;
	}

	protected override void Reset()
	{
		Policy = DamagePolicyType.Default;
	}
}
