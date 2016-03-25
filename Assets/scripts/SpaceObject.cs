using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Assets.scripts;
using System.Linq;

public class SpaceObject : MonoBehaviour
{
	public Coordinate GridPosition { get; set; }
	public Vector3 Destination { get; set; }
	public SpaceObjectType TypeOfObject { get; private set;}
	public static Sprite BlueAsteroidSprite = Resources.Load("1met_blue", typeof(Sprite)) as Sprite;
	public static Sprite GreenAsteroidSprite = Resources.Load("1met_green", typeof(Sprite)) as Sprite;
	public static Sprite RedAsteroidSprite = Resources.Load("1met_red", typeof(Sprite)) as Sprite;
	public static Sprite PurpleAsteroidSprite = Resources.Load("1met_purple", typeof(Sprite)) as Sprite;
	public static Sprite YellowAsteroidSprite = Resources.Load("1met_yellow", typeof(Sprite)) as Sprite;
	public static float DropSpeed = 5f;
	public static float MoveSpeed = 2f;
	public SpaceObjectState State { get; private set; }
	public bool UpdatedField = false;
	public bool IsAsteroid()
	{
		return AsteroidTypes.Contains(TypeOfObject);
	}

	public void DestroyAsteroid()
	{
		Destroy(gameObject);
	}
	public void Initialise(int x, int y, char type)
	{
		GridPosition = new Coordinate(x,y);
		TypeOfObject = CharsToObjectTypes[type];
		gameObject.GetComponent<SpriteRenderer>().sprite = SpaceObjectTypesToSprites[TypeOfObject];
		State = SpaceObjectState.Default;
		Destination = GameField.GetVectorFromCoord(GridPosition.X, GridPosition.Y);
	}

	void Update()
	{
		
		if (Destination != gameObject.transform.position && State != SpaceObjectState.Moving)
		{
			State = SpaceObjectState.Dropping;
		}

		if (gameObject.transform.localScale.x < 1)
		{
			Vector3 scale = transform.localScale;
			scale.x += 0.025f;
			scale.y += 0.025f;
			gameObject.transform.localScale = scale;
		}
		if (State == SpaceObjectState.Moving || State == SpaceObjectState.Dropping)
		{
			if (transform.position.Equals(Destination))
			{
				State = SpaceObjectState.Default;
				if (!GameField.IsAnyMoving())
					GameField.UpdateField();		
			}
			else
			{
				float step;
				step = State == SpaceObjectState.Dropping ? DropSpeed*Time.deltaTime : MoveSpeed*Time.deltaTime;
				transform.position = Vector3.MoveTowards(transform.position, Destination, step);
			}
		}
		else if (State == SpaceObjectState.Clicked)
		{
			gameObject.GetComponentInChildren<Light>().enabled = true;
		}
		else
			gameObject.GetComponentInChildren<Light>().enabled = false;
	}
	void OnMouseDown()
	{										 
		//Debug.Log(GridPosition.X + " " + GridPosition.Y);
		if (GameField.IsAnyMoving())
			return;
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

	public void Move(Vector3 destination)
	{
		if (State == SpaceObjectState.Dropping || State == SpaceObjectState.Moving)
			return;
		State = SpaceObjectState.Moving;
		Destination = destination;
	}


	private static readonly Dictionary<char, SpaceObjectType> CharsToObjectTypes = new Dictionary<char, SpaceObjectType>
	{
		{ 'G', SpaceObjectType.GreenAsteroid},
		{ 'R', SpaceObjectType.RedAsteroid},
		{ 'B', SpaceObjectType.BlueAsteroid},
		{ 'P', SpaceObjectType.PurpleAsteroid},
		{ 'Y', SpaceObjectType.YellowAsteroid},
		{ 'H', SpaceObjectType.BlackHole},
	};
	private static readonly Dictionary<SpaceObjectType, Sprite> SpaceObjectTypesToSprites = new Dictionary<SpaceObjectType, Sprite>
	{
		{SpaceObjectType.GreenAsteroid, GreenAsteroidSprite},
		{SpaceObjectType.RedAsteroid, RedAsteroidSprite},
		{SpaceObjectType.BlueAsteroid, BlueAsteroidSprite},
		{SpaceObjectType.PurpleAsteroid, PurpleAsteroidSprite},
		{SpaceObjectType.YellowAsteroid, YellowAsteroidSprite},
	};

	private static readonly List<SpaceObjectType> AsteroidTypes = new List<SpaceObjectType>()
	{
		SpaceObjectType.GreenAsteroid,
		SpaceObjectType.RedAsteroid,
		SpaceObjectType.YellowAsteroid,
		SpaceObjectType.BlueAsteroid,
		SpaceObjectType.PurpleAsteroid
	};

}
