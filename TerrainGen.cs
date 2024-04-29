using Godot;
using System;

public partial class TerrainGen : MeshInstance3D
{
	[Export]
	public int xSize = 20;
	[Export]
	public int zSize = 20;
	[Export]
	public float MULTIPLIER = 5.0f;

	public float y;

	public int vert;

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
		ArrayMesh a_mesh = new ArrayMesh();
		SurfaceTool st = new SurfaceTool();

		st.Begin(Mesh.PrimitiveType.Triangles);
		
		for(int z = 0; z < zSize+1; z++)
		{
			for(int x = 0; x < xSize+1; x++)
			{
				y = NoiseMAGIC(x,z) * MULTIPLIER;

				st.AddVertex(new Vector3(x, y, z));
			}


		}

		
		vert = 0;

		for(int z = 0; z < zSize; z++)
		{
			for(int x = 0; x < xSize; x++)
			{
				st.AddIndex(vert+0);
				st.AddIndex(vert+1);
				st.AddIndex(vert + xSize + 1);

				st.AddIndex(vert + xSize + 1);
				st.AddIndex(vert + 1);
				st.AddIndex(vert + xSize + 2);

				vert++;
			}
			vert++;
		}

		st.GenerateNormals();

		a_mesh = st.Commit(a_mesh);
		Mesh = a_mesh;



	}

	public float NoiseMAGIC(float x, float z)
	{
		float y = (float)(Math.Sin(x) + Math.Cos(z));
		return y;
	}


}
