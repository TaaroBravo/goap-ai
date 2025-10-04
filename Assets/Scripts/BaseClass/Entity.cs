using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public abstract class Entity : MonoBehaviour
{
    protected float _life;
    protected float _maxLife;
    public bool Dead { get; protected set; }
    public bool _attacking;
    protected bool _isMoving;

    //Avoidance
    protected List<Collider> _obstacles = new List<Collider>();
    protected Transform _closerOb;
    public float radAvoid;
    public float avoidWeight;
    public LayerMask layerObst;
    //

    public abstract event Action<float> OnSpeedChange;
    public abstract event Action<int> OnAttack;
    public abstract event Action Evade;
    public Action OnTakeDamage = () => { };
    public Action OnDying = () => { };

    public Collider attackCollider;

    protected virtual void MoveTo(Vector3 pos)
    {
        

    }

    protected void GetObstacle()
    {
        _obstacles.Clear();
        _obstacles.AddRange(Physics.OverlapSphere(transform.position, radAvoid, layerObst));
        if (_obstacles.Count > 0)
        {
            var temp = _obstacles.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).Where(x =>
            {
                var temp2 = GetComponentsInChildren<Collider>();
                foreach (var item in temp2)
                {
                    if (item.gameObject.Equals(x.gameObject))
                        return false;
                }
                return true;
            }).FirstOrDefault();
            if (temp)
                _closerOb = temp.transform;
            else _closerOb = null;
        }
        else
            _closerOb = null;
    }


    public virtual void TakeDamage(float dmg)
    {
        if (Dead)
            return;
        _life -= dmg;
        if (_life <= 0)
        {
            Dead = true;
            OnDying();
            return;
        }
        OnTakeDamage();
    }

    public virtual void EndAttack()
    {
        _attacking = false;
        attackCollider.enabled = false;
    }
    
    public virtual void ActivateAttackCollider()
    {
        attackCollider.enabled = true;
    }


}
