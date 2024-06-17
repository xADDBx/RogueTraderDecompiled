namespace Owlcat.Core.Overlays;

public class OverlayElement
{
	public string Name { get; }

	public bool Hidden { get; set; }

	protected OverlayElement(string name)
	{
		Name = name;
	}
}
