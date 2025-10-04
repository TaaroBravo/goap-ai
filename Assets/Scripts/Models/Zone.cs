using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{

    public string zoneName;
    public List<PickUps> pickups = new List<PickUps>();
    public List<Enemy> enemies = new List<Enemy>();
    public int enemiesOnZone;
    Collider _myCollider;

    private void Start()
    {
        _myCollider = GetComponent<Collider>();
        WorldInfo.Instance.zonePositions.Add(zoneName, this);
    }

    public Collider ReturnCollider()
    {
        return _myCollider;
    }

    private void OnTriggerEnter(Collider other)
    {
        var pickUp = other.GetComponent<PickUps>();
        if (pickUp)
            pickups.Add(pickUp);
        else
        {
            var player = other.GetComponent<Player>();
            if (player)
            {
                if(WorldInfo.Instance.currentZone.Equals("None"))
                    WorldInfo.Instance.currentZone = zoneName;
            }
            else
            {
                Enemy temp = other.GetComponent<Kobold>();
                if (temp && !temp.isBoss)
                {
                    enemiesOnZone++;
                    enemies.Add(temp);
                    temp.OnDying += () => enemiesOnZone--;
                    temp.OnDying += () => enemies.Remove(temp);
                }
                else
                {
                    temp = other.GetComponent<Undead>();
                    if (temp && !temp.isBoss)
                    {
                        enemiesOnZone += 2;
                        enemies.Add(temp);
                        temp.OnDying += () => enemiesOnZone-=2;
                        temp.OnDying += () => enemies.Remove(temp);
                    }
                    else
                    {
                        temp = other.GetComponent<Dragonide>();
                        if (temp && !temp.isBoss)
                        {
                            enemiesOnZone += 3;
                            enemies.Add(temp);
                            temp.OnDying += () => enemiesOnZone-=3;
                            temp.OnDying += () => enemies.Remove(temp);
                        }
                        else
                        {
                            temp = other.GetComponent<Enemy>();
                            if (temp)
                            {
                                enemiesOnZone++;
                                enemies.Add(temp);
                                temp.OnDying += () => enemiesOnZone--;
                                temp.OnDying += () => enemies.Remove(temp);

                            }
                        }
                    }
                }

            }
        }

    }
}
