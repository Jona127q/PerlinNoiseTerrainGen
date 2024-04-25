using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

// Local OpenCV library


public partial class MAIN : Node3D
{

	// Declare member variables here. Examples:
	public int sizeX = 20;
	public int sizeY = 20;
	public int stepSize = 1;

	//List with all the vertecies
	public List<Vector3> vertecies = new List<Vector3>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		createVertecies();
		// Create a new mesh instance
		MeshInstance meshInstance = new MeshInstance();
		// Create a new mesh
		Mesh mesh = new Mesh();
		// Create a new array of vertices
		Godot.Collections.Array vertices = new Godot.Collections.Array();
		// Add the vertices to the array
		foreach (Vector3 vertex in vertecies)
		{
			vertices.Add(vertex);
		}
		// Set the vertices of the mesh
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, new Godot.Collections.Array { vertices });
		// Set the mesh of the mesh instance
		meshInstance.Mesh = mesh;
		// Add the mesh instance to the scene
		AddChild(meshInstance);
	}
	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}




	public int createVertecies()
	{
		for (int x = 0; x < sizeX; x++)
		{
			for (int y = 0; y < sizeY; y++)
			{
				vertecies.Add(new Vector3(x*stepSize, PERLINMAGIC(x, y, stepSize), y*stepSize));
			}
		}


		return 0;

	}
	

	public int PERLINMAGIC(int x1, int y1, int stepSize1)
	{
		//Perlin noise magic
		//return (int)(Math.Sin(x) * Math.Cos(y) * stepSize);
		return (int)(Math.Sin(x1) * Math.Cos(y1) * stepSize1);
	}






}


