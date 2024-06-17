using System;

namespace Kingmaker.Sound;

[Serializable]
public abstract class AkReferenceBase
{
	public string Value;

	private uint? m_ValueHash;

	public uint ValueHash
	{
		get
		{
			if (string.IsNullOrEmpty(Value))
			{
				m_ValueHash = null;
				return 0u;
			}
			uint? num = (m_ValueHash = m_ValueHash ?? AkSoundEngine.GetIDFromString(Value));
			return num.Value;
		}
	}
}
