using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldInfo : MonoBehaviour {

    public string currentZone;
    public string lastZone;

    public Transform[] doors;

    public Dictionary<string, int> enemiesOnZone = new Dictionary<string, int>();
    public Dictionary<string, List<string>> nearZones = new Dictionary<string, List<string>>();
    public Dictionary<string, bool> investigatedZones = new Dictionary<string, bool>();
    public Dictionary<string, List<Tuple<string, PickUps>>> availableObjectsOnZone = new Dictionary<string, List<Tuple<string, PickUps>>>();
    public Dictionary<string, Tuple<bool, bool>> bossesAlive = new Dictionary<string, Tuple<bool, bool>>();
    public Dictionary<string, bool> doorsAndKeys = new Dictionary<string, bool>();

    public Dictionary<string, Transform> doorsPositions = new Dictionary<string, Transform>();

    public Dictionary<string, Zone> zonePositions = new Dictionary<string, Zone>();

    public int hostageSaved;

    public static WorldInfo Instance { get; private set; }


    private void Awake()
    {
        Instance = this;

        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {

        currentZone = "None";
        doorsAndKeys.Add("BlueDoor", false);
        doorsAndKeys.Add("RedDoor", false);
        doorsAndKeys.Add("CyanDoor", false);

        doorsPositions.Add("BlueDoor", doors[0]);
        doorsPositions.Add("RedDoor", doors[1]);
        doorsPositions.Add("CyanDoor", doors[2]);

        bossesAlive.Add("Dragon", Tuple.Create(false, false));
        bossesAlive.Add("Golem", Tuple.Create(false, false));
        bossesAlive.Add("Hydra", Tuple.Create(false, false));

        nearZones.Add("Entry", new List<string>());
        nearZones.Add("Bridge", new List<string>());
        nearZones.Add("EntryHallC", new List<string>());
        nearZones.Add("HallC", new List<string>());
        nearZones.Add("Study", new List<string>());
        nearZones.Add("Balcony", new List<string>());
        nearZones.Add("DragonRoom", new List<string>());
        nearZones.Add("GolemRoom", new List<string>());
        nearZones.Add("Corridor", new List<string>());
        nearZones.Add("MagicRoom", new List<string>());
        nearZones.Add("HallA", new List<string>());
        nearZones.Add("RoomA", new List<string>());
        nearZones.Add("OutsideA", new List<string>());
        nearZones.Add("HallB", new List<string>());
        nearZones.Add("Library", new List<string>());
        nearZones.Add("OutsideB", new List<string>());
        nearZones.Add("Canteen", new List<string>());
        nearZones.Add("Armory", new List<string>());
        nearZones.Add("HydraRoom", new List<string>());

        availableObjectsOnZone.Add("Entry", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Bridge", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("EntryHallC", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("HallC", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Study", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Balcony", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("DragonRoom", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("GolemRoom", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Corridor", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("MagicRoom", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("HallA", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("RoomA", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("OutsideA", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("HallB", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Library", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("OutsideB", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Canteen", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("Armory", new List<Tuple<string, PickUps>>());
        availableObjectsOnZone.Add("HydraRoom", new List<Tuple<string, PickUps>>());

        enemiesOnZone.Add("Entry", 0);
        enemiesOnZone.Add("Bridge", 0);
        enemiesOnZone.Add("EntryHallC", 0);
        enemiesOnZone.Add("HallC", 0);
        enemiesOnZone.Add("Study", 0);
        enemiesOnZone.Add("Balcony", 0);
        enemiesOnZone.Add("DragonRoom", 0);
        enemiesOnZone.Add("GolemRoom", 0);
        enemiesOnZone.Add("Corridor", 0);
        enemiesOnZone.Add("MagicRoom", 0);
        enemiesOnZone.Add("HallA", 0);
        enemiesOnZone.Add("RoomA", 0);
        enemiesOnZone.Add("OutsideA", 0);
        enemiesOnZone.Add("HallB", 0);
        enemiesOnZone.Add("Library", 0);
        enemiesOnZone.Add("OutsideB", 0);
        enemiesOnZone.Add("Canteen", 0);
        enemiesOnZone.Add("Armory", 0);
        enemiesOnZone.Add("HydraRoom", 0);

        investigatedZones.Add("Entry", false);
        investigatedZones.Add("Bridge", false);
        investigatedZones.Add("EntryHallC", false);
        investigatedZones.Add("HallC", false);
        investigatedZones.Add("Study", false);
        investigatedZones.Add("Balcony", false);
        investigatedZones.Add("DragonRoom", false);
        investigatedZones.Add("GolemRoom", false);
        investigatedZones.Add("Corridor", false);
        investigatedZones.Add("MagicRoom", false);
        investigatedZones.Add("HallA", false);
        investigatedZones.Add("RoomA", false);
        investigatedZones.Add("OutsideA", false);
        investigatedZones.Add("HallB", false);
        investigatedZones.Add("Library", false);
        investigatedZones.Add("OutsideB", false);
        investigatedZones.Add("Canteen", false);
        investigatedZones.Add("Armory", false);
        investigatedZones.Add("HydraRoom", false);
    }

    public void AddZone(string name, Zone pos)
    {
        zonePositions.Add(name, pos);
    }
}
