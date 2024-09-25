using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

public class MainAnomalyFact : AnomalyFact, IHashable
{
	[JsonProperty]
	private MechanicsContext m_Context;

	public MechanicsContext Context => m_Context;

	public override MechanicsContext MaybeContext => m_Context;

	public MainAnomalyFact(BlueprintAnomalyFact blueprint)
		: base(blueprint)
	{
	}

	[JsonConstructor]
	private MainAnomalyFact()
	{
	}

	protected override void OnAttach()
	{
		base.OnAttach();
		m_Context = new MechanicsContext(null, Game.Instance.DefaultUnit, base.Blueprint);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<MechanicsContext>.GetHash128(m_Context);
		result.Append(ref val2);
		return result;
	}
}
