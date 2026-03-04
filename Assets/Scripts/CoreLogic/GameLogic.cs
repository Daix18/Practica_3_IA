using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public PlayerList PlayerList;
    public GameState GameState;
    public GeneticController1on1 geneticAI;

    public GameEvent EndGameEvent;
    public AttackResultEvent AttackResult;
    public PlayerEvent ChangeTurnEvent;

    [Header("Ajustes Modo Juego")]
    public int maxPlayMatches = 10;
    private int currentPlayMatch = 0;
    private int _count = 0;

    private void Awake()
    {
        // Carga la población entrenada si no estamos en modo entrenamiento
        if (!geneticAI.isTrainingMode) geneticAI.LoadPopulation();
    }

    public IEnumerator Start()
    {
        // En entrenamiento usamos una espera mínima para máxima velocidad
        yield return geneticAI.isTrainingMode ? new WaitForEndOfFrame() : new WaitForSeconds(1f);
        GameState.IsFinished = false;
        ChangeTurn();
    }

    public void ChangeTurn()
    {
        // Alterna entre los dos jugadores disponibles
        int next = _count;
        _count = (_count + 1) % 2;
        GameState.CurrentPlayer = PlayerList.Players[next];
        ChangeTurnEvent.Raise(PlayerList.Players[next]);
    }

    public void OnAttackDone(Attack att)
    {
        if (att == null) return;
        var hitRoll = Dice.PercentageChance();
        int energyCost = att.AttackMade.Energy;

        // Verifica si el atacante tiene energía para ejecutar el movimiento seleccionado
        if (att.Source.Energy >= energyCost)
        {
            bool isHit = hitRoll <= att.AttackMade.HitChance;
            int damage = 0;

            if (isHit)
            {
                damage = Dice.RangeRoll(att.AttackMade.MinDam, att.AttackMade.MaxDam + 1);
                att.Target.HP -= damage;
            }
            att.Source.Energy -= energyCost;

            // No procesamos efectos visuales durante el entrenamiento para ahorrar recursos
            if (!geneticAI.isTrainingMode)
                RaiseAttackVisuals(att, isHit, damage, energyCost);
        }

        if (!EndGameTest()) ChangeTurn();
    }

    private void RaiseAttackVisuals(Attack att, bool hit, int dam, int energy)
    {
        AttackResult result = ScriptableObject.CreateInstance<AttackResult>();
        result.Attack = att;
        result.IsHit = hit;
        result.Damage = dam;
        result.Energy = energy;
        AttackResult.Raise(result);
    }

    private bool EndGameTest()
    {
        // Verifica si algún jugador ha perdido toda su vida
        if (PlayerList.Players.Any(p => p.HP <= 0))
        {
            GameState.IsFinished = true;

            // Control de sesión en modo de juego real (límite de 10 partidas)
            if (geneticAI.isPlayingMode)
            {
                currentPlayMatch++;
                Debug.Log($"Partida de juego {currentPlayMatch} de {maxPlayMatches}");

                if (currentPlayMatch >= maxPlayMatches)
                {
                    Debug.Log("Límite de partidas alcanzado en Modo Juego.");
                    EndGameEvent.Raise();
                    return true;
                }

                RestartMatch();
                return true;
            }

            // Control del ciclo evolutivo durante el entrenamiento
            if (geneticAI.isTrainingMode)
            {
                geneticAI.EvaluateFitness();

                if (geneticAI.CurrentIndividualIndex >= geneticAI.populationSize)
                {
                    geneticAI.currentGeneration++;

                    if (geneticAI.currentGeneration >= geneticAI.totalGenerations)
                    {
                        geneticAI.FinishTraining();
                        EndGameEvent.Raise();
                        return true;
                    }

                    geneticAI.NextGeneration();
                    geneticAI.CurrentIndividualIndex = 0;
                }
            }

            RestartMatch();
            return true;
        }
        return false;
    }

    private void RestartMatch()
    {
        // Restablece los valores iniciales de vida y energía para el nuevo combate
        foreach (var player in PlayerList.Players)
        {
            player.HP = player.InitialHP;
            player.Energy = player.InitialEnergy;
        }
        _count = 0;
        GameState.IsFinished = false;

        // Limpieza de memoria para optimizar el rendimiento en sesiones largas de entrenamiento
        if (geneticAI.isTrainingMode)
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            Resources.UnloadUnusedAssets();
        }

        StartCoroutine(Start());
    }
}