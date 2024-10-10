using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Overtips.MapObject.PC;

public class OvertipMapObjectTwitchDropsPCView : OvertipMapObjectTwitchDropsView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IGameModeHandler, ISubscriber
{
	[Header("Hint")]
	[SerializeField]
	private FadeAnimator m_HintAnimator;

	[SerializeField]
	private TextMeshProUGUI m_HintText;

	protected override void BindViewImplementation()
	{
		OnGameModeStart(Game.Instance.CurrentMode);
		base.BindViewImplementation();
		AddDisposable(this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			m_HintAnimator.AppearAnimation();
		}));
		AddDisposable(this.OnPointerExitAsObservable().Subscribe(delegate
		{
			m_HintAnimator.DisappearAnimation();
		}));
		AddDisposable(base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_HintText.text = value;
		}));
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

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_HintAnimator.DisappearAnimation();
	}
}
