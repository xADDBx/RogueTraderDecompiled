namespace Kingmaker.AreaLogic.Cutscenes;

public enum CutscenePauseReason
{
	HasNoActiveAnchors,
	GameModePauseBackgroundCutscenes,
	MarkedUnitControlledByOtherCutscene,
	UnitSpawnerDoesNotControlAnyUnit,
	ManualPauseFromEditor
}
