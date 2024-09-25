using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartStatsSkills : EntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStatsSkills>, IEntityPartOwner
	{
		PartStatsSkills Skills { get; }
	}

	private ModifiableValueSkill[] m_List;

	private StatsContainer Container => base.ConcreteOwner.GetRequired<PartStatsContainer>().Container;

	public ModifiableValueSkill SkillAthletics => Container.GetSkill(StatType.SkillAthletics);

	public ModifiableValueSkill SkillAwareness => Container.GetSkill(StatType.SkillAwareness);

	public ModifiableValueSkill SkillCarouse => Container.GetSkill(StatType.SkillCarouse);

	public ModifiableValueSkill SkillPersuasion => Container.GetSkill(StatType.SkillPersuasion);

	public ModifiableValueSkill SkillDemolition => Container.GetSkill(StatType.SkillDemolition);

	public ModifiableValueSkill SkillCoercion => Container.GetSkill(StatType.SkillCoercion);

	public ModifiableValueSkill SkillMedicae => Container.GetSkill(StatType.SkillMedicae);

	public ModifiableValueSkill SkillLoreXenos => Container.GetSkill(StatType.SkillLoreXenos);

	public ModifiableValueSkill SkillLoreWarp => Container.GetSkill(StatType.SkillLoreWarp);

	public ModifiableValueSkill SkillLoreImperium => Container.GetSkill(StatType.SkillLoreImperium);

	public ModifiableValueSkill SkillTechUse => Container.GetSkill(StatType.SkillTechUse);

	public ModifiableValueSkill SkillCommerce => Container.GetSkill(StatType.SkillCommerce);

	public ModifiableValueSkill SkillLogic => Container.GetSkill(StatType.SkillLogic);

	public ModifiableValue CheckBluff => Container.GetStat<ModifiableValue>(StatType.CheckBluff);

	public ModifiableValue CheckDiplomacy => Container.GetStat<ModifiableValue>(StatType.CheckDiplomacy);

	public ModifiableValue CheckIntimidate => Container.GetStat<ModifiableValue>(StatType.CheckIntimidate);

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
		m_List = new ModifiableValueSkill[13]
		{
			Container.RegisterSkill(StatType.SkillAthletics),
			Container.RegisterSkill(StatType.SkillCoercion),
			Container.RegisterSkill(StatType.SkillPersuasion),
			Container.RegisterSkill(StatType.SkillAwareness),
			Container.RegisterSkill(StatType.SkillCarouse),
			Container.RegisterSkill(StatType.SkillDemolition),
			Container.RegisterSkill(StatType.SkillTechUse),
			Container.RegisterSkill(StatType.SkillCommerce),
			Container.RegisterSkill(StatType.SkillLogic),
			Container.RegisterSkill(StatType.SkillLoreWarp),
			Container.RegisterSkill(StatType.SkillLoreImperium),
			Container.RegisterSkill(StatType.SkillMedicae),
			Container.RegisterSkill(StatType.SkillLoreXenos)
		};
		Container.Register<ModifiableValue>(StatType.CheckBluff);
		Container.Register<ModifiableValue>(StatType.CheckDiplomacy);
		Container.Register<ModifiableValue>(StatType.CheckIntimidate);
	}

	public ListEnumerator<ModifiableValueSkill> GetEnumerator()
	{
		return new ListEnumerator<ModifiableValueSkill>(m_List);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
