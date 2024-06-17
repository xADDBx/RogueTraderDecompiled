namespace Kingmaker.UI.SurfaceCombatHUD;

public struct AreaData
{
	public uint flag;

	public IAreaSource source;

	public int intersectionFlagShift;

	public bool isStratagem;

	public AreaData(uint flag, IAreaSource source, int intersectionFlagShift = 0, bool isStratagem = false)
	{
		this.flag = flag;
		this.source = source;
		this.intersectionFlagShift = intersectionFlagShift;
		this.isStratagem = isStratagem;
	}
}
