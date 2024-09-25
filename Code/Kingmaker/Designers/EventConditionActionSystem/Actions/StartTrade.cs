using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("63f4331f2b9d14b4cbdca44a66b1bd43")]
public class StartTrade : GameAction
{
	[SerializeField]
	private bool m_IsUnitVendor = true;

	[ShowIf("m_IsUnitVendor")]
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Vendor;

	[HideIf("m_IsUnitVendor")]
	[ValidateNotNull]
	[SerializeReference]
	public MapObjectEvaluator SpaceVendor;

	public override string GetCaption()
	{
		return $"Start Trade {Vendor}";
	}

	protected override void RunAction()
	{
		if (m_IsUnitVendor)
		{
			EventBus.RaiseEvent((IMechanicEntity)Vendor.GetValue(), (Action<IVendorUIHandler>)delegate(IVendorUIHandler h)
			{
				h.HandleTradeStarted();
			}, isCheckRuntime: true);
		}
		else
		{
			EventBus.RaiseEvent((IMechanicEntity)SpaceVendor.GetValue(), (Action<IVendorUIHandler>)delegate(IVendorUIHandler h)
			{
				h.HandleTradeStarted();
			}, isCheckRuntime: true);
		}
	}
}
