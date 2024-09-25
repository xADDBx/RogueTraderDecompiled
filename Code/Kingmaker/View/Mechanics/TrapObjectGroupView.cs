using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.View.Mechanics;

[KnowledgeDatabaseID("fbd3990a28d24e6188cb835ef85b43d3")]
public class TrapObjectGroupView : MechanicGroupView<TrapObjectView>
{
	protected override void OnActivate()
	{
		base.OnActivate();
		TrapObjectView[] childrenViews = base.Data.ChildrenViews;
		foreach (TrapObjectView trapObjectView in childrenViews)
		{
			trapObjectView.OnAreaDidLoad();
			Game.Instance.EntitySpawner.SpawnEntityWithView(trapObjectView.Settings.ScriptZoneTrigger, Game.Instance.LoadedAreaState.MainState, moveView: false);
		}
	}
}
