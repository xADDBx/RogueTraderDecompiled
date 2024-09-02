namespace UnityEngine.UI.Extensions;

public struct HsvColor
{
	public double H;

	public double S;

	public double V;

	public HsvColor(double h, double s, double v)
	{
		H = h;
		S = s;
		V = v;
	}

	public override string ToString()
	{
		return "{" + H + "," + S + "," + V + "}";
	}
}
