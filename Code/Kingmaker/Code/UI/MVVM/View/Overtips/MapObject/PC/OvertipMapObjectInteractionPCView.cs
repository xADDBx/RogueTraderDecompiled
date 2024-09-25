using System.Text;
using Kingmaker.Blueprints.Root.Strings;
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

public class OvertipMapObjectInteractionPCView : OvertipMapObjectInteractionView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IGameModeHandler, ISubscriber
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
		m_HintText.text = GetHint();
		AddDisposable(this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			ShowHint();
		}));
		AddDisposable(this.OnPointerExitAsObservable().Subscribe(delegate
		{
			HideHint();
		}));
	}

	private void ShowHint()
	{
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			m_HintAnimator.AppearAnimation();
		}
	}

	private void HideHint()
	{
		int? requiredResourceCount = base.ViewModel.RequiredResourceCount;
		if (requiredResourceCount.HasValue && requiredResourceCount.GetValueOrDefault() > 0)
		{
			m_HintAnimator.DisappearAnimation();
		}
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

	private string GetHint()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(base.ViewModel.ResourceName + "\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.HasResourceCount.Text}: {base.ViewModel.HasResourceCount}\n");
		stringBuilder.Append($"{UIStrings.Instance.Overtips.RequiredResourceCount.Text}: {base.ViewModel.RequiredResourceCount}\n");
		return stringBuilder.ToString();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		HideHint();
	}
}
