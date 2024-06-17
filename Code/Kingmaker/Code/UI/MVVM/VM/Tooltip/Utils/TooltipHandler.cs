using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.DragNDrop;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.UnityExtensions;
using Owlcat.Runtime.UI.ConsoleTools.RewiredCursor;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;

public class TooltipHandler : IDisposable
{
	private TooltipData m_SingleData;

	private TooltipBaseTemplate m_SingleTemplate;

	private IEnumerable<TooltipData> m_TooltipDatas;

	private MonoBehaviour m_Component;

	private TooltipConfig m_Config;

	private bool m_Show;

	private IDisposable m_Enter;

	private IDisposable m_Exit;

	private IDisposable m_Disable;

	private IDisposable m_RightClick;

	private Coroutine m_PostponedSubscription;

	private TooltipData TooltipData => m_SingleData ?? (m_SingleData = new TooltipData(m_SingleTemplate, m_Config));

	private TooltipBaseTemplate RightClickTemplate
	{
		get
		{
			TooltipBaseTemplate tooltipBaseTemplate = m_SingleTemplate;
			if (tooltipBaseTemplate == null)
			{
				IEnumerable<TooltipData> tooltipDatas = m_TooltipDatas;
				if (tooltipDatas == null)
				{
					return null;
				}
				tooltipBaseTemplate = tooltipDatas.Last().MainTemplate;
			}
			return tooltipBaseTemplate;
		}
	}

	public TooltipHandler(MonoBehaviour component, TooltipBaseTemplate template, TooltipConfig config)
		: this(component, config)
	{
		m_SingleTemplate = template;
	}

	public TooltipHandler(MonoBehaviour component, IEnumerable<TooltipData> tooltipDatas, TooltipConfig config)
		: this(component, config)
	{
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		m_TooltipDatas = tooltipDatas;
	}

	private TooltipHandler(MonoBehaviour component, TooltipConfig config)
	{
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = component.transform as RectTransform;
		}
		m_Component = component;
		m_Config = config;
		Subscribe(m_Config);
	}

	public void UpdateTooltip(TooltipBaseTemplate template, TooltipConfig config = default(TooltipConfig))
	{
		if (config.TooltipPlace == null)
		{
			config.TooltipPlace = m_Component.transform as RectTransform;
		}
		m_Config = config;
		m_SingleData = null;
		m_SingleTemplate = template;
		m_TooltipDatas = null;
	}

	private void Subscribe(TooltipConfig config)
	{
		using (ProfileScope.New("selectable bind"))
		{
			OwlcatSelectable owlcatSelectable = ((m_Component != null) ? m_Component.GetComponent<OwlcatSelectable>() : null);
			if (owlcatSelectable != null)
			{
				m_Enter = (from b in owlcatSelectable.OnHoverAsObservable()
					where b
					select b).Subscribe(delegate
				{
					EnterAction();
				});
				m_Exit = (from b in owlcatSelectable.OnHoverAsObservable()
					where !b
					select b).Subscribe(delegate
				{
					ExitAction();
				});
			}
			else
			{
				m_Enter = m_Component.OnPointerEnterAsObservable().Subscribe(delegate
				{
					EnterAction();
				});
				m_Exit = m_Component.OnPointerExitAsObservable().Subscribe(delegate
				{
					ExitAction();
				});
			}
		}
		using (ProfileScope.New("disable bind"))
		{
			m_Disable = ObservableExtensions.Subscribe(m_Component.OnDisableAsObservable(), delegate
			{
				if (m_Show)
				{
					m_Show = false;
					TooltipHelper.HideTooltip();
				}
			});
		}
		using (ProfileScope.New("right click bind"))
		{
			if (config.InfoCallPCMethod == InfoCallPCMethod.None || Game.Instance.IsControllerGamepad)
			{
				return;
			}
			OwlcatButton component = m_Component.GetComponent<OwlcatButton>();
			if (component != null)
			{
				m_RightClick = ObservableExtensions.Subscribe(component.OnRightClickAsObservable(), delegate
				{
					RightClickAction();
				});
				return;
			}
			m_RightClick = m_Component.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
			{
				if (data.button == PointerEventData.InputButton.Right)
				{
					RightClickAction();
				}
			});
		}
	}

	private void EnterAction()
	{
		if ((!Cursor.visible && !UIKitRewiredCursorController.Enabled) || DragNDropManager.IsDraggingSomething)
		{
			return;
		}
		m_Show = true;
		if (m_TooltipDatas == null)
		{
			if (m_SingleTemplate != null)
			{
				EventBus.RaiseEvent(delegate(ITooltipHandler h)
				{
					h.HandleTooltipRequest(TooltipData);
				});
			}
		}
		else
		{
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleComparativeTooltipRequest(m_TooltipDatas);
			});
		}
	}

	private void ExitAction()
	{
		if (Cursor.visible || UIKitRewiredCursorController.Enabled)
		{
			m_Show = false;
			TooltipHelper.HideTooltip();
		}
	}

	private void RightClickAction()
	{
		if (!Cursor.visible && !UIKitRewiredCursorController.Enabled)
		{
			return;
		}
		TooltipBaseTemplate rightClickTemplate = RightClickTemplate;
		if (rightClickTemplate == null || (m_Config.InfoCallPCMethod == InfoCallPCMethod.ShiftRightMouseButton && !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)))
		{
			return;
		}
		m_Show = false;
		TooltipHelper.HideTooltip();
		if (!m_Config.IsGlossary)
		{
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleInfoRequest(rightClickTemplate);
			});
		}
		else
		{
			EventBus.RaiseEvent(delegate(ITooltipHandler h)
			{
				h.HandleGlossaryInfoRequest(rightClickTemplate as TooltipTemplateGlossary);
			});
		}
	}

	public void Dispose()
	{
		if (m_PostponedSubscription != null)
		{
			CoroutineRunner.Stop(m_PostponedSubscription);
			m_PostponedSubscription = null;
		}
		if (m_Show)
		{
			m_Show = false;
			TooltipHelper.HideTooltip();
		}
		m_Component = null;
		m_Enter?.Dispose();
		m_Exit?.Dispose();
		m_Disable?.Dispose();
		m_RightClick?.Dispose();
	}
}
