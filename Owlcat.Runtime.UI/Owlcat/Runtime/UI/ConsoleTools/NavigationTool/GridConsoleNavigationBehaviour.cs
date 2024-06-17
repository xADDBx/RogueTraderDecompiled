using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.Utility;
using Owlcat.Runtime.UI.VirtualListSystem;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

public class GridConsoleNavigationBehaviour : ConsoleNavigationBehaviour
{
	private readonly List<List<IConsoleEntity>> m_Entities = new List<List<IConsoleEntity>>();

	private Vector2Int m_Cyclical;

	private bool m_LineGrid;

	private int m_ColunmnsCount = 1;

	public override IEnumerable<IConsoleEntity> Entities => m_Entities.SelectMany((List<IConsoleEntity> list) => list);

	public GridConsoleNavigationBehaviour(IConsoleNavigationOwner owner = null, IConsoleNavigationScroll scroll = null, Vector2Int cyclical = default(Vector2Int), bool lineGrid = false, bool playSoundOnSelect = false)
		: base(owner, scroll, playSoundOnSelect)
	{
		m_Cyclical = cyclical;
		m_LineGrid = lineGrid;
	}

	public GridConsoleNavigationBehaviour(List<List<IConsoleEntity>> entities, IConsoleNavigationOwner owner = null, IConsoleNavigationScroll scroll = null, Vector2Int cyclical = default(Vector2Int), bool lineGrid = false)
		: this(owner, scroll, cyclical, lineGrid)
	{
		SetEntities(entities);
	}

	public void SetEntities<TEntity>(params TEntity[][] entities) where TEntity : IConsoleEntity
	{
		Clear();
		foreach (TEntity[] obj in entities)
		{
			List<IConsoleEntity> list = new List<IConsoleEntity>();
			TEntity[] array = obj;
			foreach (TEntity val in array)
			{
				list.Add(val);
			}
			m_Entities.Add(new List<IConsoleEntity>(list));
		}
	}

	public void SetEntities<TEntity>(List<List<TEntity>> entities) where TEntity : IConsoleEntity
	{
		Clear();
		foreach (List<TEntity> entity in entities)
		{
			List<IConsoleEntity> list = new List<IConsoleEntity>();
			foreach (TEntity item in entity)
			{
				list.Add(item);
			}
			m_Entities.Add(new List<IConsoleEntity>(list));
		}
	}

	public void SetEntitiesVertical<TEntity>(List<TEntity> entities) where TEntity : IConsoleEntity
	{
		SetEntitiesVertical(entities.ToArray());
	}

	public void SetEntitiesVertical<TEntity>(params TEntity[] entities) where TEntity : IConsoleEntity
	{
		Clear();
		foreach (TEntity val in entities)
		{
			m_Entities.Add(new List<IConsoleEntity> { val });
		}
	}

	public void SetEntitiesHorizontal<TEntity>(params TEntity[] entities) where TEntity : IConsoleEntity
	{
		SetEntitiesHorizontal(entities.ToList());
	}

	public void SetEntitiesHorizontal<TEntity>(List<TEntity> entities) where TEntity : IConsoleEntity
	{
		Clear();
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		foreach (TEntity entity in entities)
		{
			list.Add(entity);
		}
		m_Entities.Add(new List<IConsoleEntity>(list));
	}

	public void SetEntitiesGrid<TEntity>(List<TEntity> entities, int columnsCount) where TEntity : IConsoleEntity
	{
		m_ColunmnsCount = columnsCount;
		SetEntitiesGrid(entities.ToArray(), columnsCount);
	}

	public void SetEntitiesGrid<TEntity>(TEntity[] entities, int columnsCount) where TEntity : IConsoleEntity
	{
		m_ColunmnsCount = columnsCount;
		Clear();
		int num = 0;
		int num2 = 0;
		foreach (TEntity val in entities)
		{
			if (m_Entities.Count <= num2)
			{
				m_Entities.Add(new List<IConsoleEntity>());
			}
			m_Entities[num2].Add(val);
			num++;
			if (columnsCount == num)
			{
				num = 0;
				num2++;
			}
		}
	}

	public void InsertInGrid(int row, int column, IConsoleEntity entity, bool elementsInRowCountStays = false)
	{
		if (row >= m_Entities.Count)
		{
			row = m_Entities.Count;
			m_Entities.Insert(row, new List<IConsoleEntity>());
		}
		if (column >= m_Entities[row].Count)
		{
			column = m_Entities[row].Count;
		}
		m_Entities[row].Insert(column, entity);
		if (elementsInRowCountStays)
		{
			List<IConsoleEntity> entities = Entities.ToList();
			SetEntitiesGrid(entities, m_ColunmnsCount);
		}
	}

	public void InsertVertical(int index, IConsoleEntity entity)
	{
		if (index >= m_Entities.Count)
		{
			index = m_Entities.Count;
		}
		m_Entities.Insert(index, new List<IConsoleEntity>(new IConsoleEntity[1] { entity }));
	}

	public void InsertHorizontal(int index, IConsoleEntity entity)
	{
		if (m_Entities.Count == 0)
		{
			m_Entities.Add(new List<IConsoleEntity>());
		}
		if (index >= m_Entities[0].Count)
		{
			index = m_Entities[0].Count;
		}
		m_Entities[0].Insert(index, entity);
	}

	public void AddEntityVertical(IConsoleEntity entity)
	{
		m_Entities.Add(new List<IConsoleEntity>(new IConsoleEntity[1] { entity }));
	}

	public void AddEntityHorizontal(IConsoleEntity entity)
	{
		if (m_Entities.Count == 0)
		{
			m_Entities.Add(new List<IConsoleEntity>());
		}
		m_Entities[0].Add(entity);
	}

	public void AddEntityGrid(IConsoleEntity entity, bool elementsInRowCountStays = false)
	{
		if (m_Entities.Count == 0)
		{
			m_Entities.Add(new List<IConsoleEntity>());
		}
		m_Entities[m_Entities.Count - 1].Add(entity);
		if (elementsInRowCountStays)
		{
			List<IConsoleEntity> entities = Entities.ToList();
			SetEntitiesGrid(entities, m_ColunmnsCount);
		}
	}

	public void AddRow<TEntity>(params TEntity[] entities) where TEntity : IConsoleEntity
	{
		AddRow(entities.ToList());
	}

	public void AddRow<TEntity>(List<TEntity> entities) where TEntity : IConsoleEntity
	{
		m_Entities.Add(entities.Cast<IConsoleEntity>().ToList());
	}

	public void InsertRow<TEntity>(int rowNum, params TEntity[] entities) where TEntity : IConsoleEntity
	{
		InsertRow(rowNum, entities.ToList());
	}

	public void InsertRow<TEntity>(int rowNum, List<TEntity> entities) where TEntity : IConsoleEntity
	{
		m_Entities.Insert(rowNum, entities.Cast<IConsoleEntity>().ToList());
	}

	public void AddColumn<TEntity>(params TEntity[] entities) where TEntity : IConsoleEntity
	{
		AddColumn(entities.ToList());
	}

	public void AddColumn<TEntity>(List<TEntity> entities) where TEntity : IConsoleEntity
	{
		if (entities == null || entities.Count == 0)
		{
			return;
		}
		if (m_Entities.Count < entities.Count)
		{
			for (int i = m_Entities.Count; i < entities.Count; i++)
			{
				m_Entities.Add(new List<IConsoleEntity>());
			}
		}
		int count = m_Entities.MaxBy((List<IConsoleEntity> list) => list.Count).Count;
		foreach (List<IConsoleEntity> entity in m_Entities)
		{
			if (entity.Count != count)
			{
				for (int j = entity.Count; j < count; j++)
				{
					entity.Add(new EmptyConsoleNavigationEntity());
				}
			}
		}
		for (int k = 0; k < entities.Count || k < m_Entities.Count; k++)
		{
			IConsoleEntity consoleEntity2;
			if (k >= entities.Count)
			{
				IConsoleEntity consoleEntity = new EmptyConsoleNavigationEntity();
				consoleEntity2 = consoleEntity;
			}
			else
			{
				IConsoleEntity consoleEntity = entities[k];
				consoleEntity2 = consoleEntity;
			}
			IConsoleEntity item = consoleEntity2;
			m_Entities[k].Add(item);
		}
	}

	public void RemoveAtGrid(int row, int column, bool elementsInRowCountStays = false)
	{
		if (row < m_Entities.Count && column < m_Entities[row].Count)
		{
			if (base.CurrentEntity == m_Entities[row][column])
			{
				SelectClosestEntity(base.CurrentEntity);
			}
			m_Entities[row].RemoveAt(column);
			if (elementsInRowCountStays)
			{
				List<IConsoleEntity> entities = Entities.ToList();
				SetEntitiesGrid(entities, m_ColunmnsCount);
			}
		}
	}

	public void RemoveAtVertical(int index)
	{
		if (index < m_Entities.Count)
		{
			if (base.CurrentEntity == m_Entities[index][0])
			{
				SelectClosestEntity(base.CurrentEntity);
			}
			m_Entities.RemoveAt(index);
		}
	}

	public void RemoveAtHorizontal(int index)
	{
		if (m_Entities.Count != 0 && m_Entities[0].Count > index)
		{
			if (base.CurrentEntity == m_Entities[0][index])
			{
				SelectClosestEntity(base.CurrentEntity);
			}
			m_Entities[0].RemoveAt(index);
		}
	}

	public bool RemoveEntity(IConsoleEntity entity)
	{
		for (int i = 0; i < m_Entities.Count; i++)
		{
			for (int j = 0; j < m_Entities[i].Count; j++)
			{
				if (m_Entities[i][j] == entity)
				{
					if (base.CurrentEntity == m_Entities[i][j])
					{
						SelectClosestEntity(base.CurrentEntity);
					}
					m_Entities[i].RemoveAt(j);
					if (m_Entities[i].Count == 0)
					{
						m_Entities.RemoveAt(i);
					}
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveEntityGrid(IConsoleEntity entity, bool elementsInRowCountStays = false)
	{
		RemoveEntity(entity);
		if (elementsInRowCountStays)
		{
			int index = Entities.IndexOf(base.Focus.Value);
			List<IConsoleEntity> entities = Entities.ToList();
			SetEntitiesGrid(entities, m_ColunmnsCount);
			FocusOnEntityManual(Entities.ElementAtOrDefault(index));
		}
	}

	protected override IConsoleEntity GetLeftValidEntity()
	{
		foreach (List<IConsoleEntity> entity in m_Entities)
		{
			if (!entity.Contains(base.CurrentEntity))
			{
				continue;
			}
			int num = entity.IndexOf(base.CurrentEntity);
			if (entity.FirstOrDefault((IConsoleEntity e) => e.IsValid()) == base.CurrentEntity)
			{
				if (m_LineGrid)
				{
					for (int num2 = m_Entities.IndexOf(entity) - 1; num2 >= 0; num2--)
					{
						for (int num3 = m_Entities[num2].Count - 1; num3 >= 0; num3--)
						{
							IConsoleEntity consoleEntity = m_Entities[num2][num3];
							if (consoleEntity != null && consoleEntity.IsValid())
							{
								return m_Entities[num2][num3];
							}
						}
					}
					break;
				}
				if (m_Cyclical.x == 1)
				{
					return entity.LastOrDefault((IConsoleEntity e) => e.IsValid());
				}
				if (num <= 0)
				{
					break;
				}
				for (int num4 = m_Entities.IndexOf(entity) - 1; num4 >= 0; num4--)
				{
					for (int num5 = num - 1; num5 >= 0; num5--)
					{
						IConsoleEntity consoleEntity2 = m_Entities[num4][num5];
						if (consoleEntity2 != null && consoleEntity2.IsValid())
						{
							return m_Entities[num4][num5];
						}
					}
				}
				break;
			}
			for (int num6 = num - 1; num6 >= 0; num6--)
			{
				IConsoleEntity consoleEntity3 = entity[num6];
				if (consoleEntity3 != null && consoleEntity3.IsValid())
				{
					return entity[num6];
				}
			}
		}
		return null;
	}

	protected override IConsoleEntity GetRightValidEntity()
	{
		foreach (List<IConsoleEntity> entity in m_Entities)
		{
			if (!entity.Contains(base.CurrentEntity))
			{
				continue;
			}
			int num = entity.IndexOf(base.CurrentEntity);
			if (entity.LastOrDefault((IConsoleEntity e) => e.IsValid()) == base.CurrentEntity)
			{
				if (m_LineGrid)
				{
					for (int i = m_Entities.IndexOf(entity) + 1; i < m_Entities.Count; i++)
					{
						for (int j = 0; j < m_Entities[i].Count; j++)
						{
							IConsoleEntity consoleEntity = m_Entities[i][j];
							if (consoleEntity != null && consoleEntity.IsValid())
							{
								return m_Entities[i][j];
							}
						}
					}
					break;
				}
				if (m_Cyclical.x == 1)
				{
					return entity.FirstOrDefault((IConsoleEntity e) => e.IsValid());
				}
				if (num <= 0)
				{
					break;
				}
				for (int num2 = m_Entities.IndexOf(entity) - 1; num2 >= 0; num2--)
				{
					for (int k = num + 1; k < m_Entities[num2].Count; k++)
					{
						IConsoleEntity consoleEntity2 = m_Entities[num2][k];
						if (consoleEntity2 != null && consoleEntity2.IsValid())
						{
							return m_Entities[num2][k];
						}
					}
				}
				break;
			}
			for (int l = num + 1; l < entity.Count; l++)
			{
				IConsoleEntity consoleEntity3 = entity[l];
				if (consoleEntity3 != null && consoleEntity3.IsValid())
				{
					return entity[l];
				}
			}
		}
		return null;
	}

	protected override IConsoleEntity GetUpValidEntity()
	{
		foreach (List<IConsoleEntity> entity in m_Entities)
		{
			if (!entity.Contains(base.CurrentEntity))
			{
				continue;
			}
			int num = entity.IndexOf(base.CurrentEntity);
			int num2 = m_Entities.IndexOf(entity);
			if (num2 == 0 && m_Cyclical.y == 1)
			{
				List<IConsoleEntity> list = m_Entities.Last((List<IConsoleEntity> r) => r.Any((IConsoleEntity e) => e.IsValid()));
				for (int num3 = Mathf.Min(num, list.Count - 1); num3 >= 0; num3--)
				{
					IConsoleEntity consoleEntity = list[num3];
					if (consoleEntity != null && consoleEntity.IsValid())
					{
						return list[num3];
					}
				}
				for (int i = num; i < list.Count; i++)
				{
					IConsoleEntity consoleEntity2 = list[i];
					if (consoleEntity2 != null && consoleEntity2.IsValid())
					{
						return list[i];
					}
				}
				break;
			}
			for (int num4 = num2 - 1; num4 >= 0; num4--)
			{
				List<IConsoleEntity> list2 = m_Entities[num4];
				if (list2.Any((IConsoleEntity e) => e.IsValid()))
				{
					for (int num5 = Mathf.Min(num, list2.Count - 1); num5 >= 0; num5--)
					{
						IConsoleEntity consoleEntity3 = list2[num5];
						if (consoleEntity3 != null && consoleEntity3.IsValid())
						{
							return list2[num5];
						}
					}
					for (int j = num; j < list2.Count; j++)
					{
						IConsoleEntity consoleEntity4 = list2[j];
						if (consoleEntity4 != null && consoleEntity4.IsValid())
						{
							return list2[j];
						}
					}
				}
			}
		}
		return null;
	}

	protected override IConsoleEntity GetDownValidEntity()
	{
		foreach (List<IConsoleEntity> entity in m_Entities)
		{
			if (!entity.Contains(base.CurrentEntity))
			{
				continue;
			}
			int num = entity.IndexOf(base.CurrentEntity);
			int num2 = m_Entities.IndexOf(entity);
			if (num2 == m_Entities.Count - 1)
			{
				if (m_Cyclical.y != 1)
				{
					break;
				}
				List<IConsoleEntity> list = m_Entities.First((List<IConsoleEntity> l) => l.Any((IConsoleEntity e) => e.IsValid()));
				for (int num3 = Mathf.Min(num, list.Count - 1); num3 >= 0; num3--)
				{
					IConsoleEntity consoleEntity = list[num3];
					if (consoleEntity != null && consoleEntity.IsValid())
					{
						return list[num3];
					}
				}
				for (int i = num; i < list.Count; i++)
				{
					IConsoleEntity consoleEntity2 = list[i];
					if (consoleEntity2 != null && consoleEntity2.IsValid())
					{
						return list[i];
					}
				}
				break;
			}
			for (int j = num2 + 1; j < m_Entities.Count; j++)
			{
				List<IConsoleEntity> list2 = m_Entities[j];
				if (!list2.Any((IConsoleEntity e) => e.IsValid()))
				{
					continue;
				}
				for (int num4 = Mathf.Min(num, list2.Count - 1); num4 >= 0; num4--)
				{
					IConsoleEntity consoleEntity3 = list2[num4];
					if (consoleEntity3 != null && consoleEntity3.IsValid())
					{
						return list2[num4];
					}
				}
				for (int k = num; k < list2.Count; k++)
				{
					IConsoleEntity consoleEntity4 = list2[k];
					if (consoleEntity4 != null && consoleEntity4.IsValid())
					{
						return list2[k];
					}
				}
			}
		}
		return null;
	}

	protected override IConsoleEntity GetValidEntityInDirection(Vector2 vector)
	{
		if (!(Math.Abs(vector.x) >= Math.Abs(vector.y)))
		{
			if (!(vector.y >= 0f))
			{
				return GetDownValidEntity();
			}
			return GetUpValidEntity();
		}
		if (!(vector.x >= 0f))
		{
			return GetLeftValidEntity();
		}
		return GetRightValidEntity();
	}

	private void SelectClosestEntity(IConsoleEntity entity)
	{
		List<IConsoleEntity> list = Entities.ToList();
		int num = list.IndexOf(entity);
		if (num < 0)
		{
			FocusOnFirstValidEntity();
			return;
		}
		IConsoleEntity consoleEntity = null;
		if (num + 1 < list.Count)
		{
			consoleEntity = list[num + 1];
		}
		else if (num - 1 >= 0)
		{
			consoleEntity = list[num - 1];
		}
		if (consoleEntity != null)
		{
			FocusOnEntity(consoleEntity);
		}
		else
		{
			FocusOnFirstValidEntity();
		}
	}

	protected override IConsoleEntity GetFirstValidEntity()
	{
		return m_Entities.SelectMany((List<IConsoleEntity> entities) => entities).FirstOrDefault((IConsoleEntity entity) => entity.IsValid());
	}

	protected override IConsoleEntity GetLastValidEntity()
	{
		return m_Entities.SelectMany((List<IConsoleEntity> entities) => entities).LastOrDefault((IConsoleEntity entity) => entity.IsValid());
	}

	public override void Clear()
	{
		ResetCurrentEntity();
		m_Entities.ForEach(delegate(List<IConsoleEntity> e)
		{
			e.Clear();
		});
		m_Entities.Clear();
	}
}
