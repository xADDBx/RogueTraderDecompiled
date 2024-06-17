using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.NamedParameters;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[TypeId("c17378fb089d4796a26f5e8fa2c2c32f")]
public class CommandReevaluateParameters : CommandBase
{
	public override string GetCaption()
	{
		return "[HACK] Reevaluate parameters";
	}

	protected override void OnRun(CutscenePlayerData player, bool skipping)
	{
		ParametrizedContextSetter.ParameterEntry[] array = (player.ParameterSetter?.Parameters).EmptyIfNull();
		foreach (ParametrizedContextSetter.ParameterEntry parameterEntry in array)
		{
			player.Parameters.Params[parameterEntry.Name] = parameterEntry.GetValue();
		}
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return true;
	}

	protected override void OnSetTime(double time, CutscenePlayerData player)
	{
	}
}
