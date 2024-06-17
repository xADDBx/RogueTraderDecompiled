using System;
using System.Threading.Tasks;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[TypeId("065ee7d03fdf6924f802585067b91373")]
public class UIViewConfigs : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<UIViewConfigs>
	{
	}

	[Serializable]
	public class ViewPrefabPair
	{
		[RequireSeparateBundle]
		public PrefabLink PCView;

		[RequireSeparateBundle]
		public PrefabLink ConsoleView;

		private GameObject m_LastObject;

		public GameObject Load(bool isPCInterface, bool hold = false)
		{
			PrefabLink prefabLink = (isPCInterface ? PCView : ConsoleView);
			prefabLink.ForceUnload();
			m_LastObject = prefabLink.Load(ignorePreloadWarning: false, hold);
			return m_LastObject;
		}

		public async Task<GameObject> LoadAsync(bool isPCInterface)
		{
			PrefabLink obj = (isPCInterface ? PCView : ConsoleView);
			obj.ForceUnload();
			m_LastObject = await obj.LoadAsync(ignorePreloadWarning: false, hold: true);
			return m_LastObject;
		}

		public void Unload()
		{
			PCView.ForceUnload();
			ConsoleView.ForceUnload();
		}

		public void Destroy()
		{
			PCView.DestroyButDontUnload();
			ConsoleView.DestroyButDontUnload();
		}
	}

	public ViewPrefabPair Common;

	public ViewPrefabPair MainMenu;

	public ViewPrefabPair Surface;

	public ViewPrefabPair Space;

	public ViewPrefabPair LoadingScreen;

	public GameObject LoadPrefab(IViewModel vm, bool isPCInterface)
	{
		if (!(vm is CommonVM))
		{
			if (!(vm is SurfaceVM))
			{
				if (!(vm is MainMenuVM))
				{
					if (!(vm is SpaceVM))
					{
						if (vm is LoadingScreenRootVM)
						{
							return LoadingScreen.Load(isPCInterface, hold: true);
						}
						return null;
					}
					return Space.Load(isPCInterface);
				}
				return MainMenu.Load(isPCInterface);
			}
			return Surface.Load(isPCInterface);
		}
		return Common.Load(isPCInterface);
	}

	public async Task<GameObject> LoadPrefabAsync(IViewModel vm, bool isPCInterface)
	{
		if (!(vm is CommonVM))
		{
			if (!(vm is SurfaceVM))
			{
				if (!(vm is MainMenuVM))
				{
					if (!(vm is SpaceVM))
					{
						if (vm is LoadingScreenRootVM)
						{
							return await LoadingScreen.LoadAsync(isPCInterface);
						}
						return null;
					}
					return await Space.LoadAsync(isPCInterface);
				}
				return await MainMenu.LoadAsync(isPCInterface);
			}
			return await Surface.LoadAsync(isPCInterface);
		}
		return await Common.LoadAsync(isPCInterface);
	}

	public void UnloadAll()
	{
		Common.Unload();
		MainMenu.Unload();
		Surface.Unload();
		Space.Unload();
		LoadingScreen.Unload();
	}

	public void DestroyAll()
	{
		Common.Destroy();
		MainMenu.Destroy();
		Surface.Destroy();
		Space.Destroy();
		LoadingScreen.Destroy();
	}
}
