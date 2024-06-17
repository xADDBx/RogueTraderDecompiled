using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.EditorDatabase;

public class FolderWrapper : ScriptableObject
{
	public string Path { get; private set; }

	public FolderWrapper Setup(string path, string folderName)
	{
		Path = path;
		base.name = folderName;
		return this;
	}
}
