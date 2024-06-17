using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.Globalmap.SystemMap;

public class StarSystemObjectContextData : ContextData<StarSystemObjectContextData>
{
	public StarSystemObjectEntity StarSystemObject;

	public StarSystemObjectContextData Setup(StarSystemObjectEntity starSystemObject)
	{
		StarSystemObject = starSystemObject;
		return this;
	}

	protected override void Reset()
	{
		StarSystemObject = null;
	}
}
