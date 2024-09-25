namespace Kingmaker.DialogSystem.Blueprints;

public interface IDialogReference
{
	DialogReferenceType GetUsagesFor(BlueprintDialog dialog);
}
