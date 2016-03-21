using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.scripts;
using Newtonsoft.Json;
namespace Assets.scripts
{
	public class Game : MonoBehaviour
	{

		// Use this for initialization
		public GameObject SpaceObjectPrefab;
		public const int MAP_SIZE = 8;
		private LevelInfo LevelInformation;

		private void Start()
		{
			LevelInformation = JsonConvert.DeserializeObject<LevelInfo>(File.ReadAllText("Assets/levels/1.json"));
			//LevelInformation =
			//new LevelInfo { Map = "VYGBBGVB GYGYYRBB RYGYYGVB GRGRGRVR RYGYVGVB GVGVYGBV RYGYGGRB GYGYYGVV" };
			GenerateMap();

		}

		// Update is called once per frame
		private void Update()
		{

		}

		private void GenerateMap()
		{
			SpaceObject[,] map = new SpaceObject[MAP_SIZE, MAP_SIZE];
			for (var i = 0; i < MAP_SIZE; i++)
			{
				for (var j = 0; j < MAP_SIZE; j++)
				{
					SpaceObject spaceObject = ((GameObject) Instantiate(
						SpaceObjectPrefab, new Vector3(i - Mathf.Floor(MAP_SIZE/2), -j + Mathf.Floor(MAP_SIZE/2), 0),
						Quaternion.Euler(new Vector3())))
						.GetComponent<SpaceObject>();
					spaceObject.Initialise(i, j, LevelInformation.Map.ElementAt(j*(MAP_SIZE+1) + i)); //?
					map[i, j] = spaceObject;
				}
			}
			GameField.Map = map;
		}
	}
}
