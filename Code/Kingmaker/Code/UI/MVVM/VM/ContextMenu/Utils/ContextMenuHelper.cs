using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;

public static class ContextMenuHelper
{
	private static bool s_Show;

	private static IDisposable SetContextMenu(this MonoBehaviour component, List<ContextMenuCollectionEntity> entities, bool isLeftClick = false)
	{
		if (entities.Empty())
		{
			return Disposable.Empty;
		}
		RectTransform owner = ((component != null) ? (component.transform as RectTransform) : null);
		ContextMenuCollection collection = new ContextMenuCollection(entities, owner);
		if (!collection.IsValid)
		{
			return Disposable.Empty;
		}
		s_Show = false;
		IDisposable rightClick = component.OnPointerClickAsObservable().Subscribe(delegate(PointerEventData data)
		{
			if (!isLeftClick)
			{
				if (data.button == PointerEventData.InputButton.Right)
				{
					TooltipHelper.HideTooltip();
					EventBus.RaiseEvent(delegate(IContextMenuHandler h)
					{
						h.HandleContextMenuRequest((!s_Show) ? collection : null);
					});
					s_Show = !s_Show;
				}
			}
			else if (data.button == PointerEventData.InputButton.Left)
			{
				TooltipHelper.HideTooltip();
				EventBus.RaiseEvent(delegate(IContextMenuHandler h)
				{
					h.HandleContextMenuRequest((!s_Show) ? collection : null);
				});
				s_Show = !s_Show;
			}
		});
		return Disposable.Create(delegate
		{
			if (s_Show)
			{
				s_Show = false;
				EventBus.RaiseEvent(delegate(IContextMenuHandler h)
				{
					h.HandleContextMenuRequest(null);
				});
			}
			rightClick?.Dispose();
		});
	}

	public static IDisposable SetContextMenu(this MonoBehaviour component, ReactiveProperty<List<ContextMenuCollectionEntity>> entities, bool isLeftClick = false)
	{
		IDisposable menuSubscription = null;
		IDisposable entitiesSubscription = entities.Subscribe(delegate(List<ContextMenuCollectionEntity> list)
		{
			menuSubscription?.Dispose();
			menuSubscription = component.SetContextMenu(list, isLeftClick);
		});
		return Disposable.Create(delegate
		{
			menuSubscription?.Dispose();
			entitiesSubscription?.Dispose();
		});
	}

	public static void ShowContextMenu(this MonoBehaviour component, List<ContextMenuCollectionEntity> entities)
	{
		if (entities.Empty())
		{
			return;
		}
		RectTransform owner = ((component != null) ? (component.transform as RectTransform) : null);
		ContextMenuCollection collection = new ContextMenuCollection(entities, owner);
		if (collection.IsValid)
		{
			EventBus.RaiseEvent(delegate(IContextMenuHandler h)
			{
				h.HandleContextMenuRequest(collection);
			});
		}
	}

	public static void HideContextMenu()
	{
		s_Show = false;
		EventBus.RaiseEvent(delegate(IContextMenuHandler h)
		{
			h.HandleContextMenuRequest(null);
		});
	}

	public static bool ContextMenuIsShow()
	{
		return s_Show;
	}
}
