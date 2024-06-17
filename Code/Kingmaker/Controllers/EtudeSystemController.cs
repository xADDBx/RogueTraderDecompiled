using System.Collections.Generic;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers;

public class EtudeSystemController : IControllerTick, IController, IAreaActivationHandler, ISubscriber
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Game.Instance.Player.EtudesSystem.ConditionsDirty)
		{
			Game.Instance.Player.EtudesSystem.UpdateEtudes();
		}
	}

	public void OnAreaActivated()
	{
		foreach (MechanicEntity mechanicEntity in Game.Instance.State.MechanicEntities)
		{
			UpdateFacts(mechanicEntity);
		}
	}

	private static void UpdateFacts(MechanicEntity entity)
	{
		List<EntityFact> list = null;
		foreach (EntityFact item in entity.Facts.List)
		{
			foreach (EntityFactSource source in item.Sources)
			{
				Etude etude = source?.Etude;
				if (etude != null && !etude.IsPlaying)
				{
					if (list == null)
					{
						list = TempList.Get<EntityFact>();
					}
					list.Add(item);
					break;
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (EntityFact item2 in list)
		{
			entity.Facts.Remove(item2);
		}
	}
}
