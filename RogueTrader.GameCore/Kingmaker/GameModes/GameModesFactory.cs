using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.GameModes;

public static class GameModesFactory
{
	public static readonly List<ControllerData> AllControllers = new List<ControllerData>();

	public static void Register(IController controller, params GameModeType[] gameModes)
	{
		ControllerData controllerData = AllControllers.FirstItem((ControllerData i) => i.Controller == controller);
		if (controllerData != null)
		{
			IEnumerable<GameModeType> enumerable = controllerData.GameModes.Intersect(gameModes);
			if (enumerable.Any())
			{
				string name = controller.GetType().Name;
				string text = string.Join(", ", enumerable);
				PFLog.Default.Error("Can't register controller twice for same game mode: " + name + ", same game modes: " + text);
			}
			controllerData.SetGameModes(controllerData.GameModes.Union(gameModes));
		}
		else
		{
			AllControllers.Add(new ControllerData(controller, gameModes.ToArray()));
		}
	}

	public static GameMode Create(GameModeType type)
	{
		List<IController> list = TempList.Get<IController>();
		foreach (ControllerData allController in AllControllers)
		{
			if (allController.GameModes.HasItem(type) && allController.Controller != null)
			{
				list.Add(allController.Controller);
			}
		}
		return new GameMode(type, list);
	}

	public static async Task Reset()
	{
		foreach (ControllerData allController in AllControllers)
		{
			IController controller = allController.Controller;
			if (!(controller is IAsyncDisposable asyncDisposable))
			{
				if (controller is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			else
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		AllControllers.Clear();
	}
}
