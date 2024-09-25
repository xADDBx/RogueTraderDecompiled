using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;

namespace Kingmaker.Pathfinding;

public class OverrideCostData
{
	private readonly RestrictionsHolder.Reference m_Restrictions;

	public int OverridePercentCost { get; }

	public EntityFactSource Source { get; }

	public NodeList Nodes
	{
		get
		{
			if (Source.Entity is AreaEffectEntity areaEffectEntity)
			{
				return areaEffectEntity.CoveredNodes;
			}
			return default(NodeList);
		}
	}

	public OverrideCostData(EntityFactSource source, int overrideCostProc, RestrictionsHolder.Reference restrictions)
	{
		Source = source;
		OverridePercentCost = overrideCostProc;
		m_Restrictions = restrictions;
	}

	public bool IsCorrectUnit(BaseUnitEntity unitEntity)
	{
		PropertyContext context = new PropertyContext(unitEntity, null, Source.Entity as AreaEffectEntity);
		return m_Restrictions?.Get()?.IsPassed(context) ?? true;
	}

	public override string ToString()
	{
		return $"Source={Source}, costBonus={(float)OverridePercentCost / 100f}, restrictions={m_Restrictions}";
	}
}
