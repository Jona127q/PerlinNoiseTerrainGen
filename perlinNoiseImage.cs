using Godot;
using System;
using PerlinNoise;
using System.Data;

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
		{ // need high bitdepth

			Image image = Image.Create(sizeX, sizeY, false, Image.Format.Rgb8);

			const int GRID_SIZE = 400;


			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{


					float val = 0;

					float frequency = 1f;
					float amplitude = 1f;


		  			for (int i = 0; i < 16; i++)
		  			{
		  			val += perlinNoise._perlinNoise(x * frequency / GRID_SIZE, y * frequency / GRID_SIZE) * amplitude;
 
		  			frequency *= 2;
		  			amplitude /= 2;
					}



					// Contrast
					// map value to 0.0-1.0 manually
					val = (val + 1.0f) * 0.5f;					

					
					GD.Print(" : x : " + (float)x + " : y : " + (float)y  + " : val : " + (float)val);
					// Set the color
					image.SetPixel(x, y, new Color(val, val, val, 1.0f));

				}
			}
			image.SavePng("res://perlinNoiseImage.png");
		}

		public static void perlinNoiseTest(){

			for (int x = 0; x < sizeX; x++)
			{
				for (int y = 0; y < sizeY; y++)
				{
					int index = (y * sizeX + x) * 4;


					float val = 0;



					val += perlinNoise._perlinNoise(x + 0.5f, y + 0.5f);




					// Contrast

					// Clipping

					

					// Convert 1 to -1 into 255 to 0
					GD.Print(" : x : " + (float)x + " : y : " + (float)y  + " : val : " + (float)val);

				}
			}
		}
	}
}
