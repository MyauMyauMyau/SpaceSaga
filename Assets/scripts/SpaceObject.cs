using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Assets.scripts;
using System.Linq;
using Assets.scripts.Enums;
using Random = System.Random;

public class SpaceObject : MonoBehaviour
{
	public Coordinate GridPosition { get; set; }
	public Vector3 Destination { get; set; }
	public Coordinate PreviousPosition {get; set;}
	public SpaceObjectType TypeOfObject { get; set;}
	public static Sprite BlueAsteroidSprite = Resources.Load("1met_blue", typeof(Sprite)) as Sprite;
	public static Sprite GreenAsteroidSprite = Resources.Load("1met_green", typeof(Sprite)) as Sprite;
	public static Sprite RedAsteroidSprite = Resources.Load("1met_red", typeof(Sprite)) as Sprite;
	public static Sprite PurpleAsteroidSprite = Resources.Load("1met_purple", typeof(Sprite)) as Sprite;
	public static Sprite YellowAsteroidSprite = Resources.Load("1met_yellow", typeof(Sprite)) as Sprite;
	public static Sprite EmptyCellSprite = Resources.Load("EmptyCell", typeof(Sprite)) as Sprite;
	public static Sprite UnstableBlueAsteroidSprite = Resources.Load("2Nmet_blue", typeof(Sprite)) as Sprite;
	public static Sprite UnstableGreenAsteroidSprite = Resources.Load("2Nmet_green", typeof(Sprite)) as Sprite;
	public static Sprite UnstableRedAsteroidSprite = Resources.Load("2Nmet_red", typeof(Sprite)) as Sprite;
	public static Sprite UnstablePurpleAsteroidSprite = Resources.Load("2Nmet_purple", typeof(Sprite)) as Sprite;
	public static Sprite UnstableYellowAsteroidSprite = Resources.Load("2Nmet_yellow", typeof(Sprite)) as Sprite;
	public const float BaseDropSpeed = 10f;
	public float DropSpeed;
	public float MoveSpeed;
	public float GrowSpeed;
	public float StartTime = 0;
	public float Delay = 0;

	public bool IsUnstable { get; set; }
	public SpaceObjectState State { get; set; }
	public bool UpdatedField = false;
	public Random rnd = new Random();
	public bool IsAsteroid()
	{
		return AsteroidTypes.Contains(TypeOfObject);
	}

	public void DestroyAsteroid()
	{
		Destroy(gameObject);
	}
	public void Initialise(int x, int y, char type, float delay = 0, bool isUnstable = false)
	{
		DropSpeed = BaseDropSpeed;
		MoveSpeed = 5f;
		GrowSpeed = 0.05f;
		IsUnstable = isUnstable;
		GridPosition = new Coordinate(x,y);
		TypeOfObject = CharsToObjectTypes[type];
		if (isUnstable)
		{
			gameObject.GetComponent<SpriteRenderer>().sprite = StableToUnstableSprites[TypeOfObject];
		}
		else
			gameObject.GetComponent<SpriteRenderer>().sprite = SpaceObjectTypesToSprites[TypeOfObject];

		Destination = GameField.GetVectorFromCoord(GridPosition.X, GridPosition.Y);
		if (delay > 0)
		{
			Delay = delay;
			StartTime = Time.time;
			State = SpaceObjectState.WaitingForInitialising;
			return;
		}
		State = SpaceObjectState.Growing;
	}

	void Update()
	{
		if (State == SpaceObjectState.WaitingForInitialising)
		{
			if (Time.time - StartTime > Delay)
				State = SpaceObjectState.Dropping;
			else
				return;
		}


		if (Destination != gameObject.transform.position && State != SpaceObjectState.Moving)
		{
			State = SpaceObjectState.Dropping;
		}

		if (gameObject.transform.localScale.x < 1)
		{
			Vector3 scale = transform.localScale;
			if (IsUnstable)
			{
				scale.x = 1;
				scale.y = 1;
			}
			else
			{
				scale.x += GrowSpeed;
				scale.y += GrowSpeed;
			}
			gameObject.transform.localScale = scale;
			if (gameObject.transform.localScale.x < 0.90)
				return;
		}
		else if (State == SpaceObjectState.Growing)
			State = SpaceObjectState.Default;
		if (IsUnstable && rnd.Next(2) == 1)
		{
			Vector3 scale = transform.localScale;
			scale.x -= 0.05f;
			scale.y -= 0.05f;
			gameObject.transform.localScale = scale;
		}
		if (State == SpaceObjectState.Moving || State == SpaceObjectState.Dropping)
		{
			if (transform.position.Equals(Destination))
			{
				if (State == SpaceObjectState.Moving && !GameField.MoveIsFinished)
				{
					if (!GameField.IsCorrectMove(new List<Coordinate>() {GridPosition, PreviousPosition}))
						GameField.Swap(GridPosition, PreviousPosition);
					else
					{
						GameField.MoveIsFinished = true;
						Game.TurnsLeft--;
					}
				}
				State = SpaceObjectState.Default;
			}
			else
			{
				float step;
				step = State == SpaceObjectState.Dropping ? this.DropSpeed*Time.deltaTime : this.MoveSpeed*Time.deltaTime;
				transform.position = Vector3.MoveTowards(transform.position, Destination, step);
			}
		}
		else if (State == SpaceObjectState.Clicked)
		{
			gameObject.GetComponentInChildren<Light>().enabled = true;
		}
		else
			gameObject.GetComponentInChildren<Light>().enabled = false;
								   
		if (IsAsteroid() && State !=SpaceObjectState.Moving && GridPosition.Y != Game.MAP_SIZE - 1)
		{
			if (GameField.Map[GridPosition.X, GridPosition.Y + 1] == null)
			{
				GameField.Drop(GridPosition, new Coordinate(GridPosition.X, GridPosition.Y + 1));
			}
			// if downdrop is blocked
			else if (GridPosition.X > 0 &&
			         GameField.Map[GridPosition.X - 1, GridPosition.Y + 1] == null)
			{
				if ((GameField.Map[GridPosition.X - 1, GridPosition.Y] != null
				&& !GameField.Map[GridPosition.X - 1, GridPosition.Y].IsAsteroid()) ||
				(GameField.Map[GridPosition.X - 1, GridPosition.Y] == null && GridPosition.Y > 0 
				&& GameField.Map[GridPosition.X - 1, GridPosition.Y - 1] != null && !GameField.Map[GridPosition.X - 1, GridPosition.Y - 1].IsAsteroid()))
					GameField.Drop(GridPosition, new Coordinate(GridPosition.X - 1, GridPosition.Y + 1));
			}
			else if (GridPosition.X < GameField.Map.GetLength(0) - 1 &&
					 GameField.Map[GridPosition.X + 1, GridPosition.Y + 1] == null)
			{
				if ((GameField.Map[GridPosition.X + 1, GridPosition.Y] != null
				&& !GameField.Map[GridPosition.X + 1, GridPosition.Y].IsAsteroid()) ||
				(GameField.Map[GridPosition.X + 1, GridPosition.Y] == null && GridPosition.Y > 0
				&& GameField.Map[GridPosition.X + 1, GridPosition.Y - 1] != null && !GameField.Map[GridPosition.X + 1, GridPosition.Y - 1].IsAsteroid()))
					GameField.Drop(GridPosition, new Coordinate(GridPosition.X + 1, GridPosition.Y + 1));
			}
		}

	}
	void OnMouseDown()
	{										 
		if (GameField.IsAnyMoving() || !IsAsteroid())
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

	public void Move(Coordinate newPosition)
	{
		State = SpaceObjectState.Moving;
		PreviousPosition = GridPosition;
		GridPosition = newPosition;
		Destination = GameField.GetVectorFromCoord(newPosition.X, newPosition.Y);
	}


	public static readonly Dictionary<char, SpaceObjectType> CharsToObjectTypes = new Dictionary<char, SpaceObjectType>
	{
		{ 'G', SpaceObjectType.GreenAsteroid},
		{ 'R', SpaceObjectType.RedAsteroid},
		{ 'B', SpaceObjectType.BlueAsteroid},
		{ 'P', SpaceObjectType.PurpleAsteroid},
		{ 'Y', SpaceObjectType.YellowAsteroid},
		{ 'H', SpaceObjectType.BlackHole},
		{ 'E', SpaceObjectType.EmptyCell }
	};
	private static readonly Dictionary<SpaceObjectType, Sprite> SpaceObjectTypesToSprites = new Dictionary<SpaceObjectType, Sprite>
	{
		{SpaceObjectType.GreenAsteroid, GreenAsteroidSprite},
		{SpaceObjectType.RedAsteroid, RedAsteroidSprite},
		{SpaceObjectType.BlueAsteroid, BlueAsteroidSprite},
		{SpaceObjectType.PurpleAsteroid, PurpleAsteroidSprite},
		{SpaceObjectType.YellowAsteroid, YellowAsteroidSprite},
		{SpaceObjectType.EmptyCell, EmptyCellSprite}
	};

	private static readonly Dictionary<SpaceObjectType, Sprite> StableToUnstableSprites = new Dictionary<SpaceObjectType, Sprite>
	{
		{SpaceObjectType.GreenAsteroid, UnstableGreenAsteroidSprite},
		{SpaceObjectType.RedAsteroid, UnstableRedAsteroidSprite},
		{SpaceObjectType.BlueAsteroid, UnstableBlueAsteroidSprite},
		{SpaceObjectType.PurpleAsteroid, UnstablePurpleAsteroidSprite},
		{SpaceObjectType.YellowAsteroid, UnstableYellowAsteroidSprite},
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
