using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;

namespace Kingmaker.View.Covers;

public struct LosDescription
{
	private EntityRef<MechanicEntity>? m_ObstacleEntity;

	public readonly LosCalculations.CoverType CoverType;

	public readonly Obstacle Obstacle;

	public CustomGridNodeBase ObstacleNode => Obstacle.Node;

	[CanBeNull]
	public MechanicEntity ObstacleEntity
	{
		get
		{
			if (ObstacleNode != null)
			{
				EntityRef<MechanicEntity> valueOrDefault = m_ObstacleEntity.GetValueOrDefault();
				if (!m_ObstacleEntity.HasValue)
				{
					valueOrDefault = (MechanicEntity)(Obstacle.IsFence ? ThinCoverEntity.FindThinCover(ObstacleNode, Obstacle.FenceDirection) : (((object)DestructibleEntity.FindByNode(ObstacleNode)) ?? ((object)ObstacleNode.GetUnit())));
					m_ObstacleEntity = valueOrDefault;
				}
			}
			else
			{
				m_ObstacleEntity = null;
			}
			EntityRef<MechanicEntity>? obstacleEntity = m_ObstacleEntity;
			if (!obstacleEntity.HasValue)
			{
				return null;
			}
			return obstacleEntity.GetValueOrDefault();
		}
	}

	public LosDescription(LosCalculations.CoverType coverType, Obstacle obstacle)
	{
		CoverType = coverType;
		Obstacle = obstacle;
		m_ObstacleEntity = null;
	}

	public LosDescription(LosCalculations.CoverType coverType, [CanBeNull] CustomGridNodeBase obstacleNode, int fenceDirection = -1)
		: this(coverType, new Obstacle(obstacleNode, fenceDirection))
	{
	}

	public LosDescription(LosCalculations.CoverType coverType)
		: this(coverType, null)
	{
	}

	public static implicit operator LosDescription((LosCalculations.CoverType Type, CustomGridNodeBase Node) t)
	{
		return new LosDescription(t.Type, t.Node);
	}

	public static implicit operator LosDescription((LosCalculations.CoverType Type, Obstacle Obstacle) t)
	{
		return new LosDescription(t.Type, t.Obstacle);
	}

	public static implicit operator LosCalculations.CoverType(LosDescription description)
	{
		return description.CoverType;
	}
}
