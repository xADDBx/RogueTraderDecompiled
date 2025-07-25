using System;
using Core.Cheats;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.Visual.Sound;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Kingmaker.Utility;

[RequireComponent(typeof(RectTransform))]
public class VideoPlayerHelper : MonoBehaviour
{
	private const uint PlayAudioCallbackFlags = 1048584u;

	private const float DesyncDelta = 0f;

	private const float MaxAudioGapToSpeedUp = 0.2f;

	private const float MaxVideoGapToSpeedUp = 2f;

	private const float DefaultPlaybackSpeed = 1f;

	private static double? s_ForceVideoSeekTime = 0.0;

	[SerializeField]
	private VideoClip m_VideoClip;

	[SerializeField]
	private bool m_PlayOnAwake;

	[SerializeField]
	private bool m_IsLooping;

	[SerializeField]
	private VideoAspectRatio m_AspectRatio;

	private RawImage m_RawImage;

	private VideoPlayer m_VideoPlayer;

	private RenderTexture m_RenderTexture;

	private bool m_Initialized;

	private bool m_IsVideoLaunched;

	private bool m_IsAudioPrepared;

	private bool m_NeedSynchronise;

	private bool m_IsAudioPlaying;

	private string m_SoundStartEventName;

	private string m_SoundStopEventName;

	private uint m_SoundId;

	private double m_AudioDuration;

	private double m_VideoDuration;

	private TimeSpan m_PlayStartTime = TimeSpan.Zero;

	private bool m_Play;

	private bool m_HasAudio;

	private bool m_IsAudioLaunched;

	private SoundStateType m_SoundStateType;

	private SoundStateType? m_PreviousSoundStateType;

	public bool IsPlaying
	{
		get
		{
			if ((bool)m_VideoPlayer && m_VideoPlayer.isPlaying)
			{
				if (!m_IsAudioPlaying)
				{
					return m_SoundId == 0;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsOvertime
	{
		get
		{
			if (m_VideoPlayer == null || m_PlayStartTime == TimeSpan.Zero)
			{
				return false;
			}
			double totalSeconds = (Game.Instance.Player.RealTime - m_PlayStartTime).TotalSeconds;
			if (m_AudioDuration > 0.0 && totalSeconds > m_AudioDuration)
			{
				return true;
			}
			if (m_VideoDuration > 0.0)
			{
				return totalSeconds > m_VideoDuration;
			}
			return false;
		}
	}

	public VideoClip VideoClip => m_VideoClip;

	public VideoPlayer VideoPlayer => m_VideoPlayer;

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (m_Initialized)
		{
			return;
		}
		m_RawImage = base.gameObject.AddComponent<RawImage>();
		m_VideoPlayer = base.gameObject.AddComponent<VideoPlayer>();
		if (m_VideoPlayer == null)
		{
			PFLog.System.Error("[VideoPlayerHelper]: Failed to create VideoPlayer component");
			return;
		}
		m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
		m_VideoPlayer.clip = m_VideoClip;
		m_VideoPlayer.playOnAwake = m_PlayOnAwake;
		m_VideoPlayer.isLooping = m_IsLooping;
		m_VideoPlayer.aspectRatio = m_AspectRatio;
		if (m_VideoPlayer.playOnAwake)
		{
			Play();
		}
		m_Initialized = true;
	}

	private void OnEnable()
	{
		if (m_VideoPlayer != null && m_VideoPlayer.playOnAwake)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public void SetClip(VideoClip clip, SoundStateType soundStateType, bool prepareVideo, string soundStartEventName, string soundStopEventName)
	{
		ResetValues();
		m_VideoClip = clip;
		m_VideoPlayer.clip = m_VideoClip;
		m_SoundStartEventName = (soundStartEventName.IsNullOrEmpty() ? null : soundStartEventName);
		m_SoundStopEventName = (soundStopEventName.IsNullOrEmpty() ? null : soundStopEventName);
		m_HasAudio = m_SoundStartEventName != null;
		m_SoundStateType = soundStateType;
		if (prepareVideo)
		{
			Prepare();
		}
		if (m_VideoPlayer.playOnAwake)
		{
			Play();
		}
	}

	public void Play()
	{
		ResetValues();
		if (TryCreateRenderTexture())
		{
			m_Play = true;
		}
	}

	public void Prepare()
	{
		if (TryCreateRenderTexture())
		{
			m_VideoPlayer.Prepare();
		}
	}

	public void Stop()
	{
		m_VideoPlayer.Stop();
		ResetValues();
	}

	private void ResetValues()
	{
		m_Play = false;
		ReleaseRenderTexture();
		StopAudio();
		SetDefaultVideoPlaybackSpeed();
		m_IsVideoLaunched = false;
		m_IsAudioLaunched = false;
		m_IsAudioPrepared = false;
		m_NeedSynchronise = false;
		m_IsAudioPlaying = false;
		m_PreviousSoundStateType = null;
		m_AudioDuration = 0.0;
		m_VideoDuration = 0.0;
		m_SoundId = 0u;
		m_PlayStartTime = TimeSpan.Zero;
		s_ForceVideoSeekTime = null;
	}

	private void ReleaseRenderTexture()
	{
		if (m_RawImage != null)
		{
			m_RawImage.texture = null;
		}
		if (m_VideoPlayer != null)
		{
			m_VideoPlayer.targetTexture = null;
		}
		if (m_RenderTexture != null)
		{
			m_RenderTexture.Release();
			m_RenderTexture = null;
		}
	}

	private bool TryCreateRenderTexture()
	{
		ReleaseRenderTexture();
		VideoClip clip = m_VideoPlayer.clip;
		if (clip == null)
		{
			return false;
		}
		m_RenderTexture = new RenderTexture((int)clip.width, (int)clip.height, 0, RenderTextureFormat.ARGB32);
		RenderTexture active = RenderTexture.active;
		RenderTexture.active = m_RenderTexture;
		GL.Clear(clearDepth: true, clearColor: true, Color.black);
		RenderTexture.active = active;
		if (!m_RenderTexture.Create())
		{
			PFLog.System.Error("[VideoPlayerHelper]: Failed to create RenderTexture");
			return false;
		}
		m_RawImage.texture = m_RenderTexture;
		m_VideoPlayer.targetTexture = m_RenderTexture;
		return true;
	}

	private void PrepareAudio()
	{
		if (!m_Play || !m_HasAudio || m_NeedSynchronise)
		{
			return;
		}
		SoundUtility.SetGenderFlags(base.gameObject);
		if (m_SoundStartEventName != null)
		{
			m_PreviousSoundStateType = SoundState.Instance.State;
			SoundState.Instance.ResetState(m_SoundStateType);
			AkSoundEngine.PrepareEvent(AkPreparationType.Preparation_LoadAndDecode, new string[1] { m_SoundStartEventName }, 1u, delegate
			{
				m_IsAudioPrepared = true;
			}, null);
			m_NeedSynchronise = true;
		}
	}

	private void PlayVideo()
	{
		if (m_Play && !m_IsVideoLaunched && (!m_HasAudio || m_IsAudioPrepared))
		{
			m_VideoPlayer.Play();
			SetDefaultVideoPlaybackSpeed();
			m_PlayStartTime = Game.Instance.Player.RealTime;
			m_VideoDuration = m_VideoPlayer.clip.length;
			m_IsVideoLaunched = true;
		}
	}

	private void PlayAudio()
	{
		if (!m_Play || !m_VideoPlayer.isPrepared || !m_HasAudio || !m_IsAudioPrepared || m_IsAudioLaunched)
		{
			return;
		}
		m_SoundId = SoundEventsManager.PostEvent(m_SoundStartEventName, base.gameObject, 1048584u, delegate(object _, AkCallbackType type, AkCallbackInfo info)
		{
			if (type == AkCallbackType.AK_Duration)
			{
				AkDurationCallbackInfo akDurationCallbackInfo = (AkDurationCallbackInfo)info;
				m_AudioDuration = TimeSpan.FromMilliseconds(akDurationCallbackInfo.fDuration).TotalSeconds;
				m_IsAudioPlaying = m_AudioDuration > 0.0;
			}
		}, null);
		m_IsAudioLaunched = true;
	}

	private void SyncAudioVideo()
	{
		if (m_VideoPlayer.isPlaying && m_IsAudioPlaying && TryGetAudioPlaybackTime(out var audioTime))
		{
			double time = m_VideoPlayer.time;
			if (s_ForceVideoSeekTime.HasValue)
			{
				SeekVideo(time + s_ForceVideoSeekTime.Value);
				s_ForceVideoSeekTime = null;
			}
			double num = Math.Abs(time - audioTime);
			if (!(num > 0.0))
			{
				SetDefaultVideoPlaybackSpeed();
			}
			else if (time < audioTime)
			{
				SeekOrSpeedUpVideo(num, audioTime);
			}
			else
			{
				SeekOrSpeedUpAudio(num, time);
			}
		}
	}

	public void Update()
	{
		PrepareAudio();
		PlayVideo();
		PlayAudio();
		SyncAudioVideo();
	}

	private bool TryGetAudioPlaybackTime(out double audioTime)
	{
		if (AkSoundEngine.GetSourcePlayPosition(m_SoundId, out var out_puPosition) != AKRESULT.AK_Success)
		{
			audioTime = 0.0;
			return false;
		}
		audioTime = TimeSpan.FromMilliseconds(out_puPosition).TotalSeconds;
		return true;
	}

	private void StopAudio()
	{
		if (m_SoundId != 0)
		{
			if (m_SoundStopEventName != null)
			{
				SoundEventsManager.PostEvent(m_SoundStopEventName, base.gameObject);
			}
			else
			{
				SoundEventsManager.StopPlayingById(m_SoundId);
			}
		}
		if (m_PreviousSoundStateType.HasValue)
		{
			SoundState.Instance.ResetState(m_PreviousSoundStateType.Value);
		}
	}

	private void SetVideoPlaybackSpeed(float playbackSpeed)
	{
		if (m_VideoPlayer != null)
		{
			m_VideoPlayer.playbackSpeed = playbackSpeed;
		}
	}

	private void SetDefaultVideoPlaybackSpeed()
	{
		SetVideoPlaybackSpeed(1f);
	}

	private void SeekOrSpeedUpVideo(double playbackDelta, double audioTime)
	{
		if (playbackDelta > 2.0)
		{
			SeekVideo(audioTime);
			SetDefaultVideoPlaybackSpeed();
		}
		else
		{
			SetVideoPlaybackSpeed(1f + (float)playbackDelta);
		}
	}

	private void SeekVideo(double playbackTime)
	{
		m_VideoPlayer.Pause();
		m_VideoPlayer.time = playbackTime;
		m_VideoPlayer.Play();
	}

	private void SeekOrSpeedUpAudio(double playbackDelta, double videoTime)
	{
		SetDefaultVideoPlaybackSpeed();
		if (playbackDelta > 0.20000000298023224)
		{
			int in_iPosition = (int)TimeSpan.FromSeconds(videoTime).TotalMilliseconds;
			AkSoundEngine.SeekOnEvent(m_SoundStartEventName, base.gameObject, in_iPosition, in_bSeekToNearestMarker: false, m_SoundId);
		}
	}

	[Cheat(Name = "seek_video", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSeekVideo(double seekTime)
	{
		s_ForceVideoSeekTime = seekTime;
	}
}
