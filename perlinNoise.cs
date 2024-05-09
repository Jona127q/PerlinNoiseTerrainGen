using Godot;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
namespace PerlinNoise
{
	public partial class perlinNoise : Node3D{
		public static uint[] seeds = new uint[] {
			3284157443,
			1911520717,
			2048419325
		};


		// Called when the node enters the scene tree for the first time.
		public override void _Ready(){
			newSeed("Mount Godot");
		}

		// Called every frame. 'delta' is the elapsed time since the previous frame.
		public override void _Process(double delta){
		}



		public static float _perlinNoise(float x, float y){

			int x0 = (int)x;
			int y0 = (int)y;

			int x1 = x0 + 1;
			int y1 = y0 + 1;

			float interpolationWheightX = x - x0;
			float interpolationWheightY = y - y0;

			float dotProductTopLeft = getDotProduct(x0, y0, x, y);
			float dotProductTopRight = getDotProduct(x1, y0, x, y);

			float topInterpolated = interpolate(dotProductTopLeft, dotProductTopRight, interpolationWheightX);


			float dotProductBottomLeft = getDotProduct(x0, y1, x, y);
			float dotProductBottomRight = getDotProduct(x1, y1, x, y);

			float bottomInterpolated = interpolate(dotProductBottomLeft, dotProductBottomRight, interpolationWheightX);

			return (float)interpolate(topInterpolated, bottomInterpolated, interpolationWheightY);

		}


		public static float interpolate(float point0, float point1, float wheight){
			return (point1 - point0) * (3.0f - wheight * 2.0f) * wheight * wheight + point0;

		}

		public static float getDotProduct(int ix, int iy, float x, float y){


			Vector2 gradientVector = randomGradient(ix, iy);

			Vector2 distanceVector = new Vector2(x - ix, y - iy);

			Vector2 gradientVector2 = new Vector2(gradientVector.X, gradientVector.Y);
			Vector2 distanceVector2 = new Vector2(distanceVector.X, distanceVector.Y);

			float dotProduct = gradientVector2.Dot(distanceVector2); //shit ass syntax


			return dotProduct;
		}


		public static Vector2 randomGradient(int ix, int iy){

			// No precomputed gradients mean this works for any number of grid coordinates
			const int w = 8 * sizeof(uint);
			const int s = w / 2;
			uint a = (uint)ix, b = (uint)iy;
			//a *= 3284157443;
			a *= seeds[0];

			b ^= a << s | a >> w - s;
			//b *= 1911520717;
			b *= seeds[1];

			a ^= b << s | b >> w - s;
			//a *= 2048419325;
			a *= seeds[2];

			float random = a * (3.14159265f / ~(~0u >> 1)); // in [0, 2*Pi]

			// Create the vector from the angle

			Vector2 gradientVector = new Vector2((float)Math.Sin(random), (float)Math.Cos(random));


			return gradientVector;
		}
		public static void newSeed(string seed){
			using (MD5 md5 = MD5.Create()){
				byte[] inputBytes = Encoding.UTF8.GetBytes(seed);
				byte[] hashBytes = md5.ComputeHash(inputBytes);
				string hashString = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 11);

				if (hashString.Length < 35){
					hashString += hashString;
				}
				else if (hashString.Length > 35){
					hashString = hashString.Substring(0, 35);
				}
				uint[] newSeeds = new uint[3];

				for (int i = 0; i < 3; i++)
				{
					int startIndex = i * 4;
					string chunk = hashString.Substring(startIndex, 4);
					newSeeds[i] = Convert.ToUInt32(chunk, 16);
				

					seeds = newSeeds;
					
				}
				return;
			}
		}
	}
}
