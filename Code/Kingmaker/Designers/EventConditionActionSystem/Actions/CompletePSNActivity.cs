using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Console.PS5.PSNObjects;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("c18b86eb68624267a9a7f63ea0f8b438")]
public class CompletePSNActivity : GameAction
{
	private enum CompleteResult
	{
		Complete,
		Failed,
		Abandoned
	}

	[SerializeField]
	private string m_ActivityId;

	[SerializeField]
	private CompleteResult m_CompleteResult;

	public override string GetCaption()
	{
		return m_CompleteResult switch
		{
			CompleteResult.Complete => "Complete", 
			CompleteResult.Failed => "Fail", 
			CompleteResult.Abandoned => "Abandon", 
			_ => throw new ArgumentOutOfRangeException(), 
		} + " PSN activity (" + m_ActivityId + ")";
	}

	public override void RunAction()
	{
		if (string.IsNullOrEmpty(m_ActivityId))
		{
			PFLog.Actions.Error("StartPSNActivity: Activity id is null or empty");
			return;
		}
		PSNObjectsManager pSNObjects = Game.Instance.Player.PSNObjects;
		if (pSNObjects == null)
		{
			PFLog.Actions.Error("PSNObjectsManager is null");
			return;
		}
		switch (m_CompleteResult)
		{
		case CompleteResult.Complete:
			pSNObjects.CompleteActivity(m_ActivityId);
			break;
		case CompleteResult.Failed:
			pSNObjects.FailActivity(m_ActivityId);
			break;
		case CompleteResult.Abandoned:
			pSNObjects.AbandonActivity(m_ActivityId);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
