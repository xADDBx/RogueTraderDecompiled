using Kingmaker.ElementsSystem.ContextData;

namespace Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;

public class BlueprintUnitCheckerInEditorContextData : ContextData<BlueprintUnitCheckerInEditorContextData>
{
	public int AreaCR;

	public BlueprintUnitCheckerInEditorContextData Setup(int cr)
	{
		AreaCR = cr;
		return this;
	}

	protected override void Reset()
	{
		AreaCR = 0;
	}
}
