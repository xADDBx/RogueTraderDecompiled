using System;
using System.Collections.Generic;

namespace Microsoft.Cci.Pdb;

internal class PdbInfo
{
	public PdbFunction[] Functions;

	public Dictionary<uint, PdbTokenLine> TokenToSourceMapping;

	public string SourceServerData;

	public int Age;

	public Guid Guid;

	public byte[] SourceLinkData;
}
