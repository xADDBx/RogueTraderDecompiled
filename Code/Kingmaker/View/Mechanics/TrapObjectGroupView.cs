using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.View.Mechanics;

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
