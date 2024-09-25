using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.QA.Arbiter;

[TypeId("3d109802e28d417c9caeda4b45e572e9")]
public class BlueprintArbiterInstruction : BlueprintScriptableObject
{
	public BlueprintComponent Test
	{
		get
		{
			if (base.ComponentsArray.Length == 0)
			{
				return null;
			}
			return base.ComponentsArray[0];
		}
	}

	public T GetTest<T>() where T : BlueprintComponent
	{
		return Test as T;
	}
}
