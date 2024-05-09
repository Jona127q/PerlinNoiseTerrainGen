using Godot;
using System;

public partial class MAINSCENE : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MeshInstance3D terrain = GetNode<MeshInstance3D>("TerrainGen");
		terrain.Call("Generate_Terrain");

		GD.Print("zSize: "+(float)terrain.Get("zSize")+ " MULTIPLIER: "+(float)terrain.Get("MULTIPLIER") + " xSize: "+(float)(terrain.Get("xSize")));

		Camera3D camera = GetNode<Camera3D>("Camera");
		camera.Call("Set_Camera_Position", new Vector3((float)terrain.Get("zSize"), (float)terrain.Get("MULTIPLIER") , (float)(terrain.Get("xSize"))));

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
