using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;

namespace Kingmaker.DialogSystem;

public class AnswerDataContext : ContextData<AnswerDataContext>
{
	public BlueprintAnswer Blueprint;

	public AnswerDataContext Setup(BlueprintAnswer blueprint)
	{
		Blueprint = blueprint;
		return this;
	}

	protected override void Reset()
	{
		Blueprint = null;
	}
}
