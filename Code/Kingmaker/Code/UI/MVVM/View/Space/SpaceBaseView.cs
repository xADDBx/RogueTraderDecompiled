using System;
using Kingmaker.Code.UI.MVVM.View.Space.InputLayers;
using Kingmaker.Code.UI.MVVM.VM.Space;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Space;

public abstract class SpaceBaseView : ViewBase<SpaceVM>, IInitializable
{
	protected InputLayer SpaceBaseInputLayer;

	protected SpaceCombatInputLayer SpaceCombatInputLayer;

	protected SpaceMainInputLayer SpaceSystemMapInputLayer;

	protected SpaceMainInputLayer SpaceGlobalMapInputLayer;

	public const string SpaceBaseInputLayerContextName = "SpaceBaseInputLayer";

	public const string SpaceCombatInputLayerContextName = "SpaceCombatInputLayer";

	public const string SpaceSystemMapInputLayerContextName = "SpaceSystemMapInputLayer";

	public const string SpaceGlobalMapInputLayerContextName = "SpaceGlobalMapInputLayer";

	public virtual void Initialize()
	{
	}

	protected override void BindViewImplementation()
	{
		SpaceBaseInputLayer = new InputLayer
		{
			ContextName = "SpaceBaseInputLayer"
		};
		CreateBaseInputImpl(SpaceBaseInputLayer);
		GamePad.Instance.SetBaseLayer(SpaceBaseInputLayer);
		AddDisposable(base.ViewModel.SpaceMode.Subscribe(delegate(SpaceMode mode)
		{
			DeactivateGlobalMapInputLayer();
			DeactivateSystemMapInputLayer();
			DeactivateCombatInputLayer();
			switch (mode)
			{
			case SpaceMode.GlobalMap:
				ActivateGlobalMapInputLayer();
				break;
			case SpaceMode.SystemMap:
				ActivateSystemMapInputLayer();
				break;
			case SpaceMode.SpaceCombat:
				ActivateCombatInputLayer();
				break;
			default:
				throw new ArgumentOutOfRangeException("mode", mode, null);
			case SpaceMode.None:
				break;
			}
		}));
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.SetBaseLayer(null);
		SpaceBaseInputLayer = null;
		DeactivateCombatInputLayer();
		DeactivateSystemMapInputLayer();
		DeactivateGlobalMapInputLayer();
	}

	private void ActivateCombatInputLayer()
	{
		SpaceCombatInputLayer = new SpaceCombatInputLayer
		{
			ContextName = "SpaceCombatInputLayer"
		};
		CreateCombatInputImpl(SpaceCombatInputLayer);
		GamePad.Instance.PushLayer(SpaceCombatInputLayer);
	}

	private void DeactivateCombatInputLayer()
	{
		if (SpaceCombatInputLayer != null)
		{
			GamePad.Instance.PopLayer(SpaceCombatInputLayer);
			SpaceCombatInputLayer.Dispose();
			SpaceCombatInputLayer = null;
		}
	}

	private void ActivateSystemMapInputLayer()
	{
		SpaceSystemMapInputLayer = new SpaceMainInputLayer
		{
			ContextName = "SpaceSystemMapInputLayer"
		};
		SpaceSystemMapInputLayer.CursorEnabled = false;
		CreateSystemMapInputImpl(SpaceSystemMapInputLayer);
		GamePad.Instance.PushLayer(SpaceSystemMapInputLayer);
	}

	private void DeactivateSystemMapInputLayer()
	{
		if (SpaceSystemMapInputLayer != null)
		{
			GamePad.Instance.PopLayer(SpaceSystemMapInputLayer);
			SpaceSystemMapInputLayer.Dispose();
			SpaceSystemMapInputLayer = null;
		}
	}

	private void ActivateGlobalMapInputLayer()
	{
		SpaceGlobalMapInputLayer = new SpaceMainInputLayer
		{
			ContextName = "SpaceGlobalMapInputLayer"
		};
		CreateGlobalMapInputImpl(SpaceGlobalMapInputLayer);
		GamePad.Instance.PushLayer(SpaceGlobalMapInputLayer);
	}

	private void DeactivateGlobalMapInputLayer()
	{
		if (SpaceGlobalMapInputLayer != null)
		{
			GamePad.Instance.PopLayer(SpaceGlobalMapInputLayer);
			SpaceGlobalMapInputLayer.Dispose();
			SpaceGlobalMapInputLayer = null;
		}
	}

	protected virtual void CreateBaseInputImpl(InputLayer baseInputLayer)
	{
	}

	protected virtual void CreateCombatInputImpl(InputLayer combatInputLayer)
	{
	}

	protected virtual void CreateSystemMapInputImpl(InputLayer systemMapInputLayer)
	{
	}

	protected virtual void CreateGlobalMapInputImpl(InputLayer globalMapInputLayer)
	{
	}
}
