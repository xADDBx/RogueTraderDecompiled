using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartStatsSaves : EntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStatsSaves>, IEntityPartOwner
	{
		PartStatsSaves Saves { get; }
	}

	private ModifiableValueSavingThrow[] m_List;

	private StatsContainer Container => base.ConcreteOwner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueSavingThrow SaveFortitude => Container.GetStat<ModifiableValueSavingThrow>(StatType.SaveFortitude);

	public ModifiableValueSavingThrow SaveReflex => Container.GetStat<ModifiableValueSavingThrow>(StatType.SaveReflex);

	public ModifiableValueSavingThrow SaveWill => Container.GetStat<ModifiableValueSavingThrow>(StatType.SaveWill);

	protected override void OnAttach()
	{
		Initialize();
	}

	protected override void OnPrePostLoad()
	{
		Initialize();
	}

	private void Initialize()
	{
		m_List = new ModifiableValueSavingThrow[3]
		{
			Container.Register<ModifiableValueSavingThrow>(StatType.SaveFortitude),
			Container.Register<ModifiableValueSavingThrow>(StatType.SaveReflex),
			Container.Register<ModifiableValueSavingThrow>(StatType.SaveWill)
		};
	}

	public ListEnumerator<ModifiableValueSavingThrow> GetEnumerator()
	{
		return new ListEnumerator<ModifiableValueSavingThrow>(m_List);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
