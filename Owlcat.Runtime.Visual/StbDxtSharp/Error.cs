namespace StbDxtSharp;

public static class Error
{
	public enum Value
	{
		NO_ERROR,
		ERROR_FORMAT,
		ERROR_DIMENSIONS,
		ERROR_DEST_SIZE
	}

	public static string ErrorToString(Value err)
	{
		return err switch
		{
			Value.NO_ERROR => "NoError", 
			Value.ERROR_FORMAT => "The input data must be in RGBA32 format", 
			Value.ERROR_DIMENSIONS => "The input data dimensions should be both divisable by 4", 
			Value.ERROR_DEST_SIZE => "Destination buffer too small for source texture", 
			_ => "Unknown error", 
		};
	}
}
