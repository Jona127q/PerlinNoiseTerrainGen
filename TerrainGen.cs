using Godot;
using System;
using PerlinNoise;




public partial class TerrainGen : MeshInstance3D
{
	[Export]
	public int xSize = 20;
	[Export]
	public int zSize = 20;

	[Export]
	public int GRID_SIZE = 400;

	[Export]
	public float MULTIPLIER = 1.0f;

	[Export]
	public float vertDistance = 1.0f;

	[Export]
	public float SURFACELEVEL = 50.0f;

	public float SANDLEVEL;

	public float GRASSLEVEL;

	public float ROCKLEVEL;

	public float SNOWLEVEL;

	public float MAXLEVEL;

	public float y;

	public int vert;
	public Vector2 uv;

	[Export]
	public bool update = false;




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//Generate_Terrain();
		//Camera3D camera = new Camera3D();
		//camera = GetNode<Camera3D>("Camera3D");
		//camera.position = new Vector3(xSize/2, 100, zSize/2);
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(update)
		{
			Generate_Terrain();
			update = false;
		}
	}



	public void Generate_Terrain()
	{	
		//CREATING 8k EMPTY IMAGE
		
		Image image = Image.Create(xSize, zSize, false, Image.Format.Rgb8);

		


		
		// CALCULATING VALUES FOR BIOMES BY FRACTIONS OF MAXLEVEL
		MAXLEVEL = MULTIPLIER;
		// SAND LEVEL IS 1/7 OF MAXLEVEL FROM SURFACE
		SANDLEVEL = (MAXLEVEL-SURFACELEVEL)*0.03f + SURFACELEVEL;
		// GRASS LEVEL IS 2/7 OF MAXLEVEL FROM SURFACE
		GRASSLEVEL = (MAXLEVEL-SURFACELEVEL)*0.2f + SURFACELEVEL;
		// ROCK LEVEL IS 3/7 OF MAXLEVEL FROM SURFACE
		ROCKLEVEL = (MAXLEVEL-SURFACELEVEL)*0.6f + SURFACELEVEL;
		// SNOW LEVEL IS 6/7 OF MAXLEVEL FROM SURFACE
		SNOWLEVEL = (MAXLEVEL-SURFACELEVEL)*0.8f + SURFACELEVEL;


		// PRINTING LEVELS
		GD.Print("SURFACELEVEL: ", Mathf.Round(SURFACELEVEL));
		GD.Print("SANDLEVEL: ", Mathf.Round(SANDLEVEL));
		GD.Print("GRASSLEVEL: ", Mathf.Round(GRASSLEVEL));
		GD.Print("ROCKLEVEL: ", Mathf.Round(ROCKLEVEL));
		GD.Print("SNOWLEVEL: ", Mathf.Round(SNOWLEVEL));
		GD.Print("MAXLEVEL: ", Mathf.Round(MAXLEVEL));


		


		
		// Laver Arraymesh og Surfacetool
		ArrayMesh a_mesh = new ArrayMesh();
		SurfaceTool st = new SurfaceTool();

		// Starter Surfacetool
		st.Begin(Mesh.PrimitiveType.Triangles);
		
		// For hver punkt i zSize+1
		for(int z = 0; z < zSize+1; z++)
		{

			// For hver punkt xSize+1
			for(int x = 0; x < xSize+1; x++)
			{
				// Bestemmer y-værdi ud fra Noise funktion (PT BARE BØLGER)
				y = NoiseMAGIC(x,z) * MULTIPLIER;

				// PRINT Y
				//GD.Print("Y: ", y);


				// BESTEMMER BIOME
				if (y <= SURFACELEVEL)
				{	
					// VERTEX IS WATER
					// FLOOR SURFACE VALUE
					y = SURFACELEVEL;

					// SET COLOR TO BLUE
					image.SetPixel(x, z, new Color(0.0f, 0.0f, 1.0f, 1.0f));
				}

				if (y <= SANDLEVEL && y > SURFACELEVEL)
				{
					// VERTEX IS SAND
					// SET COLOR TO YELLOW
					image.SetPixel(x, z, new Color(1.0f, 1.0f, 0.0f, 1.0f));
				}

				if (y <= GRASSLEVEL && y > SANDLEVEL)
				{
					// VERTEX IS GRASS
					// SET COLOR TO GREEN
					image.SetPixel(x, z, new Color(0.0f, 1.0f, 0.0f, 1.0f));
				}

				if (y <= ROCKLEVEL && y > GRASSLEVEL)
				{
					// VERTEX IS ROCK
					// SET COLOR TO GREY
					image.SetPixel(x, z, new Color(0.5f, 0.5f, 0.5f, 1.0f));
				}

				if (y <= SNOWLEVEL && y > ROCKLEVEL)
				{
					// VERTEX IS SNOW
					// SET COLOR TO WHITE
					image.SetPixel(x, z, new Color(1.0f, 1.0f, 1.0f, 1.0f));
				}

				if (y > SNOWLEVEL)
				{
					// VERTEX IS MOUNTAIN
					// SET COLOR TO White
					image.SetPixel(x, z, new Color(1.0f, 1.0f, 1.0f, 1.0f));
				}

				

				// Laver UV data for dette punkt
				uv = new Vector2(Mathf.InverseLerp(0, xSize, x),Mathf.InverseLerp(0, zSize, z));
				st.SetUV(uv);
				
				// Tilføjer denne vertex til surfacetool
				st.AddVertex(new Vector3(x*vertDistance, y, z*vertDistance));
			}


		}

		// GEM BILLEDE SOM PNG
		image.SavePng("res://terrainImage.png");


		// Sætter vertex til 0
		vert = 0;

		// For hver punkt i zSize
		for(int z = 0; z < zSize; z++)
		{
			// For hver punkt i xSize
			for(int x = 0; x < xSize; x++)
			{
				// Laver trekant 1 i urets rækkefølge
				st.AddIndex(vert+0);
				st.AddIndex(vert+1);
				st.AddIndex(vert + xSize + 1);

				// Laver trekant 2 i urets rækkefølge
				st.AddIndex(vert + xSize + 1);
				st.AddIndex(vert + 1);
				st.AddIndex(vert + xSize + 2);

				// Gå til næste vertex
				vert++;
			}
			vert++;
			// Gå til næste vertex

		}

		// Generer Normals ved brug af SurfaceTool
		st.GenerateNormals();

		// Commit mesh og sæt den på Meshinstance
		a_mesh = st.Commit(a_mesh);
		Mesh = a_mesh;
		ImageTexture colorTexture = new ImageTexture();
		colorTexture.SetImage(image);

		


		StandardMaterial3D material = new StandardMaterial3D();
		material.AlbedoTexture = colorTexture;

		// Laver om på material
		
		MaterialOverride = material;




	}


	public float NoiseMAGIC(float x, float y)
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
		val *= 1.2f;
		// Clipping using th function
		val = Mathf.Clamp(val, -1.0f, 1.0f);

		// map value to 0.0-1.0 manually
		val = (val + 1.0f) * 0.5f;                   

		return val;
	}


}
