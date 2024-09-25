using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.ElementsSystem;

[TypeId("b9ea3359b1204b798a61750d6cb4e723")]
public class ConditionsHolder : ElementsScriptableObject
{
	[NotNull]
	public ConditionsChecker Conditions = new ConditionsChecker();

	public bool Check()
	{
		return Conditions.Check();
	}
}
