using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartStatsAttributes : EntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStatsAttributes>, IEntityPartOwner
	{
		PartStatsAttributes Attributes { get; }
	}

	private ModifiableValueAttributeStat[] m_List;

	private StatsContainer Container => base.ConcreteOwner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueAttributeStat WarhammerBallisticSkill => Container.GetAttribute(StatType.WarhammerBallisticSkill);

	public ModifiableValueAttributeStat WarhammerWeaponSkill => Container.GetAttribute(StatType.WarhammerWeaponSkill);

	public ModifiableValueAttributeStat WarhammerStrength => Container.GetAttribute(StatType.WarhammerStrength);

	public ModifiableValueAttributeStat WarhammerToughness => Container.GetAttribute(StatType.WarhammerToughness);

	public ModifiableValueAttributeStat WarhammerAgility => Container.GetAttribute(StatType.WarhammerAgility);

	public ModifiableValueAttributeStat WarhammerIntelligence => Container.GetAttribute(StatType.WarhammerIntelligence);

	public ModifiableValueAttributeStat WarhammerWillpower => Container.GetAttribute(StatType.WarhammerWillpower);

	public ModifiableValueAttributeStat WarhammerPerception => Container.GetAttribute(StatType.WarhammerPerception);

	public ModifiableValueAttributeStat WarhammerFellowship => Container.GetAttribute(StatType.WarhammerFellowship);

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
		m_List = new ModifiableValueAttributeStat[9]
		{
			Container.RegisterAttribute(StatType.WarhammerBallisticSkill),
			Container.RegisterAttribute(StatType.WarhammerWeaponSkill),
			Container.RegisterAttribute(StatType.WarhammerStrength),
			Container.RegisterAttribute(StatType.WarhammerToughness),
			Container.RegisterAttribute(StatType.WarhammerAgility),
			Container.RegisterAttribute(StatType.WarhammerIntelligence),
			Container.RegisterAttribute(StatType.WarhammerWillpower),
			Container.RegisterAttribute(StatType.WarhammerPerception),
			Container.RegisterAttribute(StatType.WarhammerFellowship)
		};
	}

	public ListEnumerator<ModifiableValueAttributeStat> GetEnumerator()
	{
		return new ListEnumerator<ModifiableValueAttributeStat>(m_List);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
