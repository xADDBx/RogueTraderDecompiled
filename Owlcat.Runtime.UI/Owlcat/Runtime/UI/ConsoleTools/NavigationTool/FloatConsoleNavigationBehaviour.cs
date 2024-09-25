using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.Dependencies;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public class FloatConsoleNavigationBehaviour : ConsoleNavigationBehaviour
{
	private struct UndoEntry
	{
		public Vector2 Direction;

		public IFloatConsoleNavigationEntity Entity;
	}

	[Serializable]
	public class NavigationParameters
	{
		private enum DistanceFunctionType
		{
			Magnitude,
			Projection,
			Manhattan
		}

		private enum AngleFunctionType
		{
			Angle,
			AngleCos
		}

		[Header("History")]
		public bool ConsiderHistory = true;

		[Header("Weights")]
		[SerializeField]
		private float m_AngleWeight = 1f;

		[SerializeField]
		private float m_DistanceWeight = 1.5f;

		[SerializeField]
		private float m_NeighboursWeight = 2f;

		[Header("Bounds")]
		public int ConsideredEntitiesByDistanceCount = 3;

		[Range(5f, 90f)]
		[SerializeField]
		private float m_MaxAngleInRange = 60f;

		[Header("Calculation")]
		[SerializeField]
		private DistanceFunctionType m_DistanceFunction;

		[SerializeField]
		private AngleFunctionType m_AngleFunction;

		[Header("Axes angle offset")]
		[SerializeField]
		[Range(-45f, 45f)]
		private float m_AxesAngleOffset;

		private float m_MaxCosInRange;

		public float GetAngleCost(Vector2 first, Vector2 second, Vector2 direction)
		{
			return m_AngleFunction switch
			{
				AngleFunctionType.Angle => Vector2.Angle(second - first, direction), 
				AngleFunctionType.AngleCos => Vector2.Dot((second - first).normalized, direction), 
				_ => 0f, 
			};
		}

		public float GetDistanceCost(Vector2 first, Vector2 second, Vector2 direction)
		{
			switch (m_DistanceFunction)
			{
			case DistanceFunctionType.Magnitude:
				return (second - first).magnitude;
			case DistanceFunctionType.Projection:
				return Vector2.Dot(second - first, direction);
			case DistanceFunctionType.Manhattan:
			{
				Vector2 rhs = new Vector2(direction.y, 0f - direction.x);
				return Vector2.Dot(second - first, direction) + Mathf.Abs(Vector2.Dot(second - first, rhs));
			}
			default:
				return 0f;
			}
		}

		public bool AngleCostIsInRange(float angleCost)
		{
			return m_AngleFunction switch
			{
				AngleFunctionType.Angle => angleCost <= m_MaxAngleInRange, 
				AngleFunctionType.AngleCos => angleCost >= m_MaxCosInRange, 
				_ => false, 
			};
		}

		public bool DistanceCostIsInRange(float distanceCost, float maxDistanceCost)
		{
			return distanceCost <= maxDistanceCost;
		}

		public void UpdateAngleBounds()
		{
			m_MaxCosInRange = Mathf.Cos(m_MaxAngleInRange * (MathF.PI / 180f));
		}

		public float MoveAngleCostInRange(float angleCost)
		{
			if (m_AngleFunction == AngleFunctionType.AngleCos)
			{
				return (angleCost - m_MaxCosInRange) / (1f - m_MaxCosInRange);
			}
			return 1f - angleCost / m_MaxAngleInRange;
		}

		public float MoveDistanceCostInRange(float distanceCost, float maxDistanceCost)
		{
			return 1f - distanceCost / maxDistanceCost;
		}

		public float GetSumCost(float angleCost, float distCost, bool isNeighbour)
		{
			float num = (isNeighbour ? 1f : 0f);
			return m_AngleWeight * angleCost + m_DistanceWeight * distCost + m_NeighboursWeight * num;
		}

		public Vector2 RotateDirection(Vector2 d)
		{
			float num = Mathf.Sin(m_AxesAngleOffset * (MathF.PI / 180f));
			float num2 = Mathf.Cos(m_AxesAngleOffset * (MathF.PI / 180f));
			float x = d.x * num2 - d.y * num;
			float y = d.x * num + d.y * num2;
			return new Vector2(x, y).normalized;
		}
	}

	private List<IFloatConsoleNavigationEntity> m_Entities;

	private NavigationParameters m_NavigationParameters;

	private UndoHistory<UndoEntry> m_History;

	private readonly float m_MinAngleCost = Mathf.Cos(MathF.PI / 3f);

	public override IEnumerable<IConsoleEntity> Entities => m_Entities;

	private IFloatConsoleNavigationEntity CurrentFloatEntity => base.CurrentEntity as IFloatConsoleNavigationEntity;

	public FloatConsoleNavigationBehaviour(NavigationParameters navigationParameters, IConsoleNavigationOwner owner = null, bool playSoundOnSelect = false)
		: this(owner, playSoundOnSelect)
	{
		m_NavigationParameters = navigationParameters;
	}

	private FloatConsoleNavigationBehaviour(IConsoleNavigationOwner owner = null, bool playSoundOnSelect = false)
		: base(owner, null, playSoundOnSelect)
	{
		m_Entities = new List<IFloatConsoleNavigationEntity>();
		m_History = new UndoHistory<UndoEntry>(64);
	}

	public void AddEntities<T>(List<T> entities) where T : IFloatConsoleNavigationEntity
	{
		if (entities == null || entities.Count == 0)
		{
			UIKitLogger.Error("Cannot add an empty list");
		}
		else
		{
			entities.ForEach(AddEntity);
		}
	}

	public void AddEntities<T>(params T[] entities) where T : IFloatConsoleNavigationEntity
	{
		if (entities == null || entities.Length == 0)
		{
			UIKitLogger.Error("Cannot add an empty list");
			return;
		}
		foreach (T entities2 in entities)
		{
			AddEntity(entities2);
		}
	}

	public void AddEntity<T>(T entities) where T : IFloatConsoleNavigationEntity
	{
		if (entities == null)
		{
			UIKitLogger.Error("Cannot add null entity");
		}
		else
		{
			m_Entities.Add(entities);
		}
	}

	public void RemoveEntity(IFloatConsoleNavigationEntity entity)
	{
		if (entity == null)
		{
			UIKitLogger.Error("Cannot remove null entity");
		}
		else if (!m_Entities.Contains(entity))
		{
			UIKitLogger.Error("Cannot remove entity: it is not presented in the list");
		}
		else
		{
			m_Entities.Remove(entity);
		}
	}

	public void SelectEntityManual(IFloatConsoleNavigationEntity entity)
	{
		if (entity == null)
		{
			UIKitLogger.Error("Cannot select null entity ");
		}
		else if (!m_Entities.Contains(entity))
		{
			UIKitLogger.Error("Cannot select entity: it is not presented in the list");
		}
		else if (!entity.IsValid())
		{
			UIKitLogger.Error("Entity is not valid to select");
		}
		else
		{
			FocusOnEntity(entity);
		}
	}

	protected override IConsoleEntity GetLeftValidEntity()
	{
		return GetValidEntityInDirection(Vector2.left);
	}

	protected override IConsoleEntity GetRightValidEntity()
	{
		return GetValidEntityInDirection(Vector2.right);
	}

	protected override IConsoleEntity GetUpValidEntity()
	{
		return GetValidEntityInDirection(Vector2.up);
	}

	protected override IConsoleEntity GetDownValidEntity()
	{
		return GetValidEntityInDirection(Vector2.down);
	}

	protected override IConsoleEntity GetValidEntityInDirection(Vector2 dir)
	{
		if (!EnsureSelectedEntity())
		{
			return null;
		}
		if (dir == Vector2.zero)
		{
			UIKitLogger.Error("Cannot move in zero direction");
			return null;
		}
		dir = m_NavigationParameters.RotateDirection(dir);
		IFloatConsoleNavigationEntity floatConsoleNavigationEntity;
		if (m_NavigationParameters.ConsiderHistory && m_History.HasPrev() && Vector2.SqrMagnitude(m_History.PeekPrev().Direction + dir) < 0.001f)
		{
			floatConsoleNavigationEntity = m_History.PopPrev().Entity;
		}
		else
		{
			floatConsoleNavigationEntity = GetBestValidEntityInDirection(CurrentFloatEntity, dir);
			if (floatConsoleNavigationEntity != null && CurrentFloatEntity != null)
			{
				m_History.Add(new UndoEntry
				{
					Direction = dir,
					Entity = CurrentFloatEntity
				});
			}
		}
		return floatConsoleNavigationEntity;
	}

	private IFloatConsoleNavigationEntity GetBestValidEntityInDirection(IFloatConsoleNavigationEntity currentEntity, Vector2 direction)
	{
		List<IFloatConsoleNavigationEntity> validEntities = GetValidEntities();
		if (validEntities.Count == 0)
		{
			UIKitLogger.Error("Cannot select any entity: all entities are not valid");
			return null;
		}
		int num = validEntities.IndexOf(currentEntity);
		if (num < 0)
		{
			UIKitLogger.Error("Cannot find selected entity through valid entities");
			return null;
		}
		Vector2[] array = new Vector2[validEntities.Count];
		float[] angleCosts = new float[validEntities.Count];
		float[] distanceCosts = new float[validEntities.Count];
		bool[] isNeighbour = new bool[validEntities.Count];
		bool[] considerEntity = new bool[validEntities.Count];
		for (int j = 0; j < validEntities.Count; j++)
		{
			array[j] = validEntities[j].GetPosition();
			isNeighbour[j] = false;
			considerEntity[j] = true;
		}
		considerEntity[num] = false;
		m_NavigationParameters.UpdateAngleBounds();
		foreach (int item in ConsideredEntitiesIndices())
		{
			angleCosts[item] = m_NavigationParameters.GetAngleCost(array[num], array[item], direction);
			considerEntity[item] &= m_NavigationParameters.AngleCostIsInRange(angleCosts[item]);
		}
		if (considerEntity.All((bool value) => !value))
		{
			return null;
		}
		foreach (int item2 in ConsideredEntitiesIndices())
		{
			distanceCosts[item2] = m_NavigationParameters.GetDistanceCost(array[num], array[item2], direction);
		}
		float maxDistanceCost = (from i in ConsideredEntitiesIndices()
			select distanceCosts[i] into cost
			orderby cost
			select cost).Take(m_NavigationParameters.ConsideredEntitiesByDistanceCount).Last();
		foreach (int item3 in ConsideredEntitiesIndices())
		{
			considerEntity[item3] &= m_NavigationParameters.DistanceCostIsInRange(distanceCosts[item3], maxDistanceCost);
		}
		foreach (int item4 in ConsideredEntitiesIndices())
		{
			angleCosts[item4] = m_NavigationParameters.MoveAngleCostInRange(angleCosts[item4]);
			distanceCosts[item4] = m_NavigationParameters.MoveDistanceCostInRange(distanceCosts[item4], maxDistanceCost);
		}
		List<IFloatConsoleNavigationEntity> neighbours = validEntities[num].GetNeighbours();
		if (neighbours != null && neighbours.Count > 0)
		{
			foreach (int item5 in ConsideredEntitiesIndices())
			{
				isNeighbour[item5] = neighbours.Contains(validEntities[item5]);
			}
		}
		int index = ConsideredEntitiesIndices().MaxBy((int i) => m_NavigationParameters.GetSumCost(angleCosts[i], distanceCosts[i], isNeighbour[i]));
		return validEntities[index];
		IEnumerable<int> ConsideredEntitiesIndices()
		{
			return from i in Enumerable.Range(0, validEntities.Count)
				where considerEntity[i]
				select i;
		}
	}

	private IFloatConsoleNavigationEntity GetMiddleValidEntity()
	{
		List<IFloatConsoleNavigationEntity> validEntities = GetValidEntities();
		if (validEntities.Count == 0)
		{
			UIKitLogger.Error("Cannot select any entity: all entities are not valid");
			return null;
		}
		Vector2 avg = validEntities.Select((IFloatConsoleNavigationEntity e) => e.GetPosition()).Aggregate((Vector2 v1, Vector2 v2) => v1 + v2) / validEntities.Count;
		List<float> list = validEntities.Select((IFloatConsoleNavigationEntity e) => (e.GetPosition() - avg).magnitude).ToList();
		int index = list.IndexOf(list.Min());
		return validEntities[index];
	}

	private bool EnsureSelectedEntity()
	{
		if (CurrentFloatEntity != null && CurrentFloatEntity.IsValid())
		{
			return true;
		}
		IFloatConsoleNavigationEntity middleValidEntity = GetMiddleValidEntity();
		FocusOnEntity(middleValidEntity);
		return false;
	}

	private List<IFloatConsoleNavigationEntity> GetValidEntities()
	{
		return m_Entities.Where((IFloatConsoleNavigationEntity e) => e.IsValid()).ToList();
	}

	protected override IConsoleEntity GetFirstValidEntity()
	{
		return GetMiddleValidEntity();
	}

	protected override IConsoleEntity GetLastValidEntity()
	{
		return GetMiddleValidEntity();
	}

	public override void Clear()
	{
		ResetCurrentEntity();
		m_Entities.Clear();
	}
}
