using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.View.Mechanics.Entities;

namespace Kingmaker.Code.View.Mechanics.Entities.Covers;

[KnowledgeDatabaseID("8c00905928b4480793858c10e26e6a3d")]
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
