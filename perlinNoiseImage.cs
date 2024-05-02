using Godot;
using System;
using PerlinNoise;
namespace PerlinNoiseImage
{


	public partial class perlinNoiseImage : Node2D
	{
		public static int sizeX = 400;
		public static int sizeY = 400;
		

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{

		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta)
		{
		}

		public static void getPerlinNoiseImage()
		{
			Image image = Image.Create(sizeX, sizeY, false, Image.Format.Rgb8);

			const int GRID_SIZE = 400;


			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					int index = (y * sizeX + x) * 4;


					float val = 0;


					val += perlinNoise._perlinNoise(x , y );


					// Contrast

					// Clipping
					if (val > 1.0f)
						val = 1.0f;
					else if (val < -1.0f)
						val = -1.0f;

					// Convert 1 to -1 into 255 to 0
					int color = (int)((val + 1.0f * 0.5f) * 255);

					// Set the color
					image.SetPixel(x, y, new Color(color, color, color));

				}
			}
			image.SavePng("res://perlinNoiseImage.png");
		}
	}
}
/* Image image = Image.Create(sizeX, sizeY, false, Image.Format.Rgb8);
			for (float x = 0; x < sizeX; x += 0.01f)
			{
				for (float y = 0; y < sizeY; y += 0.01f)
				{
					float noise = perlinNoise._perlinNoise(x, y);
					image.SetPixel((int)Math.Floor(x*100), (int)Math.Floor(y*100), new Color(noise, noise, noise));
				}
			}
			image.SavePng("res://perlinNoiseImage.png"); */
