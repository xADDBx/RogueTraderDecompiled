using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Console.PS5.PSNObjects;

[TypeId("650ba41a69c44266a54a45e3ee6e4bee")]
public class BlueprintPSNObjectsRoot : BlueprintScriptableObject
{
	[Serializable]
	public class TaskData
	{
		public string ObjectId;

		public string[] SubTaskObjectsIds;
	}

	[Serializable]
	public class ActivityData
	{
		public string ObjectId;

		public TaskData[] Tasks;
	}

	public ActivityData[] Activities;
}
