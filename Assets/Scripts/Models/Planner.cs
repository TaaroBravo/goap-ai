using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using U = Utility;

public class Planner : MonoBehaviour
{
    Player _player;
    List<Tuple<Vector3, Vector3>> debugRayList = new List<Tuple<Vector3, Vector3>>();
    WorldState lastAction =null;
    public static Planner Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _player = FindObjectOfType<Player>();
    }

    public IEnumerable<WorldState> Plan(WorldState initial = null)
    {
        List<MyGoapAction> actions = CreatePossibleActionsList();
        if (initial == null)
        {
            initial = new WorldState();

            initial.lifePercentage = _player.GetLifePercentage();
            initial.currentZone = WorldInfo.Instance.currentZone;
            initial.lastZone = WorldInfo.Instance.lastZone;
            initial.doorsAndKeys = new Dictionary<string, bool>(WorldInfo.Instance.doorsAndKeys);
            initial.bossesAlive = new Dictionary<string, Tuple<bool, bool>>();
            foreach (var item in WorldInfo.Instance.bossesAlive)
                initial.bossesAlive.Add(item.Key, Tuple.Create(item.Value.Item1, item.Value.Item2));
            initial.nearZones = new Dictionary<string, List<string>>();
            foreach (var item in WorldInfo.Instance.nearZones)
            {
                initial.nearZones.Add(item.Key, new List<string>());
                foreach (var item2 in WorldInfo.Instance.nearZones[item.Key])
                    initial.nearZones[item.Key].Add(item2);
            }
            initial.availableObjectsOnZone = new Dictionary<string, List<string>>(WorldInfo.Instance.availableObjectsOnZone.Aggregate(new Dictionary<string, List<string>>(), (acum, curr) => {
                var listOfObjects = new List<string>();
                foreach (var item in curr.Value)
                    listOfObjects.Add(item.Item1);
                acum.Add(curr.Key, listOfObjects);
                return acum;
            }));
            initial.enemiesOnZone = new Dictionary<string, int>(WorldInfo.Instance.enemiesOnZone);
            initial.investigatedZones = new Dictionary<string, bool>(WorldInfo.Instance.investigatedZones);
        }
        else
            initial.lifePercentage= _player.GetLifePercentage();

        WorldState goal = new WorldState();
        var interactiveHUD = InteractiveHUD.Instance;
        if (interactiveHUD.golemGoal)
            goal.bossesAlive.Add("Golem", Tuple.Create(true, true));
        if (interactiveHUD.hydraGoal)
            goal.bossesAlive.Add("Hydra", Tuple.Create(true, true));
        if(interactiveHUD.dragonGoal)
            goal.bossesAlive.Add("Dragon", Tuple.Create(true, true));
        goal.hostageSaved = interactiveHUD.hostagesNum;

        Func<WorldState, WorldState, int> Filter = (x, y) =>
        {
            int heuristic = 0;

            foreach (var item in y.bossesAlive.Keys.Where(alive => y.bossesAlive[alive].Item2))
            {
                if (x.bossesAlive[item].Item1 != y.bossesAlive[item].Item1 || x.bossesAlive[item].Item2 != y.bossesAlive[item].Item2)
                    heuristic += 1000;
            }
            heuristic += Mathf.Clamp((y.hostageSaved - x.hostageSaved)*500, 0, 1500);

            foreach (var zone in y.investigatedZones.Where(inv => inv.Value))
            {
                if (x.investigatedZones[zone.Key] != y.investigatedZones[zone.Key])
                    heuristic += 250;
            }
            if (heuristic == 0)
                lastAction = x;
            return heuristic;
        };

        foreach (var item in GoapRun(initial, goal, actions, Filter))
        {
            yield return item;
        }
    }

    private List<MyGoapAction> CreatePossibleActionsList()
    {
        #region Zone Actions
        Dictionary<string, Action<WorldState>> zoneActions = new Dictionary<string, Action<WorldState>>();
        zoneActions.Add("Entry", world =>
        {
            world.nearZones["Entry"].Add("Corridor");
            world.nearZones["Entry"].Add("Bridge");
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
        });
        zoneActions.Add("Bridge", world =>
         {
             world.nearZones["Bridge"].Add("Entry");
             world.nearZones["Bridge"].Add("EntryHallC");
         });
        zoneActions.Add("EntryHallC", world =>
        {
            world.nearZones["EntryHallC"].Add("Bridge");
            world.nearZones["EntryHallC"].Add("HallC");
            world.enemiesOnZone[world.currentZone] = 2;
        });
        zoneActions.Add("HallC", world =>
        {
            world.nearZones["HallC"].Add("EntryHallC");
            world.nearZones["HallC"].Add("Study");
            world.enemiesOnZone[world.currentZone] = 4;
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
        });
        zoneActions.Add("Study", world =>
        {
            world.nearZones["Study"].Add("HallC");
            world.nearZones["Study"].Add("Balcony");
            world.enemiesOnZone[world.currentZone] = 5;
            world.availableObjectsOnZone[world.currentZone].Add("CyanKey");
            world.availableObjectsOnZone[world.currentZone].Add("Potion");
        });
        zoneActions.Add("Balcony", world =>
        {
            world.nearZones["Balcony"].Add("Study");
            world.enemiesOnZone[world.currentZone] = 6;
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
        });
        zoneActions.Add("DragonRoom", world =>
        {
            world.nearZones["DragonRoom"].Add("HallC");
            world.bossesAlive["Dragon"] = Tuple.Create(true, false);
        });
        zoneActions.Add("GolemRoom", world =>
        {
            world.nearZones["GolemRoom"].Add("Entry");
            world.bossesAlive["Golem"] = Tuple.Create(true, false);
        });
        zoneActions.Add("Corridor", world =>
        {
            world.nearZones["Corridor"].Add("Entry");
            world.nearZones["Corridor"].Add("MagicRoom");
            world.enemiesOnZone[world.currentZone] = 1;
        });
        zoneActions.Add("MagicRoom", world =>
        {
            world.nearZones["MagicRoom"].Add("Corridor");
            world.nearZones["MagicRoom"].Add("HallA");
            world.enemiesOnZone[world.currentZone] = 3;
        });
        zoneActions.Add("HallA", world =>
        {
            world.nearZones["HallA"].Add("MagicRoom");
            world.nearZones["HallA"].Add("HallB");
            world.nearZones["HallA"].Add("RoomA");
            world.enemiesOnZone[world.currentZone] = 4;
        });
        zoneActions.Add("RoomA", world =>
        {
            world.nearZones["RoomA"].Add("HallA");
            world.nearZones["RoomA"].Add("OutsideA");
            world.enemiesOnZone[world.currentZone] = 2;
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
        });
        zoneActions.Add("OutsideA", world =>
        {
            world.nearZones["OutsideA"].Add("RoomA");
            world.enemiesOnZone[world.currentZone] = 3;
            world.availableObjectsOnZone[world.currentZone].Add("RedKey");
            world.availableObjectsOnZone[world.currentZone].Add("Hostage");
        });
        zoneActions.Add("HallB", world =>
        {
            world.nearZones["HallB"].Add("Library");
            world.nearZones["HallB"].Add("Canteen");
            world.nearZones["HallB"].Add("HallA");
            world.enemiesOnZone[world.currentZone] = 5;
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
        });
        zoneActions.Add("Library", world =>
        {
            world.nearZones["Library"].Add("HallB");
            world.nearZones["Library"].Add("OutsideB");
            world.enemiesOnZone[world.currentZone] = 6;
        });
        zoneActions.Add("OutsideB", world =>
        {
            world.nearZones["OutsideB"].Add("Library");
            world.enemiesOnZone[world.currentZone] = 4;
            world.availableObjectsOnZone[world.currentZone].Add("BlueKey");
            world.availableObjectsOnZone[world.currentZone].Add("Potion");
        });
        zoneActions.Add("Canteen", world =>
        {
            world.nearZones["Canteen"].Add("HallB");
            world.nearZones["Canteen"].Add("Armory");
            world.enemiesOnZone[world.currentZone] = 5;
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
        });
        zoneActions.Add("Armory", world =>
        {
            world.nearZones["Armory"].Add("Canteen");
            world.enemiesOnZone[world.currentZone] = 6;
            world.availableObjectsOnZone[world.currentZone].Add("Energy");
            world.availableObjectsOnZone[world.currentZone].Add("Hostage");
        });
        zoneActions.Add("HydraRoom", world =>
        {
            world.nearZones["HydraRoom"].Add("HallB");
            world.bossesAlive["Hydra"] = Tuple.Create(true, false);
        });

        Dictionary<string, Action<WorldState>> doorsEffects = new Dictionary<string, Action<WorldState>>();
        doorsEffects.Add("BlueDoor", world => world.nearZones["HallB"].Add("HydraRoom"));
        doorsEffects.Add("RedDoor", world => world.nearZones["Entry"].Add("GolemRoom"));
        doorsEffects.Add("CyanDoor", world => world.nearZones["HallC"].Add("DragonRoom"));

        Dictionary<string, string> doorsZones = new Dictionary<string, string>();
        doorsZones.Add("BlueDoor", "HallB");
        doorsZones.Add("RedDoor", "Entry");
        doorsZones.Add("CyanDoor", "HallC");

        Dictionary<string, Action<WorldState>> pickUps = new Dictionary<string, Action<WorldState>>();
        pickUps.Add("Potion", world => world.potions++);
        pickUps.Add("Energy", world => world.energyPercentage = 1f);
        pickUps.Add("Hostage", world => world.hostageSaved++);
        pickUps.Add("RedKey", world => world.doorsAndKeys["RedDoor"] = true);
        pickUps.Add("CyanKey", world => world.doorsAndKeys["CyanDoor"] = true);
        pickUps.Add("BlueKey", world => world.doorsAndKeys["BlueDoor"] = true);


        #endregion

        return new List<MyGoapAction>()
        {
              new MyGoapAction("Investigate")
              .SetCost(1f)
              .Pre(world => {
                                if (world.investigatedZones.ContainsKey(world.currentZone))
                                {
                                    return !world.investigatedZones[world.currentZone];
                                }
                                else
                                    world.investigatedZones.Add(world.currentZone,false);
                                return true;
                            })
              .Pre(world=>world.lifePercentage>0f)
              .Effect(world=>zoneActions[world.currentZone](world))
              .Effect(world=>world.investigatedZones[world.currentZone]=true),

              new MyGoapAction("MeeleKill")
                .SetCost(5f)
                .Pre(world=>world.enemiesOnZone[world.currentZone]>0)
                .Pre(world=>world.lifePercentage>0f)
                .Effect(world=>world.lifePercentage-=0.05f * world.enemiesOnZone[world.currentZone])
                .Effect(world=>{
                    var enemiesAlive = 0f;
                    if(world.lifePercentage <= 0)
                        enemiesAlive = Mathf.Abs(world.lifePercentage) / 0.05f;
                    world.enemiesOnZone[world.currentZone] = Mathf.FloorToInt(enemiesAlive);
                 }),

              new MyGoapAction("RangeKill")
                .SetCost(3f)
                .Pre(world=>world.enemiesOnZone[world.currentZone]>0)
                .Pre(world=>world.lifePercentage>0f)
                .Pre(world=>world.energyPercentage>=0.5f)
                .Effect(world=>world.enemiesOnZone[world.currentZone] = 0)
                .Effect(world=>world.energyPercentage-=0.5f),

              new MyGoapAction("Flee")
              .SetCost(6f)
              .Pre(world => world.lifePercentage>0f)
              .Pre(world => world.lifePercentage<0.2f)
              .Pre(world => world.enemiesOnZone[world.currentZone]>0f)
              .Effect(world => world.currentZone=world.lastZone),

              new MyGoapAction("OpenDoor")
              .SetCost(1f)
              .Pre(world => world.doorsAndKeys.Where(x=>x.Value).Count()>0)
              .Pre(world => world.enemiesOnZone[world.currentZone]==0)
              .Pre(world => world.lifePercentage>0f)
              .Pre(world => world.currentZone == doorsZones[world.doorsAndKeys.Where(x=>x.Value).First().Key])
              .Effect(world =>
                  {
                      var doorToOpen = world.doorsAndKeys.Where(x=>x.Value).First().Key;
                      doorsEffects[doorToOpen](world);
                      world.doorsAndKeys.Remove(doorToOpen);
                  }),

              new MyGoapAction("PickUp")
              .SetCost(2f)
              .Pre(world => world.availableObjectsOnZone[world.currentZone].Any())
              .Pre(world => world.enemiesOnZone[world.currentZone]==0)
              .Pre(world => world.lifePercentage>0f)
              .Effect(world =>
              {
                  foreach (var item in world.availableObjectsOnZone[world.currentZone])
                  {
                      pickUps[item](world);
                  }
                   world.availableObjectsOnZone[world.currentZone].Clear();
              }),

              new MyGoapAction("UsePotion")
              .SetCost(3f)
              .Pre(world => world.potions>0)
              .Pre(world => world.enemiesOnZone[world.currentZone]==0)
              .Pre(world => world.lifePercentage<0.4f)
              .Pre(world => world.lifePercentage>0f)
              .Effect(world => world.lifePercentage+=0.6f)
              .Effect(world => world.potions--),


              new MyGoapAction("Revive")
              .SetCost(100f)
              .Pre(world => world.lifePercentage<=0f)
              .Effect(world => world.lifePercentage = 1f)
              .Effect(world => world.currentZone=world.lastZone),

              new MyGoapAction("KillBoss")
              .SetCost(1f)
              .Pre(world => world.bossesAlive.Where(x => x.Value.Item1 && !x.Value.Item2).Any())
              .Pre(world => world.lifePercentage>0f)
              .Pre(world => world.currentZone == world.bossesAlive.Where(x => x.Value.Item1 && !x.Value.Item2).First().Key + "Room")
              .Effect(world => world.lifePercentage -= 0.4f)
              .Effect(world => {
                  if (world.lifePercentage > 0f)
                  {
                      world.bossesAlive[world.bossesAlive.Where(x => x.Value.Item1 && !x.Value.Item2).First().Key] = Tuple.Create(true, true);
                  }
              }),

              new MyGoapAction("SwitchZone")
              .SetCost(45f)
              .Pre(world => world.lifePercentage>0f)
              .Pre(world => world.enemiesOnZone[world.currentZone]==0)
              .Pre(world => world.investigatedZones[world.currentZone])
              .Pre(world => world.availableObjectsOnZone[world.currentZone].Count() == 0)
              .Pre(world => world.nearZones.ContainsKey(world.currentZone) && world.nearZones[world.currentZone].Count>0)
              .Effect(world => world.lastZone=world.currentZone)               
              .Effect(world => {
                  var temp =  world.nearZones[world.currentZone].OrderBy(x=> world.investigatedZones[x]).Skip(UnityEngine.Random.Range(0, world.nearZones[world.currentZone].Count())).First();
                  world.nearZones[world.currentZone].Remove(temp);
                  world.nearZones[world.currentZone].Add(temp);
                  world.currentZone = temp;})

        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var t in debugRayList)
        {
            Gizmos.DrawRay(t.Item1, (t.Item2 - t.Item1).normalized);
            Gizmos.DrawCube(t.Item2 + Vector3.up, Vector3.one * 0.2f);
        }
    }

    public IEnumerable<WorldState> GoapRun(WorldState from, WorldState to, IEnumerable<MyGoapAction> actions, Func<WorldState, WorldState, int> CalculateHeuristic)
    {
        int watchdog = 10000;

        var seq = AStarNormal<WorldState>.Run(
            from,
            to,
            (curr, goal) => CalculateHeuristic(curr, goal),
            curr => CalculateHeuristic(curr, to) == 0,
            curr =>
            {
                if (watchdog == 0)
                {
                    InteractiveHUD.Instance.SetPlanCompleted(false);
                    return Enumerable.Empty<AStarNormal<WorldState>.Arc>();
                }
                else
                {
                    InteractiveHUD.Instance.SetPlanCompleted(true);
                    watchdog--;
                }
                return actions.Where(action =>
                {
                    foreach (var item in action.Preconditions)
                    {
                            if (!item(curr))
                                return false;
                    }
                    return true;
                })
                .Aggregate(new FList<AStarNormal<WorldState>.Arc>(), (possibleList, action) =>
                              {
                                  var newState = new WorldState(curr, action);
                                  foreach (var Eff in action.Effects)
                                  {
                                      Eff(newState);
                                  }
                                  newState.previousState = curr;
                                  newState.step = curr.step + 1;
                                  return possibleList + new AStarNormal<WorldState>.Arc(newState, action.Cost);
                              });
            });

        if (seq == null)
        {
            GetComponent<AudioSource>().Play();
            Debug.Log("Imposible planear");
            yield return null;
        }

        foreach (var act in seq.Skip(1))
        {
            yield return act;
        }

            
        Debug.Log("WATCHDOG " + watchdog);

    }

}
