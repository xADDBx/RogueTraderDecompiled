namespace Kingmaker.Networking.Platforms.User;

public static class PlatformUserUtils
{
	public static void VerticalFlip(byte[] image, int width, int height)
	{
		for (int i = 0; i < height / 2; i++)
		{
			for (int j = 0; j < width; j++)
			{
				int num = (i * width + j) * 4;
				int num2 = ((height - i - 1) * width + j) * 4;
				for (int k = 0; k < 4; k++)
				{
					ref byte reference = ref image[num + k];
					ref byte reference2 = ref image[num2 + k];
					byte b = image[num2 + k];
					byte b2 = image[num + k];
					reference = b;
					reference2 = b2;
				}
			}
		}
	}
}
