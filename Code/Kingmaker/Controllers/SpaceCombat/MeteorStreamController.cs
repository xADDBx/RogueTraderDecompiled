using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.MeteorStream;

namespace Kingmaker.Controllers.SpaceCombat;

public class MeteorStreamController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public Dictionary<CustomGridNodeBase, List<MeteorEntity>> GetDangerousNodes()
	{
		IEnumerable<MechanicEntity> enumerable = Game.Instance.TurnController.AllUnits.Where((MechanicEntity x) => x is MeteorStreamEntity);
		Dictionary<CustomGridNodeBase, List<MeteorEntity>> dictionary = new Dictionary<CustomGridNodeBase, List<MeteorEntity>>();
		foreach (MechanicEntity item2 in enumerable)
		{
			Dictionary<CustomGridNodeBase, MeteorEntity> dictionary2 = (item2 as MeteorStreamEntity)?.GetMeteorsDangerZones();
			if (dictionary2 == null)
			{
				continue;
			}
			foreach (var (key, item) in dictionary2)
			{
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = new List<MeteorEntity>();
					dictionary.Add(key, value);
				}
				value.Add(item);
			}
		}
		return dictionary;
	}

	public void Tick()
	{
		if (Game.Instance.TurnController.CurrentUnit is MeteorStreamEntity)
		{
			(Game.Instance.TurnController.CurrentUnit as MeteorStreamEntity)?.HandleTick(Game.Instance.TimeController.DeltaTime);
		}
	}
}
