using System;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.VirtualListSystem.ElementSettings;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Owlcat.Runtime.UI.VirtualListSystem;

public class VirtualListElement : IConsoleEntityProxy, IConsoleEntity, IConsoleNavigationEntity, IConfirmClickHandler, ILongConfirmClickHandler, IDeclineClickHandler, ILongDeclineClickHandler, IFunc01ClickHandler, ILongFunc01ClickHandler, IFunc02ClickHandler, ILongFunc02ClickHandler
{
	public ReactiveCommand AttachViewCommand = new ReactiveCommand();

	private IVirtualListElementData m_Data;

	private IVirtualListElementView m_View;

	private VirtualListLayoutElementSettings m_LayoutSettings;

	private IDisposable m_LayoutSettingsSubscription;

	private bool m_IsActive = true;

	private bool m_WasUpdatedAtLeastOnes;

	private bool m_NeedRecalculateSize = true;

	private IDisposable m_SubscriptionOnActive;

	private IDisposable m_SubscriptionOnData;

	public Vector2 AnchorMin { get; set; }

	public Vector2 AnchorMax { get; set; }

	public Vector2 OffsetMin { get; set; }

	public Vector2 OffsetMax { get; set; }

	public VirtualListLayoutPadding Padding { get; set; }

	public IVirtualListElementData Data => m_Data;

	public IVirtualListElementView View => m_View;

	public VirtualListLayoutElementSettings LayoutSettings => m_LayoutSettings;

	public Vector2 Size => OffsetMax - OffsetMin;

	public IConsoleEntity ConsoleEntityProxy => m_View as IConsoleEntity;

	public bool IsActive
	{
		get
		{
			return m_IsActive;
		}
		private set
		{
			if (m_IsActive != value)
			{
				m_IsActive = value;
				OnActiveUpdated();
			}
		}
	}

	public bool WasUpdatedAtLeastOnes => m_WasUpdatedAtLeastOnes;

	private event Action<VirtualListElement> LayoutUpdated;

	private event Action<VirtualListElement> ActiveUpdated;

	public VirtualListElement(IVirtualListElementData data)
	{
		m_Data = data;
	}

	public void MarkUpdated()
	{
		if (m_LayoutSettings != null)
		{
			m_LayoutSettings.MarkUpdated();
		}
		m_WasUpdatedAtLeastOnes = true;
	}

	public bool HasView()
	{
		return m_View != null;
	}

	public void UpdateViewRectTransform()
	{
		if (HasView())
		{
			m_View.RectTransform.anchorMin = AnchorMin;
			m_View.RectTransform.anchorMax = AnchorMax;
			m_View.RectTransform.offsetMin = OffsetMin + new Vector2(Padding.Left, Padding.Bottom);
			m_View.RectTransform.offsetMax = OffsetMax - new Vector2(Padding.Right, Padding.Top);
		}
	}

	public bool HasLayoutSettings()
	{
		if (m_LayoutSettings != null)
		{
			return m_LayoutSettings.OverrideType != VirtualListLayoutElementSettings.LayoutOverrideType.None;
		}
		return false;
	}

	public bool IsControlledByUnityLayout()
	{
		if (m_LayoutSettings != null)
		{
			return m_LayoutSettings.OverrideType == VirtualListLayoutElementSettings.LayoutOverrideType.UnityLayout;
		}
		return false;
	}

	public void RebuildUnitLayout()
	{
		if (m_View != null && m_NeedRecalculateSize && m_View.NeedRebuildToGetSize)
		{
			RectTransform rectTransform = m_View.RectTransform;
			LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			Vector2 size = rectTransform.rect.size;
			if (m_LayoutSettings.OverrideWidth)
			{
				m_LayoutSettings.Width = size.x;
			}
			if (m_LayoutSettings.OverrideHeight)
			{
				m_LayoutSettings.Height = size.y;
			}
			m_NeedRecalculateSize = false;
		}
	}

	public void AttachView(IVirtualListElementView view)
	{
		m_View = view;
		m_View.BindVirtualList(m_Data);
		if (m_LayoutSettings != null)
		{
			m_View.LayoutSettings.CopyFrom(m_LayoutSettings);
			VirtualListLayoutElementSettingsPool.ReturnSettings(m_LayoutSettings);
		}
		m_LayoutSettings = m_View.LayoutSettings;
		m_LayoutSettingsSubscription = m_LayoutSettings.IsDirty.Where((bool _) => _).Subscribe(delegate
		{
			OnLayoutUpdated();
		});
		AttachViewCommand.Execute();
	}

	public IVirtualListElementView DetachView()
	{
		if (m_LayoutSettings != null)
		{
			m_LayoutSettings = VirtualListLayoutElementSettingsPool.GetCopy(m_LayoutSettings);
		}
		m_LayoutSettingsSubscription?.Dispose();
		m_LayoutSettingsSubscription = null;
		IVirtualListElementView view = m_View;
		m_View = null;
		view.UnbindVirtualList();
		return view;
	}

	public void SubscribeOnLayoutUpdate(Action<VirtualListElement> action)
	{
		LayoutUpdated += action;
	}

	public void UnsubscribeFromLayoutUpdate(Action<VirtualListElement> action)
	{
		LayoutUpdated -= action;
	}

	private void OnLayoutUpdated()
	{
		this.LayoutUpdated?.Invoke(this);
	}

	public void SubscribeOnData()
	{
		m_SubscriptionOnData = m_Data.ContentChanged.Subscribe(delegate
		{
			m_NeedRecalculateSize = true;
		});
	}

	public void UnsubscribeFromData()
	{
		m_SubscriptionOnData?.Dispose();
	}

	public void SubscribeOnActiveUpdate(Action<VirtualListElement> action)
	{
		m_SubscriptionOnActive = m_Data.ActiveInVirtualList.Subscribe(delegate(bool active)
		{
			IsActive = active;
		});
		ActiveUpdated += action;
	}

	public void UnsubscribeFromActiveUpdate(Action<VirtualListElement> action)
	{
		m_SubscriptionOnActive?.Dispose();
		ActiveUpdated -= action;
	}

	private void OnActiveUpdated()
	{
		this.ActiveUpdated?.Invoke(this);
	}

	public virtual void SetFocus(bool value)
	{
		ConsoleEntityProxy?.SetFocused(value);
	}

	public bool IsValid()
	{
		return ConsoleEntityProxy?.IsValid() ?? m_Data.IsAvailable.Value;
	}

	public bool CanConfirmClick()
	{
		return ConsoleEntityProxy.CanConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return ConsoleEntityProxy.GetConfirmClickHint();
	}

	public void OnConfirmClick()
	{
		ConsoleEntityProxy.OnConfirmClick();
	}

	public bool CanDeclineClick()
	{
		return ConsoleEntityProxy.CanDeclineClick();
	}

	public string GetDeclineClickHint()
	{
		return ConsoleEntityProxy.GetDeclineClickHint();
	}

	public void OnDeclineClick()
	{
		ConsoleEntityProxy.OnDeclineClick();
	}

	public bool CanFunc01Click()
	{
		return ConsoleEntityProxy.CanFunc01Click();
	}

	public string GetFunc01ClickHint()
	{
		return ConsoleEntityProxy.GetFunc01ClickHint();
	}

	public void OnFunc01Click()
	{
		ConsoleEntityProxy.OnFunc01Click();
	}

	public bool CanFunc02Click()
	{
		return ConsoleEntityProxy.CanFunc02Click();
	}

	public string GetFunc02ClickHint()
	{
		return ConsoleEntityProxy.GetFunc02ClickHint();
	}

	public void OnFunc02Click()
	{
		ConsoleEntityProxy.OnFunc02Click();
	}

	public bool CanLongConfirmClick()
	{
		return ConsoleEntityProxy.CanLongConfirmClick();
	}

	public string GetLongConfirmClickHint()
	{
		return ConsoleEntityProxy.GetLongConfirmClickHint();
	}

	public void OnLongConfirmClick()
	{
		ConsoleEntityProxy.OnLongConfirmClick();
	}

	public bool CanLongDeclineClick()
	{
		return ConsoleEntityProxy.CanLongDeclineClick();
	}

	public string GetLongDeclineClickHint()
	{
		return ConsoleEntityProxy.GetLongDeclineClickHint();
	}

	public void OnLongDeclineClick()
	{
		ConsoleEntityProxy.OnLongDeclineClick();
	}

	public bool CanLongFunc01Click()
	{
		return ConsoleEntityProxy.CanLongFunc01Click();
	}

	public string GetLongFunc01ClickHint()
	{
		return ConsoleEntityProxy.GetLongFunc01ClickHint();
	}

	public void OnLongFunc01Click()
	{
		ConsoleEntityProxy.OnLongFunc01Click();
	}

	public bool CanLongFunc02Click()
	{
		return ConsoleEntityProxy.CanLongFunc02Click();
	}

	public string GetLongFunc02ClickHint()
	{
		return ConsoleEntityProxy.GetLongFunc02ClickHint();
	}

	public void OnLongFunc02Click()
	{
		ConsoleEntityProxy.OnLongFunc02Click();
	}
}
