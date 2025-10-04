using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveHUD : MonoBehaviour {

    public static InteractiveHUD Instance { get; private set; }

    public Slider hostages;

    public bool golemGoal;
    public bool dragonGoal;
    public bool hydraGoal;
    public Button speedx3;
    public Button speedx1;

    public int hostagesNum;

    private bool _state;

    public Text currentAction;
    public Text planState;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 0;
        
        speedx1.onClick.AddListener(() => Time.timeScale = 1);
        speedx3.onClick.AddListener(() => Time.timeScale = 3);
    }

    public void SetHostages()
    {
        hostagesNum = (int)hostages.value;
    }

    public void SetGolemGoal(bool g)
    {
        golemGoal = g;
    }

    public void SetDragonGoal(bool d)
    {
        dragonGoal = d;
    }

    public void SetHydraGoal(bool h)
    {
        hydraGoal = h;
    }

    public void SetStateOfTheWorld()
    {
        _state = true;
        Time.timeScale = 1;
    }

    public bool GetState()
    {
        return _state;
    }

    public void SetAction(string _currentAction)
    {
        currentAction.text = _currentAction;
    }

    public void SetPlanCompleted(bool state)
    {
        if(state)
        {
            planState.text = "CORRECTO";
            planState.color = Color.green;
        }
        else
        {
            planState.text = "NO CONSIGUIO";
            planState.color = Color.red;
        }
    }
}
