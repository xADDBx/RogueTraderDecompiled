using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.AreaLogic.Capital;

[AllowMultipleComponents]
[TypeId("5386e8e42dfb3e24d9dd5ec17c50c0c7")]
public class CapitalExit : GameAction, IAreaEnterPointReference
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintAreaEnterPointReference m_Destination;

	public AutoSaveMode AutoSaveMode = AutoSaveMode.BeforeExit;

	public BlueprintAreaEnterPoint Destination => m_Destination?.Get();

	public override string GetCaption()
	{
		return $"Capital exit to {Destination}";
	}

	public bool GetUsagesFor(BlueprintAreaEnterPoint point)
	{
		return point == Destination;
	}

	public override void RunAction()
	{
		if (Game.Instance.Player.CapitalPartyMode)
		{
			CapitalCompanionLogic.ExitCapital(Destination, AutoSaveMode);
		}
		else if (Destination.Area == Game.Instance.CurrentlyLoadedArea)
		{
			Game.Instance.Teleport(Destination);
		}
		else
		{
			Game.Instance.LoadArea(Destination, AutoSaveMode);
		}
	}
}
