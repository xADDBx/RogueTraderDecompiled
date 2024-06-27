using System;

namespace Microsoft.Cci.Pdb;

internal class PdbSource
{
	internal string name;

	internal Guid doctype;

	internal Guid language;

	internal Guid vendor;

	internal Guid checksumAlgorithm;

	internal byte[] checksum;

	internal PdbSource(string name, Guid doctype, Guid language, Guid vendor, Guid checksumAlgorithm, byte[] checksum)
	{
		this.name = name;
		this.doctype = doctype;
		this.language = language;
		this.vendor = vendor;
		this.checksumAlgorithm = checksumAlgorithm;
		this.checksum = checksum;
	}
}
