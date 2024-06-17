using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Models.Log.Enums;
using Kingmaker.UI.MVVM.VM.Tooltip.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;

public class TooltipBrickNestedMessageView : TooltipBaseBrickView<TooltipBrickNestedMessageVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	protected Image m_Line;

	[SerializeField]
	private CanvasGroup m_PrefixCanvasGroup;

	[SerializeField]
	private Image m_IconImage;

	[SerializeField]
	private TextMeshProUGUI m_NumberText;

	[SerializeField]
	private RectTransform m_TooltipPlaceRectTransform;

	[SerializeField]
	protected CanvasGroup m_HighlightCanvasGroup;

	[SerializeField]
	private float m_DefaultFontSize = 17f;

	protected TooltipConfig m_TooltipConfig;

	public RectTransform TooltipPlace => m_TooltipPlaceRectTransform;

	protected override void BindViewImplementation()
	{
		m_Text.text = base.ViewModel.Text;
		SetTextFontSize();
		SetTextColor(base.ViewModel.TextColor);
		SetIcon();
		if (m_Line != null)
		{
			m_Line.gameObject.SetActive(base.ViewModel.NeedShowLine);
		}
		m_HighlightCanvasGroup.alpha = 0f;
		m_TooltipConfig.TooltipPlace = TooltipPlace;
		if (Game.Instance.IsControllerMouse)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.TooltipTemplate, m_TooltipConfig));
			AddDisposable(this.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
			{
				OnClick(data);
			}));
			AddDisposable(this.OnPointerEnterAsObservable().Subscribe(delegate
			{
				m_HighlightCanvasGroup.alpha = 1f;
			}));
			AddDisposable(this.OnPointerExitAsObservable().Subscribe(delegate
			{
				m_HighlightCanvasGroup.alpha = 0f;
			}));
		}
	}

	private void SetIcon()
	{
		Sprite icon = GameLogUtility.GetIcon((base.ViewModel.ShotNumber > 0) ? PrefixIcon.Empty : base.ViewModel.PrefixIcon);
		if (icon != null)
		{
			m_IconImage.sprite = icon;
		}
		TextMeshProUGUI numberText = m_NumberText;
		int shotNumber = base.ViewModel.ShotNumber;
		numberText.text = shotNumber.ToString();
		m_NumberText.alpha = ((base.ViewModel.ShotNumber > 0) ? 1f : 0f);
		CanvasGroup prefixCanvasGroup = m_PrefixCanvasGroup;
		PrefixIcon prefixIcon = base.ViewModel.PrefixIcon;
		prefixCanvasGroup.alpha = ((prefixIcon == PrefixIcon.None || prefixIcon == PrefixIcon.Invisible) ? 0f : 1f);
	}

	private void SetTextColor(Color color)
	{
		m_Text.color = ((color.a > 0f) ? color : ((Color)GameLogStrings.Instance.DefaultColor));
	}

	private void SetTextFontSize()
	{
		m_Text.fontSize = m_DefaultFontSize * base.ViewModel.FontSizeMultiplier;
	}

	public void UpdateTextSize(float multiplier)
	{
		m_Text.fontSize = m_DefaultFontSize * multiplier;
	}

	private void OnClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			OnConfirm();
		}
	}

	protected void OnConfirm()
	{
		Game.Instance.CameraController?.Follower?.ScrollTo(base.ViewModel.Unit);
	}
}
