using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Console.PS5.PSNObjects;

[JsonObject]
public class PSNObjectsState
{
	[JsonProperty]
	private List<Activity> m_Activities = new List<Activity>();

	private Dictionary<string, ActivityBase> m_Cache = new Dictionary<string, ActivityBase>();

	private bool m_shouldUpdateCache = true;

	public IReadOnlyList<Activity> Activities => m_Activities;

	public PSNObjectsState(BlueprintPSNObjectsRoot.ActivityData[] activities)
	{
		foreach (BlueprintPSNObjectsRoot.ActivityData obj in activities)
		{
			Activity activity = new Activity(obj.ObjectId);
			m_Activities.Add(activity);
			BlueprintPSNObjectsRoot.TaskData[] tasks = obj.Tasks;
			foreach (BlueprintPSNObjectsRoot.TaskData obj2 in tasks)
			{
				Task task = new Task(obj2.ObjectId);
				activity.Tasks.Add(task);
				string[] subTaskObjectsIds = obj2.SubTaskObjectsIds;
				for (int k = 0; k < subTaskObjectsIds.Length; k++)
				{
					SubTask item = new SubTask(subTaskObjectsIds[k]);
					task.SubTasks.Add(item);
				}
			}
		}
		UpdateCache();
	}

	[JsonConstructor]
	private PSNObjectsState()
	{
		m_shouldUpdateCache = true;
	}

	private void UpdateCache()
	{
		m_Cache.Clear();
		foreach (Activity activity in Activities)
		{
			m_Cache.Add(activity.Id, activity);
			foreach (Task task in activity.Tasks)
			{
				m_Cache.Add(task.Id, task);
				foreach (SubTask subTask in task.SubTasks)
				{
					m_Cache.Add(subTask.Id, subTask);
				}
			}
		}
		m_shouldUpdateCache = false;
	}

	public bool SetStatus(string activityId, ActivityStatus status)
	{
		if (m_shouldUpdateCache)
		{
			UpdateCache();
		}
		if (m_Cache.TryGetValue(activityId, out var value))
		{
			value.Status = status;
		}
		return value != null;
	}
}
