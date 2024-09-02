namespace UnityEngine.UI.Extensions;

public class ReorderableListDebug : MonoBehaviour
{
	public Text DebugLabel;

	private void Awake()
	{
		ReorderableList[] array = Object.FindObjectsOfType<ReorderableList>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].OnElementDropped.AddListener(ElementDropped);
		}
	}

	private void ElementDropped(ReorderableList.ReorderableListEventStruct droppedStruct)
	{
		DebugLabel.text = "";
		Text debugLabel = DebugLabel;
		debugLabel.text = debugLabel.text + "Dropped Object: " + droppedStruct.DroppedObject.name + "\n";
		Text debugLabel2 = DebugLabel;
		debugLabel2.text = debugLabel2.text + "Is Clone ?: " + droppedStruct.IsAClone + "\n";
		if (droppedStruct.IsAClone)
		{
			Text debugLabel3 = DebugLabel;
			debugLabel3.text = debugLabel3.text + "Source Object: " + droppedStruct.SourceObject.name + "\n";
		}
		DebugLabel.text += $"From {droppedStruct.FromList.name} at Index {droppedStruct.FromIndex} \n";
		DebugLabel.text += $"To {droppedStruct.ToList.name} at Index {droppedStruct.ToIndex} \n";
	}
}
