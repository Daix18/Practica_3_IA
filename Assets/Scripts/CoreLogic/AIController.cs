using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class AIController : MonoBehaviour
{
    public PlayerManager Body;
    protected PlayerInfo _player;

    public GameState GameState;
    
    protected Attack _attackToDo;

    public AttackEvent AttackEvent;

    protected LogicState _currentLogicState;


    private static int MaxPly = 3;
    public void Awake()
    {
        _player = Body.Info;
        
    }

    public void OnGameTurnChange(PlayerInfo currentTurn)
    {
        Debug.Log($"Current Turn: {currentTurn.Name}");
        if (currentTurn != _player) return;
        Perceive();
        Think();
        Act();
        
    }

    protected virtual void Perceive()
    {
        _currentLogicState = new LogicState(GameState);
    }

    protected virtual void Think()
    {
        _attackToDo = null;
       
        ExpectMiniMax();
        
    }

    private void ExpectMiniMax()
    {
        _alpha = -20;
        _beta = +20;
        var value = MaxValor(_currentLogicState);
        Debug.Log($"Minimax ---> Action:{_attackToDo}:{value}");
    }


    private float _alpha;
    private float _beta;
    private float MaxValor(LogicState state)
    {
        if (Suspend(state))
        {
            return Evaluate(state);
        }

        _alpha = -20;
        var children = state.GenerateChildren(_player.Id, _player.Attacks);

        foreach (var (att,logicState) in children)
        {
            var val = RandomValor(logicState,att);
            //Debug.Log($"val:{val} -> {att}");
            if (val > _alpha)
            {
                _alpha = val;
                _attackToDo = ScriptableObject.CreateInstance<Attack>();
                _attackToDo.AttackMade = att;
                _attackToDo.Source = _player;
                _attackToDo.Target = GameState.ListOfPlayers.Players[_player.EnemyId];
            }
        }
        return _alpha;
    }

    private float MinValor(LogicState state)
    {
        if (Suspend(state))
        {
            return Evaluate(state);
        }

        _beta = +20;
        var children = state.GenerateChildren(_player.EnemyId, GameState.ListOfPlayers.Players[_player.EnemyId].Attacks);

        foreach (var (att, logicState) in children)
        {
            var val = RandomValor(logicState,att);
            if (val < _beta)
            {
                _beta = val;
                
            }
        }
        return _beta;
    }

    private float RandomValor(LogicState state,AttackInfo att)
    {
        if (Suspend(state))
        {
            return Evaluate(state);
        }

        var children = state.GenerateChildrenProb(0, att);

        var value = 0.0f;
        foreach (var (natt, logicState) in children)
        {
            if (state.PlayerIdxTurn == 0)
            {
                value += MinValor(logicState) * logicState.Probability;
            }
            else
            {
                value += MaxValor(logicState) * logicState.Probability;
            }
            
        }
        return value;
    }

    private bool Suspend(LogicState state)
    {
        return state.Ply==AIController.MaxPly || state.HitPoints.Any(hp=>hp<=0);
    }

    private float Evaluate(LogicState state)
    {
        if (state.HitPoints[_player.Id] <= 0) return -20;
        if (state.HitPoints[_player.EnemyId] <= 0) return 20;
        return state.HitPoints[_player.Id] - state.HitPoints[_player.EnemyId];
    }

    protected virtual void Act()
    {
        AttackEvent.Raise( _attackToDo);
        
    }

}


public class LogicState
{
    public float[] HitPoints;
    public float[] Energies;
    public int Turn;
    public int PlayerIdxTurn => (Turn-1) % NumPlayers;
    public int NumPlayers;
    public int Ply;

    public float Probability;

    public LogicState(GameState state)
    {
        HitPoints = state.ListOfPlayers.Players.Select(p => p.HP).ToArray();
        Energies = state.ListOfPlayers.Players.Select(p => p.Energy).ToArray();
        Turn = state.TurnNumber;
        NumPlayers = state.ListOfPlayers.Players.Count;
        Ply = 0;
        Probability = 1.0f;
    }

    public LogicState(LogicState parentState, float[] deltaHitPoints, float[] deltaEnergies)
    {
        HitPoints = parentState.HitPoints.Select((x, i) => x - deltaHitPoints[i]).ToArray();
        Energies = parentState.Energies.Select((x, i) => x - deltaEnergies[i]).ToArray();
        Turn = parentState.Turn + 1;
        NumPlayers = parentState.NumPlayers;
        Ply = parentState.Ply + 1;
        Probability = 1.0f;
    }

    public LogicState(LogicState parentState, float[] deltaHitPoints, float[] deltaEnergies, float prob): this(parentState,deltaHitPoints,deltaEnergies)
    {
        Probability = prob;
        Turn--; //No turn passed. Random State ongoing
    }

    public List<(AttackInfo,LogicState)> GenerateChildren(int idxPlayer, AttackInfo[] attacks)
    {
        var res = new List<(AttackInfo, LogicState)>();
        var deltaHp = new float[this.NumPlayers];
        var deltaEnergy = new float[this.NumPlayers];

        foreach (var attack in attacks)
        {
            if (this.Energies[idxPlayer]<attack.Energy) continue;
            
            deltaEnergy[idxPlayer] = attack.Energy;
            res.Add( (attack,new LogicState(this,deltaHp,deltaEnergy)));

        }
        return res;
    }

    public List<(AttackInfo, LogicState)> GenerateChildrenProb(int idxPlayer, AttackInfo attack)
    {
        var res = new List<(AttackInfo, LogicState)>();
        var deltaHp = new float[this.NumPlayers];
        var deltaEnergy = new float[this.NumPlayers];

        //Miss Attack
        res.Add((attack,new LogicState(this,deltaHp,deltaEnergy,1.0f-attack.HitChance)));

        //Hit Attacks
        for (var dam = attack.MinDam; dam<= attack.MaxDam; dam++)
        {
            deltaHp[(idxPlayer+1)%NumPlayers] = dam;
            res.Add((attack,new LogicState(this, deltaHp, deltaEnergy, attack.HitChance/(attack.MaxDam - attack.MinDam+1))));

        }
        return res;
    }

}