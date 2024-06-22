using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Debug;

[Serializable]
[TypeId("22dbdf57708a4afd8603a76c37654f3b")]
public class ThrowExceptionGetter : PropertyGetter
{
	public string Message = "test";

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Throw exception: " + Message;
	}
}
