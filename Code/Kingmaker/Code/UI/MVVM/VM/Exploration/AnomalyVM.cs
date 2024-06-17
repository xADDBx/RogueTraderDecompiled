using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.StatCheckLoot;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Exploration;

public class AnomalyVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IAnomalyUIHandler, ISubscriber
{
	public readonly ReactiveProperty<string> AnomalyName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> AnomalyDescription = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<string> VisitButtonLabel = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> IsFullyScanned = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveCommand Show = new ReactiveCommand();

	public readonly ReactiveCommand Hide = new ReactiveCommand();

	public readonly StatCheckLootAnomalyVM StatCheckLootAnomalyVM;

	private AnomalyView m_AnomalyView;

	public AnomalyVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		AddDisposable(StatCheckLootAnomalyVM = new StatCheckLootAnomalyVM());
	}

	protected override void DisposeImplementation()
	{
	}

	public void OpenAnomalyScreen(AnomalyView anomalyObject)
	{
		m_AnomalyView = anomalyObject;
		AnomalyEntityData data = anomalyObject.Data;
		AnomalyName.Value = data.Blueprint.Name;
		AnomalyDescription.Value = data.Blueprint.Description;
		VisitButtonLabel.Value = (data.IsInteracted ? UIStrings.Instance.ExplorationTexts.AnomalyVisitExplored : UIStrings.Instance.ExplorationTexts.AnomalyVisitUnknown);
		IsFullyScanned.Value = data.IsInteracted;
		Show.Execute();
	}

	public void UpdateAnomalyScreen(AnomalyView anomalyObject)
	{
		m_AnomalyView = anomalyObject;
		IsFullyScanned.Value = anomalyObject.Data.IsInteracted;
	}

	public void CloseAnomalyScreen()
	{
		Hide.Execute();
	}

	public void VisitAnomaly()
	{
		Game.Instance.GameCommandQueue.MoveShip(m_AnomalyView, MoveShipGameCommand.VisitType.MovePlayerShip);
	}
}
