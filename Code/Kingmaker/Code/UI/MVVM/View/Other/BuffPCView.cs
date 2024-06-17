using Kingmaker.Code.UI.MVVM.VM.Other;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Other;

public class BuffPCView : ViewBase<BuffVM>, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TextMeshProUGUI m_Rank;

	[SerializeField]
	private GameObject m_NonStackNotification;

	[SerializeField]
	private OwlcatSelectable m_Selectable;

	private BoolReactiveProperty IsHovered;

	public void SetHoverProperty(BoolReactiveProperty isHovered)
	{
		IsHovered = isHovered;
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(base.ViewModel.Icon.Subscribe(delegate(Sprite icon)
		{
			m_Icon.sprite = icon;
		}));
		AddDisposable(base.ViewModel.ShowNonStackNotification.Subscribe(delegate(bool value)
		{
			m_NonStackNotification.SetActive(value);
		}));
		AddDisposable(base.ViewModel.Rank.Subscribe(delegate(int value)
		{
			m_Rank.gameObject.SetActive(value > 1);
			m_Rank.text = value.ToString();
		}));
		if (IsHovered != null)
		{
			AddDisposable(m_Selectable.OnPointerEnterAsObservable().Subscribe(delegate
			{
				IsHovered.Value = true;
			}));
			AddDisposable(m_Selectable.OnPointerExitAsObservable().Subscribe(delegate
			{
				IsHovered.Value = false;
			}));
		}
		AddDisposable(this.SetTooltip(new TooltipTemplateBuff(base.ViewModel.Buff), new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.None)));
	}

	protected override void DestroyViewImplementation()
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		UILog.Log("Buff pointer enter");
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UILog.Log("Buff pointer enter");
	}
}
