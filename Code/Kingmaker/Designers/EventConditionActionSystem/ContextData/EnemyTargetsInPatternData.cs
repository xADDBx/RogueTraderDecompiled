using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class EnemyTargetsInPatternData : ContextData<EnemyTargetsInPatternData>
{
	private int m_EnemyTargetsInPattern;

	public int EnemyTargetsInPattern => m_EnemyTargetsInPattern;

	public EnemyTargetsInPatternData Setup(int enemyTargetsInPattern)
	{
		m_EnemyTargetsInPattern = enemyTargetsInPattern;
		return this;
	}

	protected override void Reset()
	{
		m_EnemyTargetsInPattern = 0;
	}
}
