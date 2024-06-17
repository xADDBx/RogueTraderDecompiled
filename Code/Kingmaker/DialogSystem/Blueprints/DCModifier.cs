using System;
using Kingmaker.ElementsSystem;

namespace Kingmaker.DialogSystem.Blueprints;

[Serializable]
public class DCModifier
{
	public int Mod;

	public ConditionsChecker Conditions = new ConditionsChecker();
}
