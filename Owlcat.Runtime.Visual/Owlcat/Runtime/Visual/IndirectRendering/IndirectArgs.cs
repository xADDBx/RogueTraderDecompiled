namespace Owlcat.Runtime.Visual.IndirectRendering;

public class IndirectArgs
{
	public class IndirectArgsEntry
	{
		private uint[] m_SharedArray;

		private int m_BaseIndex;

		public uint IndexCountPerInstance
		{
			get
			{
				return m_SharedArray[m_BaseIndex];
			}
			set
			{
				m_SharedArray[m_BaseIndex] = value;
			}
		}

		public uint InstanceCount
		{
			get
			{
				return m_SharedArray[m_BaseIndex + 1];
			}
			set
			{
				m_SharedArray[m_BaseIndex + 1] = value;
			}
		}

		public uint StartIndex
		{
			get
			{
				return m_SharedArray[m_BaseIndex + 2];
			}
			set
			{
				m_SharedArray[m_BaseIndex + 2] = value;
			}
		}

		public uint BaseVertex
		{
			get
			{
				return m_SharedArray[m_BaseIndex + 3];
			}
			set
			{
				m_SharedArray[m_BaseIndex + 3] = value;
			}
		}

		public uint StartInstance
		{
			get
			{
				return m_SharedArray[m_BaseIndex + 4];
			}
			set
			{
				m_SharedArray[m_BaseIndex + 4] = value;
			}
		}

		internal IndirectArgsEntry(uint[] sharedArray, int baseIndex)
		{
			m_SharedArray = sharedArray;
			m_BaseIndex = baseIndex;
		}
	}

	public uint[] Args { get; private set; }

	public IndirectArgsEntry[] Entries { get; private set; }

	public IndirectArgsEntry this[int index] => Entries[index];

	public IndirectArgs(int entriesCount)
	{
		Args = new uint[5 * entriesCount];
		Entries = new IndirectArgsEntry[entriesCount];
		for (int i = 0; i < entriesCount; i++)
		{
			Entries[i] = new IndirectArgsEntry(Args, i * 5);
		}
	}
}
