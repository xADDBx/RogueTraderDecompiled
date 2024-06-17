namespace Code.GameCore.Blueprints.BlueprintPatcher;

public enum BlueprintPatchOperationType
{
	Undefined = -1,
	Override,
	Union,
	UnionDistinct,
	InsertAfterElement,
	InsertBeforeElement,
	InsertAtBeginning,
	InsertLast,
	ReplaceElement,
	RemoveElement
}
