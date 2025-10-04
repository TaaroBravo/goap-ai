using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using IA2;

public enum PlayerStates
{
    Combat,
    LongRangeCombat,
    Dead,
    Move,
    OpenDoor,
    PickUp,
    Flee,
    SwitchZone,
    DrinkPotion,
    Investigate,
    Idle
}

public class Player : Entity
{
    int _currentIndex;
    protected SpatialGrid _grid;
    public LayerMask nodeMask;
    List<PathNode> astarPath;
    EventFSM<PlayerStates> _fsm;

    Enemy _target;
    public Transform destiny;
    public float rotSpeed;
    [Range(0f, 1f)]
    public float evadeChance;

    PlayerView _view;

    float _energy;
    float _maxEnergy;
    public int potions;

    public override event Action<float> OnSpeedChange;
    public override event Action<int> OnAttack;
    public override event Action Evade = () => { };
    public event Action OnRangeAttack = () => { };
    public event Action OnEvade;
    public event Action Attack;
    IEnumerable<WorldState> _plan;
    WorldState _currentWorld;

    Dictionary<string, PlayerStates> _actionsToStates = new Dictionary<string, PlayerStates>();

    public float enemyRadious;
    public LayerMask enemyLayer;
    public int combosAvailable;
    public bool evading;

    public Zone zoneSelected;
    public Image lifeHUD;


    private void Awake()
    {
        _life = _maxLife = 100f;
        _energy = _maxEnergy = 1f;
        _grid = FindObjectOfType<SpatialGrid>();
        _view = new PlayerView(this, GetComponent<Animator>());

        attackCollider = GetComponentInChildren<Skill>().GetComponent<Collider>();
        EndAttack();

        GenerateActionStateDict();
        ConfigureFSM();

    }

    void Start()
    {
        StartCoroutine(MakePlan());
    }

    IEnumerator MakePlan()
    {
        yield return new WaitUntil(() => InteractiveHUD.Instance.GetState());
        yield return new WaitForSeconds(1f);
        GoToPlan();
    }

    private void Update()
    {
        _fsm.Update();
        lifeHUD.fillAmount= Mathf.Clamp(_life / _maxLife, 0, 1f);
    }

    private void GenerateActionStateDict()
    {
        _actionsToStates.Add("Investigate", PlayerStates.Investigate);
        _actionsToStates.Add("MeeleKill", PlayerStates.Combat);
        _actionsToStates.Add("RangeKill", PlayerStates.LongRangeCombat);
        _actionsToStates.Add("Flee", PlayerStates.Flee);
        _actionsToStates.Add("OpenDoor", PlayerStates.OpenDoor);
        _actionsToStates.Add("PickUp", PlayerStates.PickUp);
        _actionsToStates.Add("UsePotion", PlayerStates.DrinkPotion);
        _actionsToStates.Add("Revive", PlayerStates.Dead);
        _actionsToStates.Add("KillBoss", PlayerStates.Combat);
        _actionsToStates.Add("SwitchZone", PlayerStates.SwitchZone);
    }

    public void ConfigureFSM()
    {
        var idle = new State<PlayerStates>("idle"); //X 
        var combat = new State<PlayerStates>("combat"); //X
        var longRangeCombat = new State<PlayerStates>("longRangeCombat");
        var dead = new State<PlayerStates>("dead"); //X
        var move = new State<PlayerStates>("move"); //X 
        var openDoor = new State<PlayerStates>("openDoor"); //X 
        var flee = new State<PlayerStates>("flee"); //x
        var switchZone = new State<PlayerStates>("switchZone"); //X 
        var drinkPotion = new State<PlayerStates>("drinkPotion"); //X
        var investigate = new State<PlayerStates>("investigate");
        var pickUp = new State<PlayerStates>("pickUp"); //X

        #region PlayerStates
        Action SearchEnemies = () =>
        {
            var worldInfoTemp = WorldInfo.Instance.zonePositions[WorldInfo.Instance.currentZone];
            var temp = worldInfoTemp.enemies.FirstOrDefault();
            if (temp)
            {
                _target = temp;
                temp.OnDying += () => _target = null;
            }
            else _target = null;
        };


        move.OnEnter += (x) => OnSpeedChange(1f);
        move.OnUpdate += Advance;
        move.OnExit += (x) =>
        {
            _isMoving = false;
            OnSpeedChange(0f);
        };

        combat.OnEnter += a => SearchEnemies();

        combat.OnUpdate += () =>
        {
            if (_attacking || evading)
                return;

            if (!_target)
            {
                SearchEnemies();
                if (!_target)
                {
                    NextStep();
                    return;
                }
            }

            if (Vector3.Distance(_target.transform.position, transform.position) < _target.distanceToAttack)
            {
                OnSpeedChange(0f);
                OnAttack(UnityEngine.Random.Range(1, combosAvailable + 1));
                _isMoving = false;
                _attacking = true;
            }
            else
            {
                var dir = (_target.transform.position - transform.position).normalized;
                GetObstacle();
                if (_closerOb != null)
                    dir += ((transform.position - _closerOb.position) * avoidWeight);
                dir.y = 0;
                transform.forward = Vector3.Slerp(transform.forward, dir, rotSpeed * Time.deltaTime);
                OnSpeedChange(1f);
            }
        };

        dead.OnEnter += (x) =>
        {
            if (_life > 0f)
            {
                _currentWorld.currentZone = WorldInfo.Instance.currentZone;
                _currentWorld.enemiesOnZone[_currentWorld.currentZone] = 0;
                WorldInfo.Instance.enemiesOnZone[_currentWorld.currentZone] = 0;
                _plan = Planner.Instance.Plan(_currentWorld).ToList();
                foreach (var item in _plan)
                {
                    if (item.generatingAction != null)
                        Debug.Log(item.generatingAction.Name);
                }
                if (_plan.First().generatingAction.Name.Equals("Revive"))
                    _plan = _plan.Skip(1);
                NextStep();
            }
            else
            {
                OnDying();
                _life = _maxLife;
                if (_plan.Any() && _plan.First() != null && _plan.First().generatingAction.Name.Equals("Revive"))
                {
                    _plan = _plan.Skip(1);
                    NextStep();
                }
                else if(_currentWorld.generatingAction != null && _currentWorld.generatingAction.Name.Equals("Revive"))
                {
                    transform.position = WorldInfo.Instance.zonePositions[WorldInfo.Instance.lastZone].transform.position;
                    WorldInfo.Instance.currentZone = WorldInfo.Instance.lastZone;
                }
                else
                {
                    _fsm.Feed(_actionsToStates[_currentWorld.generatingAction.Name]);
                }
            }
        };        

        openDoor.OnEnter += (x) =>
        {
            var blackKey = WorldInfo.Instance.doorsAndKeys.Where(e => e.Value).First().Key;
            var doorPositionWorldTemp = WorldInfo.Instance.doorsPositions[blackKey];
            if (Vector3.Distance(transform.position, doorPositionWorldTemp.position) < 1f)
                MoveTo(doorPositionWorldTemp.position);
            else
            {
                Destroy(doorPositionWorldTemp.gameObject);
                WorldInfo.Instance.doorsAndKeys.Remove(blackKey);
                WorldInfo.Instance.doorsPositions.Remove(blackKey);
                NextStep();
            }
        };

        switchZone.OnEnter += (x) =>
        {
            if (WorldInfo.Instance.currentZone != _currentWorld.currentZone)
            {
                MoveTo(WorldInfo.Instance.zonePositions[_currentWorld.currentZone].transform.position);
                WorldInfo.Instance.lastZone = WorldInfo.Instance.currentZone;
                WorldInfo.Instance.currentZone = _currentWorld.currentZone;
            }
            else
                NextStep();
        };

        drinkPotion.OnEnter += x =>
        {
            _life += Mathf.Clamp((_life / _maxLife) * 0.6f, 0, _maxLife);
            potions--;
            NextStep();
        };

        pickUp.OnEnter += x =>
        {
            var worldInfoTemp = WorldInfo.Instance;
            var pickUpOnZone = worldInfoTemp.availableObjectsOnZone[worldInfoTemp.currentZone];
           
            if(pickUpOnZone.Any())
            {
                var firstItem = pickUpOnZone.First();
                if (Vector3.Distance(firstItem.Item2.transform.position, transform.position) < 3f)
                {
                    firstItem.Item2.PickedUp(this);
                    pickUpOnZone.Remove(firstItem);
                    _fsm.Feed(PlayerStates.PickUp);
                    return;
                }
                else
                {
                    MoveTo(firstItem.Item2.transform.position);
                    return;
                }
            }
            else
                NextStep();    
        };

        flee.OnEnter += x =>
        {
            var worldInfoTemp = WorldInfo.Instance;
            if (WorldInfo.Instance.currentZone != _currentWorld.lastZone)
            {
                MoveTo(worldInfoTemp.zonePositions[worldInfoTemp.currentZone].transform.position);
                WorldInfo.Instance.currentZone = _currentWorld.lastZone;
            }
            else
                NextStep();
        };

        longRangeCombat.OnEnter += x => SearchEnemies();

        longRangeCombat.OnUpdate += () => {

            var worldInfoTemp = WorldInfo.Instance.zonePositions[WorldInfo.Instance.currentZone];
            var temp = worldInfoTemp.enemies;
            List<Enemy> temp2 = new List<Enemy>(temp);
            OnRangeAttack();
            foreach (var item in temp2)
            {
                item.TakeDamage(1000);
            }
            NextStep();
        };

        investigate.OnEnter += x =>
        {
            var worldInfoTemp = WorldInfo.Instance;
            worldInfoTemp.availableObjectsOnZone[worldInfoTemp.currentZone] = worldInfoTemp.zonePositions[worldInfoTemp.currentZone].pickups.Select(y=>Tuple.Create(y.pickUpName,y)).ToList();
            worldInfoTemp.enemiesOnZone[worldInfoTemp.currentZone] = worldInfoTemp.zonePositions[worldInfoTemp.currentZone].enemiesOnZone;
            worldInfoTemp.investigatedZones[worldInfoTemp.currentZone] = true;            
            worldInfoTemp.nearZones = new Dictionary<string, List<string>>();
            foreach (var item in _currentWorld.nearZones)
            {
                worldInfoTemp.nearZones.Add(item.Key, new List<string>());
                foreach (var item2 in _currentWorld.nearZones[item.Key])
                    worldInfoTemp.nearZones[item.Key].Add(item2);
            }

            worldInfoTemp.bossesAlive = new Dictionary<string, Tuple<bool, bool>>();
            foreach (var item in _currentWorld.bossesAlive)
                worldInfoTemp.bossesAlive.Add(item.Key, Tuple.Create(item.Value.Item1, item.Value.Item2));
            NextStep();
        };


        #endregion

        #region Transitions FSM
        StateConfigurer.Create(idle).SetTransition(PlayerStates.Combat, combat)
                                    .SetTransition(PlayerStates.LongRangeCombat, longRangeCombat)
                                    .SetTransition(PlayerStates.Dead, dead)
                                    .SetTransition(PlayerStates.Move, move)
                                    .SetTransition(PlayerStates.OpenDoor, openDoor)
                                    .SetTransition(PlayerStates.Flee, flee)
                                    .SetTransition(PlayerStates.SwitchZone, switchZone)
                                    .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                    .SetTransition(PlayerStates.PickUp, pickUp)
                                    .SetTransition(PlayerStates.Investigate, investigate).Done();

        StateConfigurer.Create(combat).SetTransition(PlayerStates.Idle, idle)
                                    .SetTransition(PlayerStates.LongRangeCombat, longRangeCombat)
                                    .SetTransition(PlayerStates.Dead, dead)
                                    .SetTransition(PlayerStates.Move, move)
                                    .SetTransition(PlayerStates.OpenDoor, openDoor)
                                    .SetTransition(PlayerStates.Flee, flee)
                                    .SetTransition(PlayerStates.SwitchZone, switchZone)
                                    .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                    .SetTransition(PlayerStates.PickUp, pickUp).Done();

        StateConfigurer.Create(longRangeCombat).SetTransition(PlayerStates.Idle, idle)
                                   .SetTransition(PlayerStates.Combat, combat)
                                   .SetTransition(PlayerStates.Dead, dead)
                                   .SetTransition(PlayerStates.Move, move)
                                   .SetTransition(PlayerStates.OpenDoor, openDoor)
                                   .SetTransition(PlayerStates.Flee, flee)
                                   .SetTransition(PlayerStates.SwitchZone, switchZone)
                                   .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                   .SetTransition(PlayerStates.PickUp, pickUp).Done();

        StateConfigurer.Create(move).SetTransition(PlayerStates.Idle, idle)
                                   .SetTransition(PlayerStates.Combat, combat)
                                   .SetTransition(PlayerStates.Dead, dead)
                                   .SetTransition(PlayerStates.Move, move)
                                   .SetTransition(PlayerStates.OpenDoor, openDoor)
                                   .SetTransition(PlayerStates.Flee, flee)
                                   .SetTransition(PlayerStates.SwitchZone, switchZone)
                                   .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                   .SetTransition(PlayerStates.Investigate, investigate)
                                   .SetTransition(PlayerStates.LongRangeCombat, longRangeCombat)
                                   .SetTransition(PlayerStates.PickUp, pickUp).Done();

        StateConfigurer.Create(openDoor).SetTransition(PlayerStates.Idle, idle)
                                   .SetTransition(PlayerStates.Investigate, investigate)
                                   .SetTransition(PlayerStates.Move, move)
                                   .SetTransition(PlayerStates.SwitchZone, switchZone)
                                   .SetTransition(PlayerStates.OpenDoor, openDoor)
                                   .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                   .SetTransition(PlayerStates.PickUp, pickUp).Done();

        StateConfigurer.Create(flee).SetTransition(PlayerStates.Idle, idle)
                                  .SetTransition(PlayerStates.Move, move)
                                  .SetTransition(PlayerStates.SwitchZone, switchZone)
                                  .SetTransition(PlayerStates.OpenDoor, openDoor)
                                  .SetTransition(PlayerStates.DrinkPotion, drinkPotion).Done();

        StateConfigurer.Create(drinkPotion).SetTransition(PlayerStates.Idle, idle)
                                  .SetTransition(PlayerStates.Move, move)
                                  .SetTransition(PlayerStates.SwitchZone, switchZone)
                                  .SetTransition(PlayerStates.OpenDoor, openDoor)
                                  .SetTransition(PlayerStates.Investigate, investigate)
                                  .SetTransition(PlayerStates.PickUp, pickUp).Done();

        StateConfigurer.Create(investigate).SetTransition(PlayerStates.Idle, idle)
                                  .SetTransition(PlayerStates.Combat, combat)
                                  .SetTransition(PlayerStates.Dead, dead)
                                  .SetTransition(PlayerStates.Move, move)
                                  .SetTransition(PlayerStates.OpenDoor, openDoor)
                                  .SetTransition(PlayerStates.Flee, flee)
                                  .SetTransition(PlayerStates.SwitchZone, switchZone)
                                  .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                  .SetTransition(PlayerStates.LongRangeCombat, longRangeCombat)
                                  .SetTransition(PlayerStates.PickUp, pickUp).Done();

        StateConfigurer.Create(pickUp).SetTransition(PlayerStates.Idle, idle)
                                  .SetTransition(PlayerStates.Dead, dead)
                                  .SetTransition(PlayerStates.Move, move)
                                  .SetTransition(PlayerStates.PickUp, pickUp)
                                  .SetTransition(PlayerStates.OpenDoor, openDoor)
                                  .SetTransition(PlayerStates.SwitchZone, switchZone)
                                  .SetTransition(PlayerStates.DrinkPotion, drinkPotion).Done();

        StateConfigurer.Create(switchZone).SetTransition(PlayerStates.Combat, combat)
                                    .SetTransition(PlayerStates.LongRangeCombat, longRangeCombat)
                                    .SetTransition(PlayerStates.Dead, dead)
                                    .SetTransition(PlayerStates.Move, move)
                                    .SetTransition(PlayerStates.OpenDoor, openDoor)
                                    .SetTransition(PlayerStates.Flee, flee)
                                    .SetTransition(PlayerStates.Idle, idle)
                                    .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                    .SetTransition(PlayerStates.PickUp, pickUp)
                                    .SetTransition(PlayerStates.SwitchZone, switchZone)
                                    .SetTransition(PlayerStates.Investigate, investigate).Done();

        StateConfigurer.Create(dead).SetTransition(PlayerStates.Combat, combat)
                                    .SetTransition(PlayerStates.Dead, dead)
                                    .SetTransition(PlayerStates.LongRangeCombat, longRangeCombat)
                                    .SetTransition(PlayerStates.Idle, idle)
                                    .SetTransition(PlayerStates.Move, move)
                                    .SetTransition(PlayerStates.OpenDoor, openDoor)
                                    .SetTransition(PlayerStates.Flee, flee)
                                    .SetTransition(PlayerStates.SwitchZone, switchZone)
                                    .SetTransition(PlayerStates.DrinkPotion, drinkPotion)
                                    .SetTransition(PlayerStates.PickUp, pickUp)
                                    .SetTransition(PlayerStates.Investigate, investigate).Done();
        #endregion

        _fsm = new EventFSM<PlayerStates>(idle);
        var en = FindObjectsOfType<Enemy>();
        foreach (var item in en)
        {
            item.Evade += ChanceToEvade;
        }
    }



    public void ChanceToEvade()
    {
        if (evading)
            return;
        var rnd = UnityEngine.Random.Range(0f, 1f);
        if (rnd <= evadeChance)
        {
            transform.forward = (UnityEngine.Random.Range(-1, 1) * transform.forward + UnityEngine.Random.Range(-1, 2) * transform.right).normalized;
            OnEvade();
            evading = true;
        }
    }

    public override void TakeDamage(float dmg)
    {
        _life -= dmg;
        if (_life <= 0)
        {
            _fsm.Feed(PlayerStates.Dead);
        }
        OnTakeDamage();
    }

    private void GoToPlan()
    {
        _plan = Planner.Instance.Plan().ToList();
        foreach (var item in _plan)
        {
            if(item.generatingAction!=null)
                Debug.Log(item.generatingAction.Name);
        }
        NextStep();
    }

    IEnumerator MoveIt()
    {
        yield return new WaitForSeconds(1f);
        MoveTo(destiny.position);
    }

    protected override void MoveTo(Vector3 pos)
    {
        _isMoving = true;
        _currentIndex = 0;
        astarPath = null;
        astarPath = GetPath(FindNearNode(transform.position), FindNearNode(pos));
        if (astarPath == null)
            _isMoving = false;
        _fsm.Feed(PlayerStates.Move);
    }

    protected void Advance()
    {
        if (_attacking || Dead)
            return;

        if (astarPath == null || _currentIndex == astarPath.Count)
        {
            _fsm.Feed(_actionsToStates[_currentWorld.generatingAction.Name]);
            return;
        }

        float dist = Vector3.Distance(astarPath[_currentIndex].transform.position, transform.position);
        if (dist >= 1f)
        {
            var dir = (astarPath[_currentIndex].transform.position - transform.position).normalized;
            dir.y = 0;
            transform.forward = Vector3.Slerp(transform.forward, dir, rotSpeed * Time.deltaTime);
        }
        else
            _currentIndex++;
    }



    List<PathNode> GetPath(PathNode start, PathNode end)
    {
        return Utility.AStar(start, x => x.Equals(end),
                            x =>
                            {
                                foreach (var item in x.neighbors)
                                {
                                    item.Item1.GetComponent<MeshRenderer>().material.color = Color.yellow;
                                }
                                return x.neighbors;
                            }
                            , x => Vector3.Distance(x.transform.position, end.transform.position)).ToList();
    }

    protected PathNode FindNearNode(Vector3 pos)
    {
        var posTargets = _grid.GetPosibleTargets(pos);
        PathNode closestNode = null;
        if (posTargets != null)
            closestNode = posTargets.Select(x => x.GetComponent<PathNode>()).Where(x => x != null).Where(x => !x.isBlocked)
                          .Where(x => Mathf.Abs(x.transform.position.y - pos.y) <= 1.2f)
                          .OrderBy(x => Vector3.Distance(x.transform.position, pos)).FirstOrDefault();
        if (!closestNode)
        {
            var nod = Physics.OverlapSphere(pos, 10f, nodeMask);

            closestNode = nod.Select(x => x.GetComponent<PathNode>()).Where(x => x != null).Where(x => !x.isBlocked)
                             .Where(x => Mathf.Abs(x.transform.position.y - pos.y) <= 2f)
                             .OrderBy(x => Vector3.Distance(x.transform.position, pos)).FirstOrDefault();
        }
        
        return closestNode;
    }

    void NextStep()
    {
        if (!_plan.Any())
            return;
        _currentWorld = _plan.First();
        _plan = _plan.Skip(1);
        _fsm.Feed(_actionsToStates[_currentWorld.generatingAction.Name]);
        InteractiveHUD.Instance.SetAction(_currentWorld.generatingAction.Name);

    }

    public float GetLifePercentage()
    {
        return _life / _maxLife;
    }

    public void RechargeEnergy()
    {
        _energy = _maxEnergy;
    }

    private void OnDrawGizmos()
    {
        if (zoneSelected != null)
        {
            Gizmos.color = Color.yellow;
            zoneSelected = WorldInfo.Instance.zonePositions[WorldInfo.Instance.currentZone];
            Gizmos.DrawCube(zoneSelected.ReturnCollider().bounds.center, zoneSelected.ReturnCollider().bounds.size);
        }
    }
}
