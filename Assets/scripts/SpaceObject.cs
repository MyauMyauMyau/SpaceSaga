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
	private Coordinate blackHoleTarget;
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
	public static Sprite BlackHoleSprite = Resources.Load("3Black_hole_01", typeof(Sprite)) as Sprite;
	public static Sprite IceSprite = Resources.Load("3Space_ice", typeof(Sprite)) as Sprite;
	public const float BaseDropSpeed = 10f;
	public bool IsTargetForBlackHole;
	public bool IsFrozen;
	public float DropSpeed;
	public float MoveSpeed;
	public float GrowSpeed;
	public float StartTime = 0;
	public float Delay = 0;
	private bool blackHoleHasJumped;
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
		var bottomBoundX = Math.Max(0, GridPosition.X - 1);
		var bottomBoundY = Math.Max(0, GridPosition.Y - 1);
		var topBoundX = Math.Min(Game.MAP_SIZE - 1, GridPosition.X + 1);
		var topBoundY = Math.Min(Game.MAP_SIZE - 1, GridPosition.Y + 1);
		if (TypeOfObject == SpaceObjectType.Ice)
		{
			for (int i = bottomBoundX; i <= topBoundX; i++)
			{
				for (int j = bottomBoundY; j <= topBoundY; j++)
				{
					if (GameField.Map[i, j] != null && GameField.Map[i, j].IsAsteroid())
					{
						GameField.Map[i, j].IsFrozen = true;
						GameField.Map[i, j].gameObject.transform.GetChild(1).gameObject.SetActive(true);
					}
				}
			}
			
			Destroy(gameObject);
			return;
		}
		if (IsFrozen)
		{
			IsFrozen = false;
			gameObject.transform.GetChild(1).gameObject.SetActive(false);
		}
		else
		{
			var iceList = new List<Coordinate>();
			for (int i = bottomBoundX; i <= topBoundX; i++)
			{
				if (GameField.Map[i, GridPosition.Y] != null &&
					GameField.Map[i, GridPosition.Y].TypeOfObject == SpaceObjectType.Ice)
				{
					iceList.Add(new Coordinate(i, GridPosition.Y));
				}
			}
			for (int j = bottomBoundY; j <= topBoundY; j++)
			{
				if (GameField.Map[GridPosition.X, j] != null &&
					GameField.Map[GridPosition.X, j].TypeOfObject == SpaceObjectType.Ice)
				{
					iceList.Add(new Coordinate(GridPosition.X,j));
				}
			}
			Destroy(gameObject);
			foreach (var ice in iceList)
			{
				GameField.Map[ice.X, ice.Y].DestroyAsteroid();				
			}
		}
	}
	public void Initialise(int x, int y, char type, float delay = 0, bool isUnstable = false)
	{
		DropSpeed = BaseDropSpeed*2;
		MoveSpeed = 10f;
		GrowSpeed = 0.05f;
		IsUnstable = isUnstable;
		GridPosition = new Coordinate(x,y);
		TypeOfObject = CharsToObjectTypes[type];
		if (TypeOfObject == SpaceObjectType.BlackHole)
		{
			var light = gameObject.GetComponentInChildren<Light>();
			light.enabled = true;
			light.range = 1.33f;
			light.color = Color.blue;
		}
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

		if (gameObject.transform.localScale.x < 1 && State != SpaceObjectState.Decreasing)
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
		if (TypeOfObject == SpaceObjectType.BlackHole)
		{
			HandleBlackHole();
		}
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
						Game.instance.Update();
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
		if (IsAsteroid())
			gameObject.GetComponentInChildren<Light>().enabled = State == SpaceObjectState.Clicked;
								   
		if (IsAsteroid() && !IsFrozen && State !=SpaceObjectState.Moving && GridPosition.Y != Game.MAP_SIZE - 1)
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
				&& (!GameField.Map[GridPosition.X - 1, GridPosition.Y].IsAsteroid() || GameField.Map[GridPosition.X - 1, GridPosition.Y].IsFrozen)) ||
				(GameField.Map[GridPosition.X - 1, GridPosition.Y] == null && GridPosition.Y > 0 
				&& GameField.Map[GridPosition.X - 1, GridPosition.Y - 1] != null
				&& (!GameField.Map[GridPosition.X - 1, GridPosition.Y - 1].IsAsteroid() || GameField.Map[GridPosition.X - 1, GridPosition.Y - 1].IsFrozen)))
					GameField.Drop(GridPosition, new Coordinate(GridPosition.X - 1, GridPosition.Y + 1));
			}
			else if (GridPosition.X < GameField.Map.GetLength(0) - 1 &&
					 GameField.Map[GridPosition.X + 1, GridPosition.Y + 1] == null)
			{
				if ((GameField.Map[GridPosition.X + 1, GridPosition.Y] != null
				&& (!GameField.Map[GridPosition.X + 1, GridPosition.Y].IsAsteroid() || GameField.Map[GridPosition.X + 1, GridPosition.Y].IsFrozen)) ||
				(GameField.Map[GridPosition.X + 1, GridPosition.Y] == null && GridPosition.Y > 0
				&& GameField.Map[GridPosition.X + 1, GridPosition.Y - 1] != null && 
				(!GameField.Map[GridPosition.X + 1, GridPosition.Y - 1].IsAsteroid() || GameField.Map[GridPosition.X + 1, GridPosition.Y - 1].IsFrozen)))
					GameField.Drop(GridPosition, new Coordinate(GridPosition.X + 1, GridPosition.Y + 1));
			}
		}

	}

	private void HandleBlackHole()
	{
		if (State == SpaceObjectState.Decreasing)
		{
			Vector3 scale = transform.localScale;
			scale.x -= GrowSpeed/1.5f;
			scale.y -= GrowSpeed/1.5f;
			if (scale.x <= 0.1)
			{
				State = SpaceObjectState.Growing;
				GameField.Map[blackHoleTarget.X, blackHoleTarget.Y].IsTargetForBlackHole = false;
				GameField.Jump(GridPosition, blackHoleTarget);
			}
			transform.localScale = scale;
		}
		transform.Rotate(0f, 0f, -100f*Time.deltaTime);
		if (Game.TurnsLeft%4 == 0) 
		{
			if (!blackHoleHasJumped && !GameField.IsAnyMoving())
				JumpBlackHole();
		}
		else
			blackHoleHasJumped = false;
	}

	private void JumpBlackHole()
	{
		blackHoleHasJumped = true;
		var map = GameField.Map;
		var unsuitableAsteroids = new List<SpaceObjectType>();
		if (GridPosition.X > 0 && map[GridPosition.X - 1, GridPosition.Y] != null)
			unsuitableAsteroids.Add(map[GridPosition.X - 1, GridPosition.Y].TypeOfObject);
		if (GridPosition.Y > 0 && map[GridPosition.X, GridPosition.Y - 1] != null)
			unsuitableAsteroids.Add(map[GridPosition.X, GridPosition.Y - 1].TypeOfObject);
		if (GridPosition.X < map.GetLength(0) - 1 && map[GridPosition.X + 1, GridPosition.Y] != null)
			unsuitableAsteroids.Add(map[GridPosition.X + 1, GridPosition.Y].TypeOfObject);
		if (GridPosition.Y < map.GetLength(1) - 1 && map[GridPosition.X, GridPosition.Y + 1] != null)
			unsuitableAsteroids.Add(map[GridPosition.X, GridPosition.Y + 1].TypeOfObject);
		unsuitableAsteroids = unsuitableAsteroids.Distinct().ToList();
		var coords = new List<Coordinate>();
		for (int i = 0; i < map.GetLength(0); i++)
		{
			for (int j = 0; j < map.GetLength(1); j++)
			{
				if (map[i, j] != null && map[i, j].IsAsteroid() && !unsuitableAsteroids.Contains(map[i, j].TypeOfObject)
				    &&
				    ((j < map.GetLength(1) - 1 && map[i, j + 1] != null &&
					map[i, j].TypeOfObject == map[i, j + 1].TypeOfObject) ||
					(j > 0 && map[i, j - 1] != null &&
					map[i, j].TypeOfObject == map[i, j - 1].TypeOfObject) ||
					(i > 0&& map[i - 1,j] != null &&
					map[i, j].TypeOfObject == map[i - 1, j].TypeOfObject) ||
					 (i < map.GetLength(0) - 1 && map[i + 1, j] != null &&
					 map[i, j].TypeOfObject == map[i + 1, j].TypeOfObject)))
				{
					coords.Add(map[i, j].GridPosition);
					if (map[i, j].IsUnstable)
					{
						State = SpaceObjectState.Decreasing;
						blackHoleTarget = new Coordinate(i,j);
						if (!GameField.Map[blackHoleTarget.X, blackHoleTarget.Y].IsTargetForBlackHole)
						{
							GameField.Map[blackHoleTarget.X, blackHoleTarget.Y].IsTargetForBlackHole = true;
							return;
						}
					}
				}
			} 
		}
		if (coords.Count > 0)
		{
			State = SpaceObjectState.Decreasing;
			blackHoleTarget = coords.ElementAt(rnd.Next(coords.Count));
			if (!GameField.Map[blackHoleTarget.X, blackHoleTarget.Y].IsTargetForBlackHole)
				GameField.Map[blackHoleTarget.X, blackHoleTarget.Y].IsTargetForBlackHole = true;
		}
	}

	void OnMouseDown()
	{										 
		if (GameField.IsAnyMoving() || !IsAsteroid() || IsFrozen)
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
		{ 'E', SpaceObjectType.EmptyCell},
		{'I', SpaceObjectType.Ice}
	};
	private static readonly Dictionary<SpaceObjectType, Sprite> SpaceObjectTypesToSprites = new Dictionary<SpaceObjectType, Sprite>
	{
		{SpaceObjectType.GreenAsteroid, GreenAsteroidSprite},
		{SpaceObjectType.RedAsteroid, RedAsteroidSprite},
		{SpaceObjectType.BlueAsteroid, BlueAsteroidSprite},
		{SpaceObjectType.PurpleAsteroid, PurpleAsteroidSprite},
		{SpaceObjectType.YellowAsteroid, YellowAsteroidSprite},
		{SpaceObjectType.EmptyCell, EmptyCellSprite},
		{SpaceObjectType.BlackHole, BlackHoleSprite},
		{SpaceObjectType.Ice, IceSprite}
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
