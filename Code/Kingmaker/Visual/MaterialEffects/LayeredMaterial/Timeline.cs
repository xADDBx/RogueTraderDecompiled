using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class Timeline
{
	private const int kDefaultMaxPlayingTracksCount = 2;

	private const float kNextUpdateRefreshTime = float.NegativeInfinity;

	private int m_NextToken;

	private readonly List<Track> m_TracksOrderedByPriority = new List<Track>();

	private readonly List<Track> m_PlayingTracks = new List<Track>();

	private int m_MaxActiveTrackPriority;

	private int m_MaxPlayingTracksCount = 2;

	private float m_NextPlayingTracksRefreshTime;

	public void SetMaxPlayingTracksCount(int value)
	{
		if (value <= 0)
		{
			value = 2;
		}
		if (m_MaxPlayingTracksCount != value)
		{
			m_MaxPlayingTracksCount = value;
			m_NextPlayingTracksRefreshTime = float.NegativeInfinity;
		}
	}

	public Track AddTrack(int priority, float time, float duration, Material material, RendererType rendererTypeMask)
	{
		int token = m_NextToken++;
		Track track = Track.Get();
		track.token = token;
		track.material = material;
		track.rendererTypeMask = rendererTypeMask;
		track.priority = priority;
		track.beginTime = time;
		track.endTime = ((duration > 0f) ? (time + duration) : float.PositiveInfinity);
		m_TracksOrderedByPriority.Insert(GetTrackIndexToInsert(track.priority), track);
		m_NextPlayingTracksRefreshTime = float.NegativeInfinity;
		return track;
	}

	private int GetTrackIndexToInsert(int priority)
	{
		int i = 0;
		for (int count = m_TracksOrderedByPriority.Count; i < count; i++)
		{
			if (m_TracksOrderedByPriority[i].priority > priority)
			{
				return i;
			}
		}
		return m_TracksOrderedByPriority.Count;
	}

	public void RemoveTrack(int token)
	{
		for (int num = m_TracksOrderedByPriority.Count - 1; num >= 0; num--)
		{
			Track track = m_TracksOrderedByPriority[num];
			if (track.token == token)
			{
				m_TracksOrderedByPriority.RemoveAt(num);
				if (m_PlayingTracks.Contains(track))
				{
					m_NextPlayingTracksRefreshTime = float.NegativeInfinity;
				}
				track.Recycle();
				break;
			}
		}
	}

	public bool UpdatePlayingTracks(float time)
	{
		if (time < m_NextPlayingTracksRefreshTime)
		{
			return false;
		}
		m_PlayingTracks.Clear();
		float num = float.PositiveInfinity;
		for (int num2 = m_TracksOrderedByPriority.Count - 1; num2 >= 0; num2--)
		{
			Track track = m_TracksOrderedByPriority[num2];
			if (time > track.endTime)
			{
				m_TracksOrderedByPriority.RemoveAt(num2);
			}
			else if (time < track.beginTime)
			{
				num = math.min(num, track.beginTime);
			}
			else
			{
				num = math.min(num, track.endTime);
				if (m_PlayingTracks.Count < m_MaxPlayingTracksCount)
				{
					m_PlayingTracks.Add(track);
				}
			}
		}
		m_NextPlayingTracksRefreshTime = num;
		return true;
	}

	public int GetTracksCount()
	{
		return m_TracksOrderedByPriority.Count;
	}

	public void GetPlayingTracks(RendererType rendererType, List<Track> results)
	{
		foreach (Track playingTrack in m_PlayingTracks)
		{
			if ((playingTrack.rendererTypeMask & rendererType) != 0)
			{
				results.Add(playingTrack);
			}
		}
	}

	public void GetPlayingTracksMaterials(RendererType rendererType, List<Material> results)
	{
		foreach (Track playingTrack in m_PlayingTracks)
		{
			if ((playingTrack.rendererTypeMask & rendererType) != 0)
			{
				results.Add(playingTrack.material);
			}
		}
	}
}
