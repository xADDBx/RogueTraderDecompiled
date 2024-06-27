namespace Kingmaker.Utility.UnityExtensions;

public class WebSenderEntry
{
	public string Name;

	public byte[] Data;

	public WebSenderEntry(string Name, byte[] Data)
	{
		this.Name = Name;
		this.Data = Data;
	}
}
