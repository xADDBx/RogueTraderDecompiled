using System;
using Core.Cheats;
using Kingmaker.Sound.Base;
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

	private uint? m_PreviousAkSoundEngineState;

	private uint m_SoundId;

	private double m_AudioDuration;

	private double m_VideoDuration;

	private TimeSpan m_PlayStartTime = TimeSpan.Zero;

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
		VideoPlayer videoPlayer = m_VideoPlayer;
		if ((object)videoPlayer != null && videoPlayer.playOnAwake)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public void SetClip(VideoClip clip)
	{
		Stop();
		m_VideoClip = clip;
		m_VideoPlayer.clip = m_VideoClip;
		if (m_VideoPlayer.playOnAwake)
		{
			Play();
		}
	}

	public void Play()
	{
		Stop();
		if (TryCreateRenderTexture())
		{
			m_VideoPlayer.Play();
		}
	}

	public void Prepare()
	{
		Stop();
		if (TryCreateRenderTexture())
		{
			m_VideoPlayer.Prepare();
		}
	}

	public void Stop()
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
		StopAudio();
	}

	private bool TryCreateRenderTexture()
	{
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

	public void Update()
	{
		if (m_NeedSynchronise && (m_IsVideoLaunched || TryPlayPrepared()) && m_VideoPlayer.isPlaying && m_IsAudioPlaying && TryGetAudioPlaybackTime(out var audioTime))
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
				ResetVideoAudioSpeed();
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

	public void PrepareAudio(string soundStartEventName, string soundStopEventName)
	{
		SoundUtility.SetGenderFlags(base.gameObject);
		m_SoundStartEventName = soundStartEventName;
		m_SoundStopEventName = soundStopEventName;
		if (m_SoundStartEventName != null)
		{
			AkSoundEngine.GetState("GameAudioState", out var out_rState);
			m_PreviousAkSoundEngineState = out_rState;
			SoundState.Instance.ResetState(SoundStateType.Video);
			AkSoundEngine.PrepareEvent(AkPreparationType.Preparation_LoadAndDecode, new string[1] { soundStartEventName }, 1u, delegate
			{
				m_IsAudioPrepared = true;
			}, null);
			m_NeedSynchronise = true;
		}
	}

	public void StopAudio()
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
		if (m_PreviousAkSoundEngineState.HasValue)
		{
			AkSoundEngine.SetState("GameAudioState", m_PreviousAkSoundEngineState.ToString());
		}
		ResetVideoAudioSpeed();
		m_IsVideoLaunched = false;
		m_IsAudioPrepared = false;
		m_NeedSynchronise = false;
		m_IsAudioPlaying = false;
		m_SoundStartEventName = null;
		m_SoundStopEventName = null;
		m_PreviousAkSoundEngineState = null;
		m_AudioDuration = 0.0;
		m_VideoDuration = 0.0;
		m_SoundId = 0u;
		m_PlayStartTime = TimeSpan.Zero;
		s_ForceVideoSeekTime = null;
	}

	private bool TryPlayPrepared()
	{
		if (!m_IsAudioPrepared || !m_VideoPlayer.isPrepared)
		{
			return m_IsVideoLaunched;
		}
		m_VideoPlayer.Play();
		m_SoundId = SoundEventsManager.PostEvent(m_SoundStartEventName, base.gameObject, 1048584u, delegate(object _, AkCallbackType type, AkCallbackInfo info)
		{
			if (type == AkCallbackType.AK_Duration)
			{
				AkDurationCallbackInfo akDurationCallbackInfo = (AkDurationCallbackInfo)info;
				m_AudioDuration = TimeSpan.FromMilliseconds(akDurationCallbackInfo.fDuration).TotalSeconds;
				m_IsAudioPlaying = m_AudioDuration > 0.0;
			}
		}, null);
		ResetVideoAudioSpeed();
		m_PlayStartTime = Game.Instance.Player.RealTime;
		m_VideoDuration = m_VideoPlayer.clip.length;
		m_IsVideoLaunched = true;
		return true;
	}

	private void SetVideoPlaybackSpeed(float playbackSpeed)
	{
		m_VideoPlayer.playbackSpeed = playbackSpeed;
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

	private void ResetVideoAudioSpeed()
	{
		SetDefaultVideoPlaybackSpeed();
	}

	[Cheat(Name = "seek_video", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void CheatSeekVideo(double seekTime)
	{
		s_ForceVideoSeekTime = seekTime;
	}
}
