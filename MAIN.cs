using Godot;
using System;
using System.Collections.Generic;

public partial class MAIN : Node3D
{
    public int sizeX = 20;
    public int sizeY = 20;
    public int stepSize = 1;
    public List<Vector3> vertices = new List<Vector3>();

    public override void _Ready()
    {
        CreateVertices();
		GD.Print(vertices.Count+" Vertices created:");
		foreach (Vector3 vertex in vertices){
			GD.Print(vertex);
		}

        // Access the existing MeshInstance3D node from the scene
        MeshInstance3D meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
		GD.Print("MeshInstance3D node found.");
        
        // Create a new array of vertices
        Godot.Collections.Array meshVertices = new Godot.Collections.Array();
        foreach (Vector3 vertex in vertices)
        {
            meshVertices.Add(vertex);
			//GD.Print(vertex+ " added to meshVertices");
        }
        
        // Create a new ArrayMesh and set the vertex array
        ArrayMesh arrayMesh = new ArrayMesh();
		GD.Print("ArrayMesh created.");
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, new Godot.Collections.Array { meshVertices });
		GD.Print("Surface added to arrayMesh");

        // Set the mesh of the MeshInstance3D
        meshInstance.Mesh = arrayMesh;
		GD.Print("Mesh set to MeshInstance3D");

        // Create a new SpatialMaterial and assign it to the MeshInstance3D
        StandardMaterial3D  material = new StandardMaterial3D();
		GD.Print("StandardMaterial3D created.");

        material.AlbedoColor = new Color(1, 0, 0); // Set the material color to red
        GD.Print("Material color set to red.");
		
		meshInstance.MaterialOverride = material; // Assign the material to the MeshInstance3D
		GD.Print("Material assigned to MeshInstance3D.");

    }

    public override void _Process(double delta)
    {
        // Process logic here
    }

    public void CreateVertices()
    {
        for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				vertices.Add(new Vector3(x * stepSize, PERLINMAGIC(x,y), y * stepSize));
			}
		}

	
	}

	public float PERLINMAGIC(int x, int y)
	{
		//DO MAGIC HERE
		return (float)(Math.Sin(x) + Math.Cos(y));
	}



}