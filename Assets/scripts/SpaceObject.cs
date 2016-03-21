using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Assets.scripts;

public class SpaceObject : MonoBehaviour
{
	public Coordinate GridPosition { get; set; }
	public SpaceObjectType TypeOfObject { get; private set;}
	public static Material BlueAsteroidMaterial = Resources.Load("BlueAsteroidMaterial", typeof(Material)) as Material;
	public static Material GreenAsteroidMaterial = Resources.Load("GreenAsteroidMaterial", typeof(Material)) as Material;
	public static Material RedAsteroidMaterial = Resources.Load("RedAsteroidMaterial", typeof(Material)) as Material;
	public static Material VioletAsteroidMaterial = Resources.Load("VioletAsteroidMaterial", typeof(Material)) as Material;
	public static Material YellowAsteroidMaterial = Resources.Load("YellowAsteroidMaterial", typeof(Material)) as Material;
	public SpaceObjectState State { get; private set; }
	public void Initialise(int x, int y, char type)
	{
		GridPosition = new Coordinate(x,y);
		TypeOfObject = CharsToObjectTypes[type];
		gameObject.GetComponent<Renderer>().material = SpaceObjectTypesToMaterials[TypeOfObject];
		State = SpaceObjectState.Default;
	}

	void Update()
	{
		if (State == SpaceObjectState.Clicked)
		{
			gameObject.GetComponentInChildren<Light>().enabled = true;
		}
		else
			gameObject.GetComponentInChildren<Light>().enabled = false;
	}
	void OnMouseDown()
	{
		Debug.Log(GridPosition.X + " " + GridPosition.Y);
		if (State == SpaceObjectState.Default)
		{
			if (GameField.ClickedObject == null)
			{
				GameField.ClickedObject = GridPosition;
				State = SpaceObjectState.Clicked;
			}
			else
			{
				if (GridPosition.IsNeighbourWith(GameField.ClickedObject.Value))
				{
					GameField.Map[GameField.ClickedObject.Value.X, GameField.ClickedObject.Value.Y].State = SpaceObjectState.Default;
					State = SpaceObjectState.Default;
					GameField.Swap(GridPosition, GameField.ClickedObject.Value);
				}
				else
				{
					GameField.Map[GameField.ClickedObject.Value.X, GameField.ClickedObject.Value.Y].State = SpaceObjectState.Default;
					GameField.ClickedObject = null;
					State = SpaceObjectState.Default;
				}
			}
		}
		else if (State == SpaceObjectState.Clicked)
		{
			State = SpaceObjectState.Default;
			GameField.ClickedObject = null;
		}
	}

	private static readonly Dictionary<char, SpaceObjectType> CharsToObjectTypes = new Dictionary<char, SpaceObjectType>
	{
		{ 'G', SpaceObjectType.GreenAsteroid},
		{ 'R', SpaceObjectType.RedAsteroid},
		{ 'B', SpaceObjectType.BlueAsteroid},
		{ 'V', SpaceObjectType.VioletAsteroid},
		{ 'Y', SpaceObjectType.YellowAsteroid},
		{ 'H', SpaceObjectType.BlackHole},
	};
	private static readonly Dictionary<SpaceObjectType, Material> SpaceObjectTypesToMaterials = new Dictionary<SpaceObjectType,Material>
	{
		{SpaceObjectType.GreenAsteroid, GreenAsteroidMaterial},
		{SpaceObjectType.RedAsteroid, RedAsteroidMaterial},
		{SpaceObjectType.BlueAsteroid, BlueAsteroidMaterial},
		{SpaceObjectType.VioletAsteroid, VioletAsteroidMaterial},
		{SpaceObjectType.YellowAsteroid, YellowAsteroidMaterial},
	};

}
