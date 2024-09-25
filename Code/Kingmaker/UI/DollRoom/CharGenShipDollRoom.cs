namespace Kingmaker.UI.DollRoom;

public class CharGenShipDollRoom : ShipDollRoom
{
	public override void Show()
	{
		base.Show();
		SetupDollPostProcessAndAnimation(isCharGen: true);
	}
}
