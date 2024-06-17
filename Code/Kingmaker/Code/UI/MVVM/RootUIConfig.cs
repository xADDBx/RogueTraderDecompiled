using System.Collections;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Common;
using Kingmaker.Code.UI.MVVM.VM.LoadingScreen;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Kingmaker.Utility.BuildModeUtils;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RootUIConfig : MonoBehaviour
{
	public MonoBehaviour View;

	public MonoBehaviour TryCreateView<TViewModel>(TViewModel viewModel) where TViewModel : class, IViewModel
	{
		if (!(viewModel is CommonVM viewModel2))
		{
			if (!(viewModel is SurfaceVM viewModel3))
			{
				if (!(viewModel is MainMenuVM viewModel4))
				{
					if (!(viewModel is SpaceVM viewModel5))
					{
						if (viewModel is LoadingScreenRootVM viewModel6)
						{
							return CreateAndBindView(viewModel6);
						}
						return null;
					}
					return CreateAndBindView(viewModel5);
				}
				return CreateAndBindView(viewModel4);
			}
			return CreateAndBindView(viewModel3);
		}
		return CreateAndBindView(viewModel2);
	}

	private MonoBehaviour CreateAndBindView<TViewModel>(TViewModel viewModel) where TViewModel : class, IViewModel
	{
		ViewBase<TViewModel> component = UIConfig.Instance.ViewConfigs.LoadPrefab(viewModel, Game.Instance.IsControllerMouse).GetComponent<ViewBase<TViewModel>>();
		if (component == null)
		{
			return null;
		}
		ViewBase<TViewModel> viewBase = Object.Instantiate(component, base.transform);
		viewBase.transform.parent = null;
		((IInitializable)viewBase).Initialize();
		viewBase.Bind(viewModel);
		return viewBase;
	}

	public IEnumerator TryCreateViewCoroutine<TViewModel>(TViewModel viewModel, MonoBehaviour view) where TViewModel : class, IViewModel
	{
		if (!(viewModel is CommonVM viewModel2))
		{
			if (!(viewModel is SurfaceVM viewModel3))
			{
				if (!(viewModel is MainMenuVM viewModel4))
				{
					if (viewModel is SpaceVM viewModel5)
					{
						return CreateAndBindViewCoroutine(viewModel5, view);
					}
					return null;
				}
				return CreateAndBindViewCoroutine(viewModel4, view);
			}
			return CreateAndBindViewCoroutine(viewModel3, view);
		}
		return CreateAndBindViewCoroutine(viewModel2, view);
	}

	private IEnumerator CreateAndBindViewCoroutine<TViewModel>(TViewModel viewModel, MonoBehaviour view) where TViewModel : class, IViewModel
	{
		Task<ViewBase<TViewModel>> loadPrefab = LoadPrefabAsync(viewModel);
		while (!loadPrefab.IsCompleted)
		{
			yield return null;
		}
		ViewBase<TViewModel> prefab = loadPrefab.Result;
		if (!(prefab == null))
		{
			yield return null;
			view = Object.Instantiate(prefab, base.transform);
			yield return null;
			view.transform.parent = null;
			((IInitializable)view).Initialize();
			yield return null;
			((ViewBase<TViewModel>)view).Bind(viewModel);
			View = view;
		}
	}

	private static async Task<ViewBase<TViewModel>> LoadPrefabAsync<TViewModel>(TViewModel viewModel) where TViewModel : class, IViewModel
	{
		return (await UIConfig.Instance.ViewConfigs.LoadPrefabAsync(viewModel, Game.Instance.IsControllerMouse)).GetComponent<ViewBase<TViewModel>>();
	}

	public void Unload()
	{
		if ((BuildModeUtility.Data?.Loading?.DestroyUIPrefabs).GetValueOrDefault())
		{
			UIConfig.Instance.ViewConfigs.UnloadAll();
		}
	}

	public void Destroy()
	{
		if ((BuildModeUtility.Data?.Loading?.DestroyUIPrefabs).GetValueOrDefault())
		{
			UIConfig.Instance.ViewConfigs.DestroyAll();
		}
	}
}
