using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("862e6465fbfe41e4ad63ce755dbb2c1d")]
public class ReturnFromGroundOperation : GameAction
{
	[SerializeField]
	[CanBeNull]
	private BlueprintAreaEnterPointReference m_AreaEnterPoint;

	[SerializeField]
	private AutoSaveMode m_AutoSaveMode;

	public override string GetCaption()
	{
		return "Return from area to last Sector/System map";
	}

	public override void RunAction()
	{
		BlueprintArea previousVisitedArea = Game.Instance.Player.PreviousVisitedArea;
		BlueprintAreaEnterPoint enterPoint = ((previousVisitedArea != null) ? previousVisitedArea.DefaultPreset.EnterPoint : m_AreaEnterPoint?.Get());
		if (enterPoint == null)
		{
			return;
		}
		EventBus.RaiseEvent(delegate(IPartyLeaveAreaHandler h)
		{
			h.HandlePartyLeaveArea(Game.Instance.CurrentlyLoadedArea, enterPoint);
		});
		Vector3? mapCoord = ((previousVisitedArea == null) ? Game.Instance.Player.LastPositionOnPreviousVisitedArea : null);
		Game.Instance.LoadArea(enterPoint, m_AutoSaveMode, delegate
		{
			if (mapCoord.HasValue)
			{
				Game.Instance.Player.PlayerShip.Position = mapCoord.Value;
			}
		});
	}
}
