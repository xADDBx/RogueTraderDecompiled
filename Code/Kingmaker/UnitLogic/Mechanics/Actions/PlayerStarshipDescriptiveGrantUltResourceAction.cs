using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[ComponentName("Actions/PlayerStarshipDescriptiveGrantUltResourceAction")]
[AllowMultipleComponents]
[TypeId("75e3029993d72e845986afa840aa24bc")]
public class PlayerStarshipDescriptiveGrantUltResourceAction : GameAction
{
	private enum AmountValue
	{
		Low,
		Average,
		High
	}

	[SerializeField]
	private AmountValue m_AmountValue = AmountValue.Average;

	[SerializeField]
	private WarhammerRestoreResourcesSet.RestoreMode m_RestoreMode;

	[SerializeField]
	private bool LoseInstead;

	protected override void RunAction()
	{
		int num = m_AmountValue switch
		{
			AmountValue.Low => 25, 
			AmountValue.Average => 50, 
			AmountValue.High => 100, 
			_ => 0, 
		};
		if (LoseInstead)
		{
			WarhammerRestoreResourcesSet.SpendUltimateResources(Game.Instance.Player.PlayerShip, num, m_RestoreMode);
		}
		else
		{
			WarhammerRestoreResourcesSet.GrantUltimateResources(Game.Instance.Player.PlayerShip, 0, num, m_RestoreMode);
		}
	}

	public override string GetCaption()
	{
		string arg = (LoseInstead ? "Make player starship lose" : "Grant player starship");
		return $"{arg} \"{m_AmountValue}\" amount of ultimate abilities resources in \"{m_RestoreMode}\" mode";
	}
}
