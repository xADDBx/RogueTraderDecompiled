using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.TextTools.Base;

namespace Kingmaker.TextTools;

public class DateTemplate : TextTemplate
{
	public override int MinParameters => 1;

	public override int MaxParameters => 1;

	public override int Balance => 1;

	public override string Generate(bool capitalized, List<string> parameters)
	{
		if (parameters.Count < 1)
		{
			PFLog.Default.Error("DateTemplate.Generate: parameter is missing");
		}
		string format = parameters[0];
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		return BlueprintRoot.Instance.Calendar.GetDateText(gameTime, format);
	}
}
