using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.PC;

public class OvertipMapObjectInteractionPCView : OvertipMapObjectInteractionView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IGameModeHandler, ISubscriber
{
	protected override void BindViewImplementation()
	{
		OnGameModeStart(Game.Instance.CurrentMode);
		base.BindViewImplementation();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (base.IsBinded)
		{
			base.ViewModel.MapObjectEntity.View.Highlighted = true;
			base.ViewModel.IsMouseOverUI.Value = true;
			SetHighlightImage(active: true);
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View, isHighlighted: true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (base.IsBinded)
		{
			base.ViewModel.MapObjectEntity.View.Highlighted = false;
			base.ViewModel.IsMouseOverUI.Value = false;
			SetHighlightImage(active: false);
			Game.Instance.CursorController.SetMapObjectCursor(base.ViewModel.MapObjectEntity.View, isHighlighted: false);
		}
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_Button.Interactable = gameMode == GameModeType.Default || gameMode == GameModeType.StarSystem || gameMode == GameModeType.GlobalMap || gameMode == GameModeType.Pause;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}
}
