using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items;
using Kingmaker.Interaction;
using Kingmaker.Items;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.InteractionRestrictions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.VariativeInteraction;

public class VariativeInteractionVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly MapObjectView MapObjectView;

	private readonly Action m_CloseCallback;

	public readonly AutoDisposingReactiveCollection<InteractionVariantVM> Variants = new AutoDisposingReactiveCollection<InteractionVariantVM>();

	public Vector3 ObjectWorldPosition
	{
		get
		{
			if (!CheckMapObject())
			{
				return Vector3.zero;
			}
			return MapObjectView.transform.position;
		}
	}

	private bool CheckMapObject()
	{
		if (MapObjectView != null)
		{
			return true;
		}
		m_CloseCallback?.Invoke();
		return false;
	}

	public VariativeInteractionVM(MapObjectView mapObjectView, Action closeCallback)
	{
		MapObjectView = mapObjectView;
		m_CloseCallback = closeCallback;
		foreach (IInteractionVariantActor item2 in GetIHasInteractionVariants(MapObjectView)?.GetInteractionVariantActors())
		{
			if (item2 is UnlockRestrictionPart)
			{
				continue;
			}
			BlueprintItem item = item2.RequiredItem;
			int? resourceCount = null;
			int? requiredResourceCount = null;
			if (item != null)
			{
				resourceCount = Game.Instance.Player.Inventory.Items.Where((ItemEntity i) => i.Blueprint == item).Sum((ItemEntity i) => i.Count);
				requiredResourceCount = item2.RequiredItemsCount;
			}
			Variants.Add(new InteractionVariantVM(mapObjectView, item2, item?.Name, resourceCount, requiredResourceCount, Close));
		}
	}

	protected override void DisposeImplementation()
	{
		Variants.Clear();
	}

	public void Close()
	{
		m_CloseCallback?.Invoke();
	}

	private static IHasInteractionVariantActors GetIHasInteractionVariants([CanBeNull] MapObjectView mapObjectView)
	{
		if (mapObjectView == null)
		{
			return null;
		}
		return mapObjectView.Data.Parts.GetAll<InteractionPart>().FirstOrDefault((InteractionPart i) => i is IHasInteractionVariantActors && i.Enabled) as IHasInteractionVariantActors;
	}

	public static bool HasVariativeInteraction([CanBeNull] MapObjectView mapObjectView)
	{
		if (mapObjectView == null)
		{
			return false;
		}
		return (GetIHasInteractionVariants(mapObjectView)?.GetInteractionVariantActors()?.Any()).GetValueOrDefault();
	}
}
