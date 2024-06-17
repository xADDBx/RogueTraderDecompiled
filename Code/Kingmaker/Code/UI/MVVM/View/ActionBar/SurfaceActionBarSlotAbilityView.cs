namespace Kingmaker.Code.UI.MVVM.View.ActionBar;

public abstract class SurfaceActionBarSlotAbilityView : ActionBarBaseSlotView
{
	public int Index => base.ViewModel?.Index ?? (-1);

	public bool IsHeroicAct => base.ViewModel?.IsHeroicAct ?? false;

	public bool IsDesperateMeasure => base.ViewModel?.IsDesperateMeasure ?? false;
}
