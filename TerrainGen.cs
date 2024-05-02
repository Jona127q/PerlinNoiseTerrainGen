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

	public float y;

	public int vert;
	public Vector2 uv;

	[Export]
	public bool update = false;




	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Generate_Terrain();


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

				// Laver UV data for dette punkt
				uv = new Vector2(Mathf.InverseLerp(0, xSize, x),Mathf.InverseLerp(0, zSize, z));
				st.SetUV(uv);
				
				// Tilføjer denne vertex til surfacetool
				st.AddVertex(new Vector3(x*vertDistance, y, z*vertDistance));
			}


		}

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
