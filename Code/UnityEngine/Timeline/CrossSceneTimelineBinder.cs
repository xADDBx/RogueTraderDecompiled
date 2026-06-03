using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.Utility.Attributes;
using UnityEngine.Playables;

namespace UnityEngine.Timeline;

public class CrossSceneTimelineBinder : MonoBehaviour
{
	[Serializable]
	private class BindEntry
	{
		[HideInInspector]
		public string? Name;

		[InspectorReadOnly]
		public string? BindKey;

		public ControllableReference? CrossSceneObject;
	}

	private TimelineAsset? _timeline;

	private PlayableDirector? _director;

	[SerializeField]
	private BindEntry[] CrossSceneBindings = new BindEntry[0];

	private void Init()
	{
		if (_director == null)
		{
			_director = GetComponent<PlayableDirector>();
			if (_director == null)
			{
				return;
			}
			_director.played += OnPlayed;
		}
		_timeline = _director.playableAsset as TimelineAsset;
	}

	private void Awake()
	{
		Init();
	}

	private void OnPlayed(PlayableDirector obj)
	{
		RestoreBindings();
	}

	public void RestoreBindings()
	{
		if (_director == null || _timeline == null || CrossSceneBindings.Length == 0)
		{
			return;
		}
		Dictionary<string, TrackAsset> dictionary = (from t in _timeline.GetOutputTracks()
			where t is ICrossSceneTrack
			select t).ToDictionary((TrackAsset t) => (t as ICrossSceneTrack)?.BindKey, (TrackAsset t) => t);
		List<BindEntry> list = new List<BindEntry>(CrossSceneBindings.Length);
		BindEntry[] crossSceneBindings = CrossSceneBindings;
		foreach (BindEntry bindEntry in crossSceneBindings)
		{
			if (!dictionary.TryGetValue(bindEntry.BindKey, out var value))
			{
				list.Add(bindEntry);
			}
			else
			{
				if (bindEntry.CrossSceneObject == null)
				{
					continue;
				}
				ControllableComponent controllable;
				if (Application.isPlaying)
				{
					if (!bindEntry.CrossSceneObject.TryGetValue(out controllable))
					{
						continue;
					}
				}
				else
				{
					controllable = ControllableComponentCache.All.FirstOrDefault((ControllableComponent c) => c.UniqueId == bindEntry.CrossSceneObject.UniqueId);
				}
				if (controllable == null)
				{
					continue;
				}
				if (!(value is CrossSceneAnimationTrack))
				{
					if (value is CrossSceneActivationTrack)
					{
						_director.SetGenericBinding(value, controllable.gameObject);
					}
					continue;
				}
				Animator component = controllable.GetComponent<Animator>();
				if (component != null)
				{
					_director.SetGenericBinding(value, component);
				}
			}
		}
	}
}
