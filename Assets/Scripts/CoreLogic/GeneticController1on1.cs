using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

public class GeneticController1on1 : AIController
{
    [Header("Configuración Genética")]
    public int populationSize = 40;
    public int totalGenerations = 50;
    public int matchesPerIndividual = 10;
    public bool isTrainingMode = true;
    public bool isPlayingMode = false;
    public int demoIndex = 0;
    public float MutationRate = 0.1f;
    public float minMutationRate = 0.01f;
    public float mutationDecayFactor = 0.99f;
    public CrossoverType crossoverType;

    [Header("Estado de Entrenamiento")]
    public int currentGeneration = 0;
    public int CurrentIndividualIndex = 0;
    private int currentMatchCount = 0;
    private float accumulatedFitness = 0f;

    [HideInInspector] public int energyFailuresInMatch = 0;

    public List<ControlledIndividual> population;
    private List<ControlledIndividual> globalTopIndividuals = new List<ControlledIndividual>();
    public List<ControlledIndividual> SavedPopulation;

    private const int GLOBAL_TOP_LIMIT = 20;
    public enum CrossoverType { OnePoint, Uniform }

    [Serializable]
    public class ControlledIndividualData
    {
        public int[] chromosome;
        public float fitness;
        public ControlledIndividualData(ControlledIndividual ind)
        {
            chromosome = (int[])ind.chromosome.Clone();
            fitness = ind.fitness;
        }
    }

    [Serializable]
    public class PopulationData { public List<ControlledIndividualData> individuals; }

    [Serializable]
    public class ControlledIndividual
    {
        public int[] chromosome;
        public float fitness;
        private int attackCount;

        public ControlledIndividual(int attackCount)
        {
            this.attackCount = attackCount;
            chromosome = new int[81];
            for (int i = 0; i < chromosome.Length; i++)
                chromosome[i] = UnityEngine.Random.Range(0, attackCount);
            fitness = 0f;
        }

        public ControlledIndividual Clone()
        {
            var copy = new ControlledIndividual(attackCount);
            chromosome.CopyTo(copy.chromosome, 0);
            copy.fitness = fitness;
            return copy;
        }
    }

    void Start()
    {
        if (isTrainingMode)
            InitializePopulation();
        else
            LoadPopulation();
    }

    public void InitializePopulation()
    {
        population = new List<ControlledIndividual>();
        for (int i = 0; i < populationSize; i++)
            population.Add(new ControlledIndividual(_player.Attacks.Length));
    }

    protected override void Think()
    {
        int stateIndex = StateToTable();
        ControlledIndividual currentInd = isPlayingMode ? SavedPopulation[demoIndex] : population[CurrentIndividualIndex];
        int attackIndex = currentInd.chromosome[stateIndex];

        if (_player.Energy < _player.Attacks[attackIndex].Energy)
            energyFailuresInMatch++;

        if (_attackToDo == null)
        {
            _attackToDo = ScriptableObject.CreateInstance<Attack>();
        }

        _attackToDo.AttackMade = _player.Attacks[attackIndex];
        _attackToDo.Source = _player;
        _attackToDo.Target = GameState.ListOfPlayers.Players[_player.EnemyId];
    }

    public void EvaluateFitness()
    {
        PlayerInfo enemy = GameState.ListOfPlayers.Players[_player.EnemyId];

        float damageDealt = enemy.InitialHP - enemy.HP;
        float damageTaken = _player.InitialHP - _player.HP;
        float matchFitness = (damageDealt * 5f) - (damageTaken * 2f);

        if (enemy.HP <= 0) matchFitness += 1000f;
        if (_player.HP <= 0) matchFitness -= 1000f;

        matchFitness -= (energyFailuresInMatch * 1500f);

        accumulatedFitness += matchFitness;
        currentMatchCount++;
        energyFailuresInMatch = 0;

        if (currentMatchCount >= matchesPerIndividual)
        {
            population[CurrentIndividualIndex].fitness = accumulatedFitness / matchesPerIndividual;
            TryAddToGlobalTop(population[CurrentIndividualIndex]);
            CurrentIndividualIndex++;
            currentMatchCount = 0;
            accumulatedFitness = 0f;
        }
    }

    public ControlledIndividual TournamentSelection(int TSize)
    {
        var tournament = new List<ControlledIndividual>();
        for (int i = 0; i < TSize; i++)
            tournament.Add(population[UnityEngine.Random.Range(0, population.Count)]);
        return tournament.OrderByDescending(ind => ind.fitness).First();
    }

    public void Mutate(ControlledIndividual child)
    {
        for (int i = 0; i < child.chromosome.Length; i++)
        {
            if (UnityEngine.Random.value < MutationRate)
                child.chromosome[i] = UnityEngine.Random.Range(0, _player.Attacks.Length);
        }
    }

    public ControlledIndividual Crossover(ControlledIndividual one, ControlledIndividual two)
    {
        var child = new ControlledIndividual(_player.Attacks.Length);
        if (crossoverType == CrossoverType.OnePoint)
        {
            int crossPoint = UnityEngine.Random.Range(0, child.chromosome.Length);
            for (int i = 0; i < child.chromosome.Length; i++)
                child.chromosome[i] = (i < crossPoint) ? one.chromosome[i] : two.chromosome[i];
        }
        else
        {
            for (int i = 0; i < child.chromosome.Length; i++)
                child.chromosome[i] = (UnityEngine.Random.value < 0.5f) ? two.chromosome[i] : one.chromosome[i];
        }
        return child;
    }

    public void NextGeneration()
    {
        List<ControlledIndividual> newPopulation = new List<ControlledIndividual>();
        var sorted = population.OrderByDescending(i => i.fitness).ToList();
        newPopulation.Add(sorted[0].Clone());
        newPopulation.Add(sorted[1].Clone());

        while (newPopulation.Count < populationSize)
        {
            var p1 = TournamentSelection(5);
            var p2 = TournamentSelection(5);
            var child = Crossover(p1, p2);
            Mutate(child);
            newPopulation.Add(child);
        }
        population = newPopulation;
        MutationRate = Mathf.Max(MutationRate * mutationDecayFactor, minMutationRate);
    }

    private int StateToTable()
    {
        float myHP = (float)_player.HP / _player.InitialHP;
        float myEnergy = (float)_player.Energy / _player.InitialEnergy;
        PlayerInfo enemyInfo = GameState.ListOfPlayers.Players[_player.EnemyId];
        float enemyHP = (float)enemyInfo.HP / enemyInfo.InitialHP;
        float enemyEnergy = (float)enemyInfo.Energy / enemyInfo.InitialEnergy;

        return LevelFromPercentage(myHP) * 27 + LevelFromPercentage(myEnergy) * 9 +
               LevelFromPercentage(enemyHP) * 3 + LevelFromPercentage(enemyEnergy);
    }

    private int LevelFromPercentage(float value)
    {
        if (value <= 0.25f) return 0;
        if (value <= 0.60f) return 1;
        return 2;
    }

    public void FinishTraining()
    {
        SavedPopulation = globalTopIndividuals.Select(ind => ind.Clone()).ToList();
        isTrainingMode = false;
        SavePopulation();
        Debug.Log("Entrenamiento Finalizado y Guardado");
    }

    public void SavePopulation()
    {
        PopulationData data = new PopulationData { individuals = globalTopIndividuals.Select(ind => new ControlledIndividualData(ind)).ToList() };
        File.WriteAllText(Application.persistentDataPath + "/population.json", JsonUtility.ToJson(data, true));
    }

    public void LoadPopulation()
    {
        string path = Application.persistentDataPath + "/population.json";
        if (!File.Exists(path)) return;
        PopulationData data = JsonUtility.FromJson<PopulationData>(File.ReadAllText(path));
        globalTopIndividuals = data.individuals.Select(d =>
        {
            var ind = new ControlledIndividual(_player.Attacks.Length) { chromosome = (int[])d.chromosome.Clone(), fitness = d.fitness };
            return ind;
        }).ToList();
        SavedPopulation = globalTopIndividuals.Select(ind => ind.Clone()).ToList();
    }

    private void TryAddToGlobalTop(ControlledIndividual individual)
    {
        globalTopIndividuals.Add(individual.Clone());
        globalTopIndividuals = globalTopIndividuals.OrderByDescending(ind => ind.fitness).ToList();
        if (globalTopIndividuals.Count > GLOBAL_TOP_LIMIT)
            globalTopIndividuals.RemoveAt(globalTopIndividuals.Count - 1);
    }
}