using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.Units;

public class UnitGroupsController : IControllerEnable, IController, IControllerTick, IControllerStart, IControllerStop, IUnitChangeAttackFactionsHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IUnitFactionHandler, IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>
{
	public readonly List<UnitGroup> Groups = new List<UnitGroup>();

	public readonly List<UnitGroup> AwakeGroups = new List<UnitGroup>();

	private readonly HashSet<UnitGroup> m_GroupsForUpdateAttackFactions = new HashSet<UnitGroup>();

	public UnitGroup Party => GetGroup("<directly-controllable-unit>");

	public UnitGroup GetGroup(string id)
	{
		if (id == null)
		{
			return new UnitGroup(null);
		}
		UnitGroup unitGroup = Groups.FirstOrDefault((UnitGroup g) => g.Id.Equals(id));
		if (unitGroup == null)
		{
			unitGroup = new UnitGroup(id);
			Groups.Add(unitGroup);
			Groups.Sort();
		}
		return unitGroup;
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.System;
	}

	public void Tick()
	{
		bool flag = false;
		foreach (UnitGroup group in Groups)
		{
			if (group.Empty())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Groups.RemoveAll(delegate(UnitGroup g)
			{
				bool num = g.Empty();
				if (num)
				{
					g.Dispose();
				}
				return num;
			});
		}
		foreach (UnitGroup groupsForUpdateAttackFaction in m_GroupsForUpdateAttackFactions)
		{
			groupsForUpdateAttackFaction.UpdateAttackFactionsCache();
		}
		m_GroupsForUpdateAttackFactions.Clear();
	}

	public void OnEnable()
	{
		foreach (UnitGroup group in Groups)
		{
			group.UpdateAttackFactionsCache();
		}
	}

	public void OnStart()
	{
		Tick();
	}

	public void OnStop()
	{
		Tick();
	}

	public void Clear()
	{
		foreach (UnitGroup group in Groups)
		{
			group.Dispose();
		}
		Groups.Clear();
		AwakeGroups.Clear();
		m_GroupsForUpdateAttackFactions.Clear();
	}

	public void HandleUnitChangeAttackFactions(MechanicEntity unit)
	{
		m_GroupsForUpdateAttackFactions.Add(EventInvokerExtensions.BaseUnitEntity.CombatGroup.Group);
	}

	public void HandleUnitSpawned()
	{
		EventInvokerExtensions.BaseUnitEntity?.CombatGroup.UpdateAttackFactionsCache();
	}

	public void HandleUnitDestroyed()
	{
		EventInvokerExtensions.BaseUnitEntity?.CombatGroup.UpdateAttackFactionsCache();
	}

	public void HandleUnitDeath()
	{
	}

	public void HandleFactionChanged()
	{
		m_GroupsForUpdateAttackFactions.Add(EventInvokerExtensions.BaseUnitEntity.CombatGroup.Group);
	}

	public void RestoreGroup(UnitGroup group)
	{
		UnitGroup unitGroup = Groups.FirstItem((UnitGroup i) => i.Id == group.Id);
		if (unitGroup != null)
		{
			IAbstractUnitEntity[] array = unitGroup.Units.Select((UnitReference i) => i.Entity).NotNull().ToArray();
			Groups.Remove(unitGroup);
			unitGroup.Dispose();
			IAbstractUnitEntity[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				BaseUnitEntity baseUnitEntity = (BaseUnitEntity)array2[j];
				group.Add(baseUnitEntity);
				baseUnitEntity.CombatGroup.EnsureAndUpdateGroup();
			}
		}
		Groups.Add(group);
		Groups.Sort();
	}
}
