using Kingmaker.Controllers.TurnBased;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

public class BaseCoverEntityView : DestructibleEntityView
{
	protected override bool HasHighlight => true;

	private bool DebugHighlightCovers => Game.Instance.InteractionHighlightController?.DebugHighlightCovers ?? false;

	protected override bool GlobalHighlighting
	{
		get
		{
			if (!base.GlobalHighlighting)
			{
				return DebugHighlightCovers;
			}
			return true;
		}
	}

	protected override bool HighlightOnHover
	{
		get
		{
			if ((!base.HighlightOnHover && !base.Highlighted) || !TurnController.IsInTurnBasedCombat())
			{
				return DebugHighlightCovers;
			}
			return true;
		}
	}
}
