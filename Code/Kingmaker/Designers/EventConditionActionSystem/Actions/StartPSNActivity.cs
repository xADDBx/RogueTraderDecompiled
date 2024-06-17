using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Console.PS5.PSNObjects;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("da31e8b1a8774c2cbb8611c1e7342fae")]
public class StartPSNActivity : GameAction
{
	[SerializeField]
	private string m_ActivityId;

	public override string GetCaption()
	{
		return "Start PSN activity (" + m_ActivityId + ")";
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
		}
		else
		{
			pSNObjects.StartActivity(m_ActivityId);
		}
	}
}
