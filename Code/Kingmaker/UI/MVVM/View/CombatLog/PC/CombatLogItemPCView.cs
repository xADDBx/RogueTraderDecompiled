using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.UI.MVVM.View.CombatLog.PC;

public class CombatLogItemPCView : CombatLogItemBaseView, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	[SerializeField]
	private CanvasGroup m_HighlightCanvasGroup;

	[SerializeField]
	private float m_DefaultFontSize = 17f;

	private TooltipConfig m_TooltipConfig;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_TooltipConfig.PriorityPivots = new List<Vector2>
		{
			new Vector2(1f, 0.5f)
		};
		AddDisposable(this.SetTooltip(base.ViewModel.TooltipTemplate, m_TooltipConfig));
		m_HighlightCanvasGroup.alpha = 0f;
		SetTextFontSize();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		m_HighlightCanvasGroup.alpha = 1f;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		m_HighlightCanvasGroup.alpha = 0f;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			Game.Instance.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
		}
	}

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultFontSize * base.ViewModel.FontSizeMultiplier;
	}

	public override void UpdateTextSize(float multiplier)
	{
		m_Text.fontSize = m_DefaultFontSize * multiplier;
		base.UpdateTextSize(multiplier);
	}
}
