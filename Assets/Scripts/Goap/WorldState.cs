using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WorldState
{
    public float lifePercentage;
    [Range(0f, 1f)]
    public float energyPercentage;
    public string currentZone;
    public string lastZone;
    [Range(0,3)]
    public int hostageSaved;
    public int potions;
    public Dictionary<string, int> enemiesOnZone = new Dictionary<string, int>();
    public Dictionary<string, List<string>> nearZones = new Dictionary<string, List<string>>();
    public Dictionary<string, bool> investigatedZones = new Dictionary<string, bool>();
    public Dictionary<string, List<string>> availableObjectsOnZone = new Dictionary<string, List<string>>();
    public Dictionary<string, Tuple<bool, bool>> bossesAlive = new Dictionary<string, Tuple<bool, bool>>();
    public Dictionary<string, bool> doorsAndKeys = new Dictionary<string, bool>();

    public MyGoapAction generatingAction;
    public int step;
    public WorldState previousState;

    public WorldState(MyGoapAction genAction = null)
    {
        generatingAction = genAction;
    }

    public WorldState(WorldState copy, MyGoapAction genAction = null)
    {
        lifePercentage = copy.lifePercentage;
        energyPercentage = copy.energyPercentage;
        currentZone = copy.currentZone;
        lastZone = copy.lastZone;
        hostageSaved = copy.hostageSaved;
        potions = copy.potions;
        enemiesOnZone = new Dictionary<string, int>(copy.enemiesOnZone);
        nearZones = new Dictionary<string, List<string>>();
        foreach (var item in copy.nearZones)
        {
            nearZones.Add(item.Key, new List<string>());
            foreach (var item2 in copy.nearZones[item.Key])
                nearZones[item.Key].Add(item2);
        }
        investigatedZones = new Dictionary<string, bool>(copy.investigatedZones);
        availableObjectsOnZone = new Dictionary<string, List<string>>();
        foreach (var item in copy.availableObjectsOnZone)
        {
            availableObjectsOnZone.Add(item.Key, new List<string>());
            foreach (var item2 in copy.availableObjectsOnZone[item.Key])
                availableObjectsOnZone[item.Key].Add(item2);
        }
        bossesAlive = new Dictionary<string, Tuple<bool, bool>>();
        foreach (var item in copy.bossesAlive)
            bossesAlive.Add(item.Key, Tuple.Create(item.Value.Item1, item.Value.Item2));
        doorsAndKeys = new Dictionary<string, bool>(copy.doorsAndKeys);
        generatingAction = genAction;
    }
}
