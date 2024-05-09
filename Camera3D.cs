using Godot;
using System;

//SCRIP TO MOVE CAMERA AROUND FLYING

public partial class Camera3D : Godot.Camera3D
{

	public float x = 0;
	public float y = 0;
	public float z = 0;

	private const float RotationSpeed = -0.02f;

	private Vector2 _lastMousePosition;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_lastMousePosition = GetViewport().GetMousePosition();
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Move the camera with the arrow keys
		Vector3 direction = new Vector3();
		if (Input.IsActionPressed("ui_right"))
		{
			x = x + 1;
		}
		if (Input.IsActionPressed("ui_left"))
		{
			x = x - 1;
		}
		if (Input.IsActionPressed("ui_up"))
		{
			z = z - 1;
		}
		if (Input.IsActionPressed("ui_down"))
		{
			z = z + 1;
		}



		//move with wasd
		if (Input.IsActionPressed("ui_w"))
		{
			z = z - 1;
		}
		if (Input.IsActionPressed("ui_s"))
		{
			z = z + 1;
		}
		if (Input.IsActionPressed("ui_a"))
		{
			x = x - 1;
		}
		if (Input.IsActionPressed("ui_d"))
		{
			x = x + 1;
		}

		GD.Print("x : " + Position.X + " : y :  " + Position.Y + " : z : " +  Position.Z);
		// Move the camera
		Translate(new Vector3(x, y, z));

		// Reset the movement VARIABLES
		x = 0;
		y = 0;
		z = 0;


		// Rotate the camera with the mouse
		Vector2 currentMousePosition = GetViewport().GetMousePosition();
		Vector2 deltaPosition = currentMousePosition - _lastMousePosition;
		_lastMousePosition = currentMousePosition;
	

		//Horizontal rotation 
		RotateY(deltaPosition[0] * RotationSpeed);
		

		// Vertical rotation
		RotateX(deltaPosition[1] * RotationSpeed);


	}
	public void setPosition(Vector3 pos)
	{
		setPosition(pos);
	}
}
