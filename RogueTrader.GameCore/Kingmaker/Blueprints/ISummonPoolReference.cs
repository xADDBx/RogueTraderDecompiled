namespace Kingmaker.Blueprints;

public interface ISummonPoolReference
{
	SummonPoolReferenceType GetUsagesFor(BlueprintUnlockableFlag flag);
}
