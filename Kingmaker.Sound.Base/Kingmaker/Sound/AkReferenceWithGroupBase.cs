using System;

namespace Kingmaker.Sound;

[Serializable]
public abstract class AkReferenceWithGroupBase : AkReferenceBase
{
	public string Group;

	private uint? m_GroupHash;

	public uint GroupHash
	{
		get
		{
			if (string.IsNullOrEmpty(Group))
			{
				m_GroupHash = null;
				return 0u;
			}
			uint? num = (m_GroupHash = m_GroupHash ?? AkSoundEngine.GetIDFromString(Group));
			return num.Value;
		}
	}
}
