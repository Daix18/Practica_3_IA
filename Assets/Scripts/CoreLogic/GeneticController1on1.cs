//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;


//public class GeneticController1on1 : AIController
//{
//    public void Start()
//    {
//        Debug.Log("Start Genetic");
//    }


//    protected override void Think()
//    {
//        _attackToDo = ScriptableObject.CreateInstance<Attack>();
//        _attackToDo.AttackMade = _player.Attacks[0];
//        _attackToDo.Source = _player;
//        _attackToDo.Target = GameState.ListOfPlayers.Players[_player.EnemyId];

//    }
//}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;


//public class GeneticController1on1 : AIController
//{
//    public bool isTraining = false; // variable para controlar el modo de entrenamiento o juego
//    public int TrainingGenerations = 100;//numero de generaciones a entrenar.

//    //nos interesa que futuros entrenamientos comiencen con la mejor poblacion encontrada en el entrenamiento anterior, para que siga mejorando a lo largo del tiempo. Para eso guardamos el mejor individuo encontrado en una variable que se mantiene entre partidas.
//    private static List<ControlledIndividual> savedPopulation;


//    public ControlledIndividual CurrentIndividual;
//    public float MutationRate = 0.02f;


//    public List<ControlledIndividual> population; // lista de poblacion completa
//    public int populationSize = 20;                // tamaño de poblacion(lista)
//    public class ControlledIndividual
//    {
//        public int[] chromosome;
//        public float fitness;
//        private int attackCount; // guardar número de ataques

//        public ControlledIndividual(int attackCount)
//        {
//            this.attackCount = attackCount;
//            chromosome = new int[81]; // 81 estados posibles
//            for (int i = 0; i < chromosome.Length; i++)
//            {
//                chromosome[i] = UnityEngine.Random.Range(0, attackCount);
//            }
//            fitness = 0f;
//        }

//        public ControlledIndividual Clone()
//        {
//            var copy = new ControlledIndividual(attackCount); // usar el número correcto de ataques
//            chromosome.CopyTo(copy.chromosome, 0);
//            copy.fitness = fitness;
//            return copy;
//        }
//    }



//    // Inicializa población de manera aleatoria . Cada individuo es una estrategia completa para todos los estados posibles del juego (81 estados). Cada gen del cromosoma representa la acción a tomar en un estado específico.
//    public void InitializePopulation()
//    {
//        population = new List<ControlledIndividual>();
//        for (int i = 0; i < populationSize; i++)
//            population.Add(new ControlledIndividual(_player.Attacks.Length));
//    }

//    public void NextGeneration()
//    {
//        List<ControlledIndividual> newPopulation = new List<ControlledIndividual>();

//        //  conservar el mejor de la generación actual (elitismo)
//        var best = population.OrderByDescending(i => i.fitness).First().Clone();
//        newPopulation.Add(best);

//        while (newPopulation.Count < populationSize)
//        {
//            var parent1 = TournamentSelection(3);
//            var parent2 = TournamentSelection(3);

//            var child = Crossover(parent1, parent2);
//            Mutate(child);

//            newPopulation.Add(child);
//        }

//        population = newPopulation;
//    }



//    void Start()
//    {

//        if (isTraining)
//        {
//            Debug.Log("Modo aprendizaje activado");

//            // PASO CLAVE: Si ya teníamos una población guardada, la usamos
//            if (savedPopulation != null && savedPopulation.Count > 0)
//            {
//                population = savedPopulation;
//                // RESETEAR FITNESS para que la "suerte" del Play anterior no afecte al nuevo
//                //foreach (var ind in population) ind.fitness = 0;
//                Debug.Log("Continuando entrenamiento con la población previa...");
//            }
//            else
//            {
//                InitializePopulation(); // Si es la primera vez, crea 20 aleatorios
//                Debug.Log("Iniciando nueva población desde cero...");
//            }

//            Train(TrainingGenerations);

//            // Al terminar el entrenamiento, actualizamos el almacén estático
//            savedPopulation = new List<ControlledIndividual>(population);

//            CurrentIndividual = population.OrderByDescending(i => i.fitness).First();
//            Debug.Log($"Entrenamiento finalizado. Mejor Fitness actual: {CurrentIndividual.fitness}");
//        }
//        else
//        {
//            Debug.Log("Modo juego activado");

//            // Usamos directamente el mejor individuo de la población entrenada
//            if (population != null && population.Count > 0)
//            {
//                CurrentIndividual = savedPopulation.OrderByDescending(ind => ind.fitness).First();
//            }
//            else
//            {
//                // Primera partida: jugamos aleatorio si no hay población
//                CurrentIndividual = new ControlledIndividual(_player.Attacks.Length);
//            }
//        }
//    }

//    protected override void Think()
//    {
//        //obtengo el indice de la tabla a partir del estado actual del juego 0-81
//        int stateIndex = StateToTable();

//        //obtengo el ataque a hacer a partir del cromosoma del individuo actual para ese estado valor q guarda la posicion  stateIndex  del gen para ese individuo
//        int attackIndex = CurrentIndividual.chromosome[stateIndex];

//        //todo lo relacionado a ejecutar el ataque seleccionado
//        _attackToDo = ScriptableObject.CreateInstance<Attack>();
//        _attackToDo.AttackMade = _player.Attacks[attackIndex];
//        _attackToDo.Source = _player;
//        _attackToDo.Target = GameState.ListOfPlayers.Players[_player.EnemyId];
//    }

//    //PARA PARTIDA REAL
//    public void EvaluateFitness()
//    {
//        PlayerInfo enemy = GameState.ListOfPlayers.Players[_player.EnemyId];
//        CurrentIndividual.fitness = _player.HP - enemy.HP;
//        if (enemy.HP <= 0) CurrentIndividual.fitness += 1000; // bonus por ganar// premio grande por victoria. ganar es mejor  q nada
//    }

//    //PARA SIMULACION
//    public void EvaluateFitness(LogicState state, ControlledIndividual individual)
//    {

//        float myHP = state.HitPoints[_player.Id];
//        float enemyHP = state.HitPoints[_player.EnemyId];

//        float fitness = 0f;

//        // 🔹 Diferencia de vida (base)
//        fitness += (myHP - enemyHP) * 2f;

//        // 🔹 Gran recompensa por victoria
//        if (enemyHP <= 0)
//            fitness += 1000f;

//        // 🔹 Gran penalización por derrota
//        if (myHP <= 0)
//            fitness -= 1000f;

//        //// 🔹 Penalizar partidas largas (más eficiente = mejor)
//        //fitness -= state.Ply * 0.5f;

//        // 🔹 Recompensar conservar vida
//        fitness += myHP * 0.05f;

//        // 🔹 Penalizar dejar al enemigo con mucha vida
//        fitness -= enemyHP * 0.5f;

//        // bonus por haber hecho mucho daño total
//        float totalDamageDone = _player.InitialHP - enemyHP;
//        fitness += totalDamageDone * 0.5f;

//        individual.fitness = fitness;



//    }

//    //SELECCION
//    public ControlledIndividual TournamentSelection(int TSize)
//    {
//        var tournament = new List<ControlledIndividual>();
//        for (int i = 0; i < TSize; i++)
//        {
//            tournament.Add(population[UnityEngine.Random.Range(0, population.Count)]);
//        }

//        return tournament.OrderByDescending(ind => ind.fitness).First();
//    }
//    //MUTACION
//    public void Mutate(ControlledIndividual child) //cambia aleatoriamente algunos genes del cromosoma con cierta probabilidad
//    {
//        for (int i = 0; i < child.chromosome.Length; i++)
//        {

//            if (UnityEngine.Random.value < MutationRate)//probabilidad de mutar cada gen
//            {
//                int newGen;
//                do
//                {
//                    newGen = UnityEngine.Random.Range(0, _player.Attacks.Length);//genera un nuevo gen aleatorio
//                } while (newGen == child.chromosome[i]); // aseguramos que el nuevo gen sea diferente al actual. vslor generado != al valor actual del gen

//                child.chromosome[i] = newGen; // asignamos el nuevo gen al cromosoma
//            }

//        }
//    }

//    //CRUCE (cambiar funcion)
//    public ControlledIndividual Crossover(ControlledIndividual one, ControlledIndividual two)
//    {
//        var child = one.Clone();
//        int crossPoint = UnityEngine.Random.Range(0, child.chromosome.Length); //punto desde el cual los genes cambian del padre 1 al 2 ej 3 primeros del 1 siguientes del 2
//        for (int i = crossPoint; i < child.chromosome.Length; i++)
//        {
//            if (UnityEngine.Random.value < 0.5f) //con probabilidad 50% se elige el gen del padre 2, sino se mantiene el del padre 1. como cada gen es distinto son 81 estados independientes y no estan relacionados.
//            {
//                child.chromosome[i] = two.chromosome[i];
//                //AAAAAAAAAAA -> padre 1
//                //BBBBBBBBBBB -> padre 2
//                //ABBAABBAAAB -> hijo (con cruces aleatorios)
//            }
//        }

//        return child;
//    }


//    //ENTRENAMIENTO --> suavizar azar haciendo q un individuo juegue varias partidas y promediando su fitness, o usando la mediana, etc. para que no dependa tanto de la suerte de una partida. Tambien se puede entrenar contra un oponente fijo (como Minimax) para que el aprendizaje sea más consistente.
//    public void Train(int generations)
//    {
//        // CAMBIO CLAVE: Solo inicializamos si NO hay una población guardada
//        if (population == null || population.Count == 0)
//        {
//            InitializePopulation();
//            Debug.Log("Creando población inicial aleatoria...");
//        }
//        else
//        {
//            Debug.Log("Entrenando sobre la población existente...");
//        }

//        for (int g = 0; g < generations; g++)
//        {
//            // Debug.Log($"Generación {g + 1}"); // Opcional: puede llenar mucho la consola

//            foreach (var individual in population)
//            {
//                CurrentIndividual = individual;
//                float totalFitness = 0f;//NUEVO
//                for (int ss = 0; ss < 3; ss++) // NUEVO: simular varias veces por individuo para suavizar el azar
//                {
//                    SimulateGame(CurrentIndividual);
//                    totalFitness += individual.fitness;
//                }
//                individual.fitness = totalFitness / 3f; // Promediamos el fitness de las simulaciones
//            }

//            float bestFitness = population.Max(i => i.fitness);
//            Debug.Log($"Simulacion de Generación {g}: Mejor Fitness encontrado = {bestFitness}");

//            NextGeneration();
//        }

//        // Al final, guardamos al mejor
//        CurrentIndividual = population.OrderByDescending(ind => ind.fitness).First();
//    }


//    private void SimulateGame(ControlledIndividual individual)
//    {
//        LogicState virtualState = new LogicState(GameState);

//        int maxTurns = 200;
//        int counter = 0;

//        while (!Suspend(virtualState) && counter < maxTurns)
//        {
//            AttackInfo selectedAttack;


//            if (virtualState.PlayerIdxTurn == _player.Id)
//            {
//                int stateIndex = GetStateIndexFromLogic(virtualState);
//                int attackIndex = individual.chromosome[stateIndex];
//                selectedAttack = _player.Attacks[attackIndex];

//                // ✅ Verificar energía antes de ejecutar
//                if (virtualState.Energies[_player.Id] < selectedAttack.Energy)
//                {
//                    var possibleAttacks = _player.Attacks
//                        .Where(a => virtualState.Energies[_player.Id] >= a.Energy)
//                        .ToList();

//                    if (possibleAttacks.Count > 0)
//                        selectedAttack = possibleAttacks[UnityEngine.Random.Range(0, possibleAttacks.Count)];
//                    else
//                        return; // no puede atacar, termina simulación
//                }

//                var children = virtualState.GenerateChildrenProb(_player.Id, selectedAttack);

//                // Tomar un resultado al azar según probabilidad
//                float r = UnityEngine.Random.value;
//                float acc = 0f;
//                foreach (var (att, childState) in children)
//                {
//                    acc += childState.Probability;
//                    if (r <= acc)
//                    {
//                        virtualState = childState; // sustituimos el estado virtual
//                        break;
//                    }
//                }
//            }
//            else
//            {
//                // Jugador enemigo usa Minimax
//                selectedAttack = GetBestMinimaxAttack(virtualState);

//                // 🔴 VERIFICACIÓN DE ENERGÍA AQUÍ
//                int enemyId = _player.EnemyId; // Usamos el ID del enemigo ya conocido

//                if (virtualState.Energies[enemyId] < selectedAttack.Energy)
//                {
//                    var enemyAttacks = GameState.ListOfPlayers.Players[enemyId].Attacks;
//                    // Buscar primer ataque válido
//                    selectedAttack = enemyAttacks
//                        .FirstOrDefault(a => virtualState.Energies[enemyId] >= a.Energy);

//                    // Si no puede atacar, pasar turno (estado sin cambios)
//                    if (selectedAttack == null)
//                    {
//                        virtualState = new LogicState(virtualState,
//                            new float[virtualState.NumPlayers],
//                            new float[virtualState.NumPlayers]);

//                        continue;
//                    }
//                }
//                var children = virtualState.GenerateChildrenProb(virtualState.PlayerIdxTurn, selectedAttack);
//                float r = UnityEngine.Random.value;
//                float acc = 0f;
//                foreach (var (att, childState) in children)
//                {
//                    acc += childState.Probability;
//                    if (r <= acc)
//                    {
//                        virtualState = childState;
//                        break;
//                    }
//                }
//                if (virtualState == null)
//                    virtualState = children.Last().Item2;
//            }

//            counter++;
//        }

//        EvaluateFitness(virtualState, individual);
//    }


//    private AttackInfo GetBestMinimaxAttack(LogicState state)
//    {
//        float bestVal = -100f;
//        AttackInfo bestAtt = null;

//        // 1. Identificar quién es el enemigo en este turno virtual
//        // Usamos el ID del enemigo configurado en el player real
//        int enemyId = _player.EnemyId;

//        // 2. Seguridad: Validar que el índice exista en el estado actual
//        if (enemyId >= state.NumPlayers) enemyId = 1; // Fallback seguro

//        var enemyInfo = GameState.ListOfPlayers.Players[enemyId];

//        // 3. Generar hijos pasando el ID validado
//        var children = state.GenerateChildren(enemyId, enemyInfo.Attacks);

//        foreach (var (att, childState) in children)
//        {
//            float val = RandomValor(childState, att);

//            if (val > bestVal)
//            {
//                bestVal = val;
//                bestAtt = att;
//            }
//        }

//        return bestAtt ?? enemyInfo.Attacks[0];
//    }



//    //Función de discretización : convierte un valor continuo (como el hp o la energía) en un valor discreto (como "bajo", "medio" o "alto"). Esto es útil para simplificar el espacio de estados y hacer que el algoritmo genético funcione mejor.
//    private int LevelFromPercentage(float value)
//    {
//        if (value <= 0.33f) return 0;   // Bajo
//        if (value <= 0.66f) return 1;   // Medio
//        return 2;                        // Alto
//    }



//    //    //FUNCIONES AUXILIARES
//    //tomar valores mios y del enemigo, discretizar y convertirlos en indices de tabla
//    private int StateToTable()
//    {
//        //valores
//        float myHP = _player.HP / _player.InitialHP;
//        float myEnergy = _player.Energy / _player.InitialEnergy;

//        PlayerInfo enemyInfo = GameState.ListOfPlayers.Players[_player.EnemyId];
//        float enemyHP = enemyInfo.HP / enemyInfo.InitialHP;
//        float enemyEnergy = enemyInfo.Energy / enemyInfo.InitialEnergy;

//        //discretizar
//        int myHPLevel = LevelFromPercentage(myHP);
//        int myEnergyLevel = LevelFromPercentage(myEnergy);
//        int enemyHPLevel = LevelFromPercentage(enemyHP);
//        int enemyEnergyLevel = LevelFromPercentage(enemyEnergy);

//        //convertir a indice
//        int StateIndex = myHPLevel * 27 + myEnergyLevel * 9 + enemyHPLevel * 3 + enemyEnergyLevel; //
//        return StateIndex;
//    }

//    private int GetStateIndexFromLogic(LogicState state)
//    {
//        // Sacamos los porcentajes de la simulación virtual, no del objeto real
//        float myHP = state.HitPoints[_player.Id] / _player.InitialHP;
//        float myEnergy = state.Energies[_player.Id] / _player.InitialEnergy;

//        // Datos del enemigo en la simulación
//        PlayerInfo enemyInfo = GameState.ListOfPlayers.Players[_player.EnemyId];
//        float enemyHP = state.HitPoints[_player.EnemyId] / enemyInfo.InitialHP;
//        float enemyEnergy = state.Energies[_player.EnemyId] / enemyInfo.InitialEnergy;

//        // Reutilizamos tu función LevelFromPercentage
//        int myHPLevel = LevelFromPercentage(myHP);
//        int myEnergyLevel = LevelFromPercentage(myEnergy);
//        int enemyHPLevel = LevelFromPercentage(enemyHP);
//        int enemyEnergyLevel = LevelFromPercentage(enemyEnergy);

//        // El mismo índice de 81 estados que definiste
//        return myHPLevel * 27 + myEnergyLevel * 9 + enemyHPLevel * 3 + enemyEnergyLevel;
//    }


//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GeneticController1on1 : AIController
{
    public float matchfitness = 0f;
    public int generationsPerSession = 50;
    public int totalSessions = 10;
    public bool isTrainingMode = true;
    public int currentGeneration = 0;
    public int currentSession = 0;

    public int CurrentIndividualIndex;
    public float MutationRate = 0.02f;
    public float minMutationRate = 0.001f;
    public float mutationDecayFactor = 0.95f;

    public List<ControlledIndividual> population;
    public int populationSize = 20;
    public CrossoverType crossoverType;

    public List<GeneticController1on1.ControlledIndividual> BestIndividualsPerMatch = new List<GeneticController1on1.ControlledIndividual>();
    public List<GeneticController1on1.ControlledIndividual> SavedPopulation;

    public enum CrossoverType
    {
        OnePoint,
        Uniform
    }

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
            {
                chromosome[i] = UnityEngine.Random.Range(0, attackCount);
            }
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

    public void InitializePopulation()
    {
        population = new List<ControlledIndividual>();
        for (int i = 0; i < populationSize; i++)
            population.Add(new ControlledIndividual(_player.Attacks.Length));
    }

    public void NextGeneration()
    {
        List<ControlledIndividual> newPopulation = new List<ControlledIndividual>();

        var best = population.OrderByDescending(i => i.fitness).First().Clone();
        newPopulation.Add(best);

        while (newPopulation.Count < populationSize)
        {
            var parent1 = TournamentSelection(3);
            var parent2 = TournamentSelection(3);

            var child = Crossover(parent1, parent2);
            Mutate(child);

            newPopulation.Add(child);
        }

        population = newPopulation;

        MutationRate *= mutationDecayFactor;
        if (MutationRate < minMutationRate)
            MutationRate = minMutationRate;
    }

    void Start()
    {
        if (isTrainingMode)
        {
            InitializePopulation();
            MutationRate = 0.02f;
        }
        else
        {
            if (SavedPopulation != null && SavedPopulation.Count > 0)
            {
                population = SavedPopulation.Select(ind => ind.Clone()).ToList();
                MutationRate = 0f;
                Debug.Log("IA cargada en modo juego con la mejor población guardada.");
            }
            else
            {
                Debug.Log("No hay población guardada.");
            }
        }
    }

    protected override void Think()
    {
        int stateIndex = StateToTable();
        ControlledIndividual currentInd = population[CurrentIndividualIndex];

        int attackIndex = currentInd.chromosome[stateIndex];

        _attackToDo = ScriptableObject.CreateInstance<Attack>();
        _attackToDo.AttackMade = _player.Attacks[attackIndex];
        _attackToDo.Source = _player;
        _attackToDo.Target = GameState.ListOfPlayers.Players[_player.EnemyId];

        Debug.Log($"IA Genética - Estado: {stateIndex} | Acción elegida (Gen): {attackIndex}");
    }

    public void EvaluateFitness()
    {
        PlayerInfo enemy = GameState.ListOfPlayers.Players[_player.EnemyId];

        matchfitness = _player.HP - enemy.HP;

        if (enemy.HP <= 0) matchfitness += 1000;
        if (_player.HP <= 0) matchfitness -= 1000;

        matchfitness += _player.HP * 0.05f;
        matchfitness -= enemy.HP * 0.5f;

        population[CurrentIndividualIndex].fitness = matchfitness;
    }

    public ControlledIndividual TournamentSelection(int TSize)
    {
        var tournament = new List<ControlledIndividual>();
        for (int i = 0; i < TSize; i++)
        {
            tournament.Add(population[UnityEngine.Random.Range(0, population.Count)]);
        }

        return tournament.OrderByDescending(ind => ind.fitness).First();
    }

    public void Mutate(ControlledIndividual child)
    {
        for (int i = 0; i < child.chromosome.Length; i++)
        {
            if (UnityEngine.Random.value < MutationRate)
            {
                int newGen;
                do
                {
                    newGen = UnityEngine.Random.Range(0, _player.Attacks.Length);
                } while (newGen == child.chromosome[i]);

                child.chromosome[i] = newGen;
            }
        }
    }

    public ControlledIndividual Crossover(ControlledIndividual one, ControlledIndividual two)
    {
        var child = new ControlledIndividual(_player.Attacks.Length);
        if (crossoverType == CrossoverType.OnePoint)
        {
            int crossPoint = UnityEngine.Random.Range(0, child.chromosome.Length);

            for (int i = 0; i < child.chromosome.Length; i++)
            {
                child.chromosome[i] = (i < crossPoint) ? one.chromosome[i] : two.chromosome[i];
            }
        }
        else
        {
            for (int i = 0; i < child.chromosome.Length; i++)
            {
                if (UnityEngine.Random.value < 0.5f)
                {
                    child.chromosome[i] = two.chromosome[i];
                }
                else
                {
                    child.chromosome[i] = one.chromosome[i];
                }
            }
        }

        return child;
    }

    private int LevelFromPercentage(float value)
    {
        if (value <= 0.33f) return 0;
        if (value <= 0.66f) return 1;
        return 2;
    }

    private int StateToTable()
    {
        float myHP = (float)_player.HP / _player.InitialHP;
        float myEnergy = (float)_player.Energy / _player.InitialEnergy;

        PlayerInfo enemyInfo = GameState.ListOfPlayers.Players[_player.EnemyId];
        float enemyHP = (float)enemyInfo.HP / enemyInfo.InitialHP;
        float enemyEnergy = (float)enemyInfo.Energy / enemyInfo.InitialEnergy;

        int myHPLevel = LevelFromPercentage(myHP);
        int myEnergyLevel = LevelFromPercentage(myEnergy);
        int enemyHPLevel = LevelFromPercentage(enemyHP);
        int enemyEnergyLevel = LevelFromPercentage(enemyEnergy);

        int StateIndex = myHPLevel * 27 + myEnergyLevel * 9 + enemyHPLevel * 3 + enemyEnergyLevel;
        return StateIndex;
    }

    public void SaveBestOfCurrentSession()
    {
        var best = population
            .OrderByDescending(ind => ind.fitness)
            .First()
            .Clone();

        BestIndividualsPerMatch.Add(best);

        Debug.Log("Mejor individuo de la sesión guardado con fitness: " + best.fitness);
    }

    public void FinishTraining()
    {
        SavedPopulation = BestIndividualsPerMatch
            .OrderByDescending(ind => ind.fitness)
            .Take(populationSize)
            .Select(ind => ind.Clone())
            .ToList();

        isTrainingMode = false;
        Debug.Log("ENTRENAMIENTO FINALIZADO. IA lista para jugar con la población guardada.");
    }

    public void BestFitnessDuringTraining()
    {
        Debug.Log("🏆 Mejores individuos por sesión:");

        for (int i = 0; i < BestIndividualsPerMatch.Count; i++)
        {
            var ind = BestIndividualsPerMatch[i];
            Debug.Log($"Sesión {i + 1}: Fitness = {ind.fitness},]");
        }
    }
}