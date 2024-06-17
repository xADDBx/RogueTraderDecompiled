using Kingmaker.Code.UI.MVVM.View.Surface.InputLayers;
using Kingmaker.Code.UI.MVVM.VM.Surface;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Surface;

public abstract class SurfaceBaseView : ViewBase<SurfaceVM>, IInitializable
{
	protected InputLayer SurfaceBaseInputLayer;

	protected SurfaceMainInputLayer SurfaceMainInputLayer;

	protected SurfaceCombatInputLayer SurfaceCombatInputLayer;

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		SurfaceBaseInputLayer = new InputLayer
		{
			ContextName = "SurfaceBaseInputLayer"
		};
		CreateBaseInputImpl(SurfaceBaseInputLayer);
		GamePad.Instance.SetBaseLayer(SurfaceBaseInputLayer);
		SurfaceMainInputLayer = new SurfaceMainInputLayer
		{
			ContextName = "SurfaceMainInputLayer"
		};
		CreateMainInputImpl(SurfaceMainInputLayer);
		GamePad.Instance.PushLayer(SurfaceMainInputLayer);
		AddDisposable(base.ViewModel.IsCombatInputModeActive.Subscribe(delegate(bool isActive)
		{
			if (isActive)
			{
				ActivateCombatInputLayer();
			}
			else
			{
				DeactivateCombatInputLayer();
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.SetBaseLayer(null);
		GamePad.Instance.PopLayer(SurfaceMainInputLayer);
		SurfaceBaseInputLayer = null;
		SurfaceMainInputLayer.Dispose();
		SurfaceMainInputLayer = null;
		DeactivateCombatInputLayer();
	}

	private void ActivateCombatInputLayer()
	{
		SurfaceCombatInputLayer = new SurfaceCombatInputLayer
		{
			ContextName = "SurfaceCombatInputLayer"
		};
		CreateCombatInputImpl(SurfaceCombatInputLayer);
		GamePad.Instance.PushLayer(SurfaceCombatInputLayer);
	}

	private void DeactivateCombatInputLayer()
	{
		if (SurfaceCombatInputLayer != null)
		{
			GamePad.Instance.PopLayer(SurfaceCombatInputLayer);
			SurfaceCombatInputLayer.Dispose();
			SurfaceCombatInputLayer = null;
		}
	}

	protected virtual void CreateBaseInputImpl(InputLayer baseInputLayer)
	{
	}

	protected virtual void CreateMainInputImpl(InputLayer mainInputLayer)
	{
	}

	protected virtual void CreateCombatInputImpl(InputLayer combatInputLayer)
	{
	}
}
