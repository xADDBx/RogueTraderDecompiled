using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Interaction;
using Newtonsoft.Json;

namespace Kingmaker.AreaLogic.Etudes;

public class EtudeBracketOverrideUnitInteraction : IUnitInteraction
{
	[JsonProperty]
	public readonly IEtudeBracketOverrideInteraction Source;

	public int Distance => Source.Distance;

	public bool IsApproach => false;

	public float ApproachCooldown => 5f;

	public bool MainPlayerPreferred => true;

	[JsonConstructor]
	protected EtudeBracketOverrideUnitInteraction()
	{
	}

	public EtudeBracketOverrideUnitInteraction(IEtudeBracketOverrideInteraction source)
	{
		Source = source;
	}

	public AbstractUnitCommand.ResultType Interact(BaseUnitEntity user, AbstractUnitEntity target)
	{
		return Source.Interact(user, target);
	}

	public bool IsAvailable(BaseUnitEntity initiator, AbstractUnitEntity target)
	{
		return true;
	}
}
