using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Kingmaker.QA.Arbiter;

[Serializable]
[TypeId("2c779547e76d4d068e439172843a1972")]
public class ArbiterElement : Element
{
	public override string GetCaption()
	{
		return "ArbiterElement";
	}
}
