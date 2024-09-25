using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.DialogSystem.Blueprints;
using Newtonsoft.Json;

namespace Kingmaker.QA.Clockwork;

public class PlayData
{
	public class AreaEntry
	{
		[JsonProperty]
		public string AreaId;

		[JsonProperty]
		public string AreaName;

		[JsonProperty]
		public string AreaPart;

		[JsonProperty]
		public int Depth;

		[JsonProperty]
		public bool Visited;

		[JsonProperty]
		public readonly List<AreaMechanicSet> MechanicSetsSeen = new List<AreaMechanicSet>();

		[JsonProperty]
		public TimeSpan LastVisitTime;
	}

	public class AreaMechanicSet
	{
		[JsonProperty]
		public List<string> MechanicScenes;

		public bool Matches(IEnumerable<string> mechSet)
		{
			return MechanicScenes.SequenceEqual(mechSet);
		}
	}

	public class DialogEntry
	{
		[JsonProperty]
		public string DialogId;

		[JsonProperty]
		public bool Seen;

		[JsonProperty]
		public TimeSpan LastTalkTime;
	}

	[JsonProperty]
	public string SaveName;

	[JsonProperty]
	public readonly Dictionary<string, int> LocalTransitionUseCount = new Dictionary<string, int>();

	[JsonProperty]
	public readonly HashSet<string> InteractedObjects = new HashSet<string>();

	[JsonProperty]
	public readonly HashSet<string> UnreachableObjects = new HashSet<string>();

	[JsonProperty]
	public readonly List<AreaEntry> AreasSeen = new List<AreaEntry>();

	[JsonProperty]
	public readonly List<DialogEntry> DialogsSeen = new List<DialogEntry>();

	public void MarkUnreachable(string uniqueId)
	{
		UnreachableObjects.Add(uniqueId);
	}

	public void CountLocalTransitionUse(string guid)
	{
		if (!LocalTransitionUseCount.ContainsKey(guid))
		{
			LocalTransitionUseCount[guid] = 0;
		}
		LocalTransitionUseCount[guid]++;
	}

	public AreaEntry GetAreaData(BlueprintArea area, BlueprintAreaPart areaPart = null)
	{
		string guid = SimpleBlueprintExtendAsObject.Or(areaPart, null)?.AssetGuid ?? area.AssetGuid;
		AreaEntry areaEntry = AreasSeen.FirstOrDefault((AreaEntry e) => e.AreaId == guid);
		if (areaEntry == null)
		{
			List<AreaEntry> areasSeen = AreasSeen;
			AreaEntry obj = new AreaEntry
			{
				AreaId = guid,
				AreaName = area.AreaName,
				AreaPart = SimpleBlueprintExtendAsObject.Or(areaPart, null)?.name
			};
			areaEntry = obj;
			areasSeen.Add(obj);
		}
		return areaEntry;
	}

	public DialogEntry GetDialogData(BlueprintDialog dialog)
	{
		DialogEntry dialogEntry = DialogsSeen.FirstOrDefault((DialogEntry e) => e.DialogId == dialog.AssetGuid);
		if (dialogEntry == null)
		{
			List<DialogEntry> dialogsSeen = DialogsSeen;
			DialogEntry obj = new DialogEntry
			{
				DialogId = dialog.AssetGuid
			};
			dialogEntry = obj;
			dialogsSeen.Add(obj);
		}
		return dialogEntry;
	}
}
