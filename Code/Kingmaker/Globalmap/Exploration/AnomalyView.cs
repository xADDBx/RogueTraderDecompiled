using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.View.MapObjects.SriptZones;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

[KnowledgeDatabaseID("c5e5cb6942bb46cda47cbfd039659697")]
public class AnomalyView : StarSystemObjectView
{
	private bool m_WasStarShipInInteractionRadius;

	public StarSystemObjectView BlockedObject;

	private IScriptZoneShape m_ScriptZone;

	public BlueprintAnomaly BlueprintAnomaly => (BlueprintAnomaly)(Blueprint?.Get());

	public new AnomalyEntityData Data => (AnomalyEntityData)base.Data;

	protected override Entity CreateEntityDataImpl(bool load)
	{
		return Entity.Initialize(new AnomalyEntityData(this, BlueprintAnomaly));
	}

	protected override void OnDidAttachToData()
	{
		m_ScriptZone = GetComponentInChildren<IScriptZoneShape>();
	}

	public void Tick()
	{
		if (Data.CanInteract() && Data.InteractTime != BlueprintAnomaly.AnomalyInteractTime.ByClick)
		{
			if (BlueprintAnomaly.InteractTime == BlueprintAnomaly.AnomalyInteractTime.OnTouch)
			{
				return;
			}
			if (IsInInteractionZone())
			{
				if (!m_WasStarShipInInteractionRadius || StarSystemMapClickObjectHandler.DestinationSso == this)
				{
					InteractWithAnomaly();
					m_WasStarShipInInteractionRadius = true;
				}
			}
			else
			{
				m_WasStarShipInInteractionRadius = false;
			}
		}
		if (!Data.IsMoving)
		{
			return;
		}
		if ((double)(base.ViewTransform.position - Data.Destination).magnitude < 0.1)
		{
			Data.IsMoving = false;
			if (Data.RemoveAfterMove)
			{
				Data.IsInGame = false;
			}
		}
		else
		{
			Vector3 normalized = (Data.Destination - base.ViewTransform.position).normalized;
			base.ViewTransform.position += normalized * (Data.Speed * Game.Instance.TimeController.DeltaTime);
		}
	}

	public void InteractWithAnomaly()
	{
		StarSystemMapMoveController.StopPlayerShip();
		Data.Interact();
		StarSystemMapClickObjectHandler.DestinationSso = null;
	}

	private bool IsInInteractionZone()
	{
		StarSystemShip starSystemShip = Game.Instance.StarSystemMapController.StarSystemShip;
		if (BlueprintAnomaly.InteractTime == BlueprintAnomaly.AnomalyInteractTime.OnTouch)
		{
			return false;
		}
		if (m_ScriptZone != null)
		{
			return m_ScriptZone.Contains(starSystemShip.Position);
		}
		return (starSystemShip.Position - base.transform.position).magnitude <= Data.InteractDistance;
	}

	public override bool CheckLanding()
	{
		return BlueprintAnomaly.InteractTime == BlueprintAnomaly.AnomalyInteractTime.OnTouch;
	}
}
