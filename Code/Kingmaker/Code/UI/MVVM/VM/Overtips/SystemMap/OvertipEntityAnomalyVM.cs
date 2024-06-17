using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Overtips.SystemMap;

public class OvertipEntityAnomalyVM : OvertipEntityVM, IAnomalyResearchHandler, ISubscriber<AnomalyEntityData>, ISubscriber
{
	public readonly MapObjectEntity SystemMapObject;

	public readonly ReactiveProperty<string> AnomalyName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> IsExplored = new ReactiveProperty<bool>(initialValue: false);

	public readonly AnomalyView AnomalyView;

	protected override Vector3 GetEntityPosition()
	{
		return SystemMapObject.Position;
	}

	public OvertipEntityAnomalyVM(MapObjectEntity systemMapObjectData)
	{
		AddDisposable(EventBus.Subscribe(this));
		SystemMapObject = systemMapObjectData;
		AnomalyView = SystemMapObject.View.GetComponent<AnomalyView>();
		UpdateAnomalyInfo();
	}

	public void UpdateAnomalyInfo()
	{
		if (!(AnomalyView == null))
		{
			AnomalyEntityData data = AnomalyView.Data;
			if (!string.IsNullOrEmpty(data.Blueprint.Name))
			{
				AnomalyName.Value = data.Blueprint.Name;
			}
			IsExplored.Value = data.IsInteracted;
			EventBus.RaiseEvent(delegate(IAnomalyUIHandler h)
			{
				h.UpdateAnomalyScreen(AnomalyView);
			});
		}
	}

	public void OpenAnomalyInfo()
	{
		EventBus.RaiseEvent(delegate(IAnomalyUIHandler h)
		{
			h.OpenAnomalyScreen(AnomalyView);
		});
	}

	public void HandleAnomalyStartResearch()
	{
		if (EventInvokerExtensions.Entity as AnomalyEntityData == AnomalyView.Data)
		{
			OpenAnomalyInfo();
		}
	}

	public void HandleAnomalyResearched(BaseUnitEntity unit, RulePerformSkillCheck skillCheck)
	{
		if (EventInvokerExtensions.Entity as AnomalyEntityData == AnomalyView.Data)
		{
			UpdateAnomalyInfo();
		}
	}

	public void RequestVisit()
	{
		VisitPlanet();
	}

	private void VisitPlanet()
	{
		Game.Instance.GameCommandQueue.MoveShip(AnomalyView, MoveShipGameCommand.VisitType.MovePlayerShip);
	}
}
