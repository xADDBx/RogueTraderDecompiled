using System.Collections.Generic;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Controllers;

public class GarbageEntityController : IControllerDisable, IController, IControllerTick
{
	public TickType GetTickType()
	{
		return TickType.Any;
	}

	public void Tick()
	{
		List<ItemEntity> list = TempList.Get<ItemEntity>();
		foreach (ItemEntity item in EntityService.Instance.GetListUnsafe<ItemEntity>())
		{
			if (IsGarbageItem(item))
			{
				list.Add(item);
			}
		}
		foreach (ItemEntity item2 in list)
		{
			LogChannel.System.Log($"Dispose garbage entity {item2}");
			item2.Dispose();
		}
	}

	private static bool IsGarbageItem(ItemEntity item)
	{
		if (item.Collection == null && item.Owner == null && item.Wielder == null)
		{
			return !item.IsPartOfAnotherItem;
		}
		return false;
	}

	public void OnDisable()
	{
		Tick();
	}
}
