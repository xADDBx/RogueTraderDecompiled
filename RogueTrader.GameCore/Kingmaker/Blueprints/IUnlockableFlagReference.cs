namespace Kingmaker.Blueprints;

public interface IUnlockableFlagReference
{
	UnlockableFlagReferenceType GetUsagesFor(BlueprintUnlockableFlag flag);
}
