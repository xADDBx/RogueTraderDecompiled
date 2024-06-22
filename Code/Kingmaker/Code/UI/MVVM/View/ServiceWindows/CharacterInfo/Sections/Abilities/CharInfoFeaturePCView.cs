using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.View.ActionBar.PC;
using Kingmaker.Code.UI.MVVM.VM.ActionBar.Surface;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.DragNDrop;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Abilities;

public class CharInfoFeaturePCView : CharInfoFeatureSimpleBaseView
{
	[Header("Tooltip")]
	[SerializeField]
	protected RectTransform m_LeftSideTooltipPlace;

	[Header("Drag'n'Drop")]
	[SerializeField]
	private DragNDropHandler m_DragNDropHandler;

	private List<Vector2> LeftSideTooltipPivot { get; } = new List<Vector2>
	{
		new Vector2(1f, 0.5f)
	};


	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		SetTooltip();
		SetupDragNDrop();
	}

	private void SetTooltip()
	{
		if (base.ViewModel.Tooltip != null)
		{
			AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, m_LeftSideTooltipPlace, 0, 0, 0, LeftSideTooltipPivot)));
		}
	}

	private void SetupDragNDrop()
	{
		if ((bool)m_DragNDropHandler)
		{
			MechanicEntity mechanicEntity = base.ViewModel.Ability?.ConcreteOwner;
			m_DragNDropHandler.CanDrag = base.ViewModel.Ability != null && (mechanicEntity.IsMyNetRole() || mechanicEntity.InPartyAndControllable());
			AddDisposable(m_DragNDropHandler.OnDragEnd.Subscribe(OnDragEnd));
		}
	}

	private void OnDragEnd(GameObject dropTarget)
	{
		SurfaceActionBarSlotAbilityPCView targetSlot = dropTarget.Or(null)?.GetComponent<SurfaceActionBarSlotAbilityPCView>();
		if ((bool)targetSlot)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(base.ViewModel.Ability, targetSlot.Index);
			});
		}
	}
}
