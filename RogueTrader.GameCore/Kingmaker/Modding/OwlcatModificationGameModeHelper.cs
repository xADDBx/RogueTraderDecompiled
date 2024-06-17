using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Modding;

public static class OwlcatModificationGameModeHelper
{
	public class ControllerInserter
	{
		private int m_Index;

		public ControllerInserter(int index)
		{
			m_Index = index;
		}

		public ControllerInserter Insert(OwlcatModificationController controller, params GameModeType[] gameModes)
		{
			if (gameModes.Empty())
			{
				PFLog.Mods.Error("No game modes specified");
				return this;
			}
			GameModesFactory.AllControllers.Insert(m_Index++, new ControllerData(controller, gameModes));
			return this;
		}
	}

	private static void EnsureControllersInitialized()
	{
		if (GameModesFactory.AllControllers.Empty())
		{
			throw new NullReferenceException("GameModesFactory.AllControllers.Empty");
		}
	}

	public static void OverrideController<TOldController>(OwlcatModificationController newController, bool invokeBaseController) where TOldController : IController
	{
		EnsureControllersInitialized();
		ControllerData controllerData = GameModesFactory.AllControllers.FirstOrDefault((ControllerData i) => IsControllerOfType<TOldController>(i.Controller));
		if (controllerData == null)
		{
			PFLog.Mods.Error("Can't find controller of type TOldController");
			return;
		}
		if (invokeBaseController)
		{
			newController.DefaultController = controllerData.Controller;
		}
		controllerData.SetController(newController);
	}

	public static ControllerInserter GetControllerInserterBefore<TController>() where TController : IController
	{
		EnsureControllersInitialized();
		int num = GameModesFactory.AllControllers.FindIndex((ControllerData i) => IsControllerOfType<TController>(i.Controller));
		if (num < 0)
		{
			PFLog.Mods.Error("Can't find controller of type TController");
			return null;
		}
		return new ControllerInserter(num);
	}

	public static ControllerInserter GetControllerInserterAfter<TController>() where TController : IController
	{
		EnsureControllersInitialized();
		int num = GameModesFactory.AllControllers.FindIndex((ControllerData i) => IsControllerOfType<TController>(i.Controller));
		if (num < 0)
		{
			PFLog.Mods.Error("Can't find controller of type TController");
			return null;
		}
		return new ControllerInserter(num + 1);
	}

	public static void AddControllerToGameModes<TController>(params GameModeType[] gameModes) where TController : IController
	{
		if (gameModes.Empty())
		{
			PFLog.Mods.Error("No game modes specified");
			return;
		}
		EnsureControllersInitialized();
		ControllerData controllerData = GameModesFactory.AllControllers.FirstOrDefault((ControllerData i) => IsControllerOfType<TController>(i.Controller));
		if (controllerData == null)
		{
			PFLog.Mods.Error("Can't find controller of type TController");
		}
		else
		{
			controllerData.SetGameModes(controllerData.GameModes.Concat(gameModes).Distinct());
		}
	}

	private static bool IsControllerOfType<T>([NotNull] IController controller) where T : IController
	{
		while (true)
		{
			if (controller is T)
			{
				return true;
			}
			if (!(controller is OwlcatModificationController owlcatModificationController))
			{
				break;
			}
			controller = owlcatModificationController.DefaultController;
		}
		return false;
	}
}
