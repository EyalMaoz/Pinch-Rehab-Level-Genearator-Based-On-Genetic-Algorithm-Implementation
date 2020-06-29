using System;
using System.Collections.Generic;
using System.Linq;

namespace GetenicAlgorithmForLevelsVR
{
    public class Program
    {
        public static Random RandGenerator = new Random();
        public const int _DifficultyMax = 10;
        public const int _DifficultyMin = 1;
        // Inits:
        public const int dpT2 = 2;
        public const int dpP2 = 3;
        public const int dpT3 = 2;
        public const int dpP3 = 3;
        public const int dpL2 = 5;

        public enum PinchType
        {
            T2 = 0, T3 = 1, P2 = 2, P3 = 3, L2 = 4
        }

        public class Action
        {
            public PinchType Type;
            public int DifficultyLevel;// dm
            public int DifficultyPerPlayer;// dp
            public Action(PinchType t, int dp, int diffScore = -1)
            {
                Type = t;
                DifficultyPerPlayer = dp;
                if (diffScore == -1) DifficultyLevel = Program.RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);
                else DifficultyLevel = diffScore;
            }
        }

        public class Challenge
        {
            public List<Action> ActionsList = null;
            public string Name;
            public Challenge Source = null;
            public float LF = 0f;

            private int version = 1;

            public Challenge(string n, List<Action> Actions = null)
            {
                if (Actions == null)
                {
                    ActionsList = new List<Action>();
                }
                else ActionsList = Actions;
                Name = n;
            }

            public Challenge(Challenge original)
            {
                ActionsList = new List<Action>();
                foreach (Action a in original.ActionsList)
                {
                    ActionsList.Add(new Action(a.Type, a.DifficultyPerPlayer, a.DifficultyLevel));
                }
                version = original.version + 1;
                Name = original.Name;
                LF = original.LF;
                if (original.Source == null)
                {
                    this.Source = original;
                }
                else
                {
                    this.Source = original.Source;
                }
            }

            public float GetChallengeDifficulty()
            {
                float answer = 0;
                foreach (Action action in ActionsList)
                {
                    answer += action.DifficultyLevel + action.DifficultyPerPlayer;
                }
                return answer;
            }

            public int CalculateCR(int PT, int ST)
            {
                int RL = (PT + 2) - (ST + 1);
                int PR = 2 * RL - 1;
                return PT - PR;
            }

            public void PrintActions()
            {
                Console.WriteLine(Name + "_Ver" + version);
                Console.Write("{ ");
                foreach (Action a in ActionsList)
                {
                    Console.Write(a.Type.ToString() + "[" + a.DifficultyLevel + "] ");
                }
                Console.WriteLine("}");
            }

            public void AddRandomAction(int Difficulty = -1)
            {
                PinchType newActType = (PinchType)(Program.RandGenerator.Next(0, 4 + 1));
                if (Difficulty == -1)
                {
                    Difficulty = Program.RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);
                }
                switch (newActType)
                {
                    case PinchType.T2:
                        ActionsList.Add(new Action(newActType, dpT2, Difficulty));
                        break;
                    case PinchType.P2:
                        ActionsList.Add(new Action(newActType, dpP2, Difficulty));
                        break;
                    case PinchType.T3:
                        ActionsList.Add(new Action(newActType, dpT3, Difficulty));
                        break;
                    case PinchType.P3:
                        ActionsList.Add(new Action(newActType, dpP3, Difficulty));
                        break;
                    case PinchType.L2:
                        ActionsList.Add(new Action(newActType, dpL2, Difficulty));
                        break;
                }
            }

            public bool LowerDifficulty()
            {
                for (int i = ActionsList.Count - 1; i >= 0; i--)
                {
                    if (ActionsList[i].DifficultyLevel > _DifficultyMin)
                    {
                        ActionsList[i].DifficultyLevel -= 1;
                        return true;
                    }
                }
                return false;
            }

            public bool HigherDifficulty()
            {
                for (int i = ActionsList.Count - 1; i >= 0; i--)
                {
                    if (ActionsList[i].DifficultyLevel < _DifficultyMax)
                    {
                        ActionsList[i].DifficultyLevel += 1;
                        return true;
                    }
                }
                return false;
            }

            public void ReplaceWithRandomAction()
            {
                int actionIndex = (Program.RandGenerator.Next(0, ActionsList.Count));
                int newActionIndex = (Program.RandGenerator.Next(0, 4 + 1));
                while (actionIndex == newActionIndex)
                {
                    newActionIndex = (Program.RandGenerator.Next(0, 4 + 1));
                }

                ActionsList.RemoveAt(actionIndex);
                PinchType newActType = (PinchType)newActionIndex;
                int Difficulty = Program.RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);

                switch (newActType)
                {
                    case PinchType.T2:
                        ActionsList.Add(new Action(newActType, dpT2, Difficulty));
                        break;
                    case PinchType.P2:
                        ActionsList.Add(new Action(newActType, dpP2, Difficulty));
                        break;
                    case PinchType.T3:
                        ActionsList.Add(new Action(newActType, dpT3, Difficulty));
                        break;
                    case PinchType.P3:
                        ActionsList.Add(new Action(newActType, dpP3, Difficulty));
                        break;
                    case PinchType.L2:
                        ActionsList.Add(new Action(newActType, dpL2, Difficulty));
                        break;
                }
            }
        }

        public static class Algorithm
        {
            const float CL = 1f;// Coefficient of Learning
            const long StuckRefreshMaxCount = 3;// Coefficient of Learning
            static long stuckCounter = 0;

            public static Challenge CreateNewChallenge(Challenge original, int PT, int ST)
            {
                Challenge newChal;
                if (original.Source == null)
                    newChal = new Challenge(original);
                else
                    newChal = new Challenge(original.Source);

                bool shouldCheckCR = true;
                int CR = original.CalculateCR(PT, ST);

                while (true)
                {

                    if (shouldCheckCR)// depends if we get back to this step or only the next step
                    {
                        if (CR >= 0)
                        {
                            newChal.AddRandomAction();
                        }
                        else
                        {
                            //newChal.LowerDifficulty();
                            if (!newChal.LowerDifficulty())
                            {// All the actions are at the minimum difficulty
                                newChal.AddRandomAction(_DifficultyMin);
                            }
                        }
                    }

                    float Hdif = newChal.GetChallengeDifficulty() - original.GetChallengeDifficulty();
                    newChal.LF = CL * CR;

                    if (Hdif < newChal.LF)
                    {
                        if (!newChal.HigherDifficulty())
                        {
                            shouldCheckCR = true;
                        }
                        else
                            shouldCheckCR = false;
                        //shouldCheckCR = true;
                        //continue;
                    }
                    else if (Hdif > newChal.LF)
                    {
                        if (!newChal.LowerDifficulty())
                        {
                            stuckCounter++;
                            if (stuckCounter == StuckRefreshMaxCount)
                            {
                                stuckCounter = 0;
                                newChal.ReplaceWithRandomAction();
                            }
                            break;
                        }
                        else shouldCheckCR = false;
                    }
                    else if (Hdif == newChal.LF) break;
                }
                //newChal.LF /= 10;
                return newChal;
            }

            public static List<Challenge> SinglePointCrossover(Challenge c1, Challenge c2)
            {
                int crossPoint = (Program.RandGenerator.Next(1, Math.Min(c1.ActionsList.Count, c2.ActionsList.Count)));
                Challenge offspring1 = new Challenge(c1.Name);
                Challenge offspring2 = new Challenge(c2.Name);

                for (int i = 0; i < crossPoint; i++)
                {
                    offspring1.ActionsList.Add(new Action(c1.ActionsList[i].Type, c1.ActionsList[i].DifficultyPerPlayer, c1.ActionsList[i].DifficultyLevel));
                    offspring2.ActionsList.Add(new Action(c2.ActionsList[i].Type, c2.ActionsList[i].DifficultyPerPlayer, c2.ActionsList[i].DifficultyLevel));
                }

                for (int i = crossPoint; i < c1.ActionsList.Count; i++)
                {
                    offspring2.ActionsList.Add(new Action(c1.ActionsList[i].Type, c1.ActionsList[i].DifficultyPerPlayer, c1.ActionsList[i].DifficultyLevel));
                }

                for (int i = crossPoint; i < c2.ActionsList.Count; i++)
                {
                    offspring1.ActionsList.Add(new Action(c2.ActionsList[i].Type, c2.ActionsList[i].DifficultyPerPlayer, c2.ActionsList[i].DifficultyLevel));
                }


                return new List<Challenge>() { offspring1, offspring2 };
            }

            public static List<Challenge> SelectChromForNextLevel(List<Challenge> chromosomePool)
            {
                List<Challenge> theChosenOnes = new List<Challenge>();
                Dictionary<float, Challenge> challengeDictionary = new Dictionary<float, Challenge>();
                float maxDifficulty = 0;
                for (int i = 0; i < chromosomePool.Count; i++)
                {
                    if (chromosomePool[i].GetChallengeDifficulty() > maxDifficulty)
                        maxDifficulty = chromosomePool[i].GetChallengeDifficulty();
                }

                for (int i = 0; i < chromosomePool.Count; i++)
                {
                    float diff = Math.Abs(chromosomePool[i].GetChallengeDifficulty() / maxDifficulty - chromosomePool[i].LF / 10);
                    while (challengeDictionary.ContainsKey(diff)) diff += 0.0001f;
                    challengeDictionary.Add(Math.Abs(diff), chromosomePool[i]);
                }

                var l = challengeDictionary.OrderBy(key => key.Key);
                var sortedDictionary = l.ToDictionary((keyItem) => keyItem.Key, (valueItem) => valueItem.Value);

                for (int i = 0; i < challengeDictionary.Count / 2; i++)
                {
                    theChosenOnes.Add(sortedDictionary.ElementAt(i).Value);
                }

                return theChosenOnes;
            }
        }

        static void Main()
        {
            long counter = 1;
            // Creating Challenges:
            Challenge Challenge1 = new Challenge("Challenge1", new List<Action>()
            {
                new Action(PinchType.T2, dpT2),
                new Action(PinchType.T2, dpT2)

            });

            Challenge Challenge2 = new Challenge("Challenge2", new List<Action>()
            {
                new Action(PinchType.T3, dpT3),
                new Action(PinchType.T3, dpT3)

            });
            Challenge Challenge3 = new Challenge("Challenge3", new List<Action>()
            {
                new Action(PinchType.P2, dpP2),
                new Action(PinchType.P2, dpP2)

            });

            Challenge Challenge4 = new Challenge("Challenge4", new List<Action>()
            {
                new Action(PinchType.P3, dpP3),
                new Action(PinchType.P3, dpP3)

            });

            Challenge1.PrintActions();
            Challenge2.PrintActions();
            Challenge3.PrintActions();
            Challenge4.PrintActions();
            // Create new challenges:

            Challenge new1Chal = Challenge1;
            Challenge new2Chal = Challenge2;
            Challenge new3Chal = Challenge3;
            Challenge new4Chal = Challenge4;

            string stop = string.Empty;
            while (stop != "s")
            {
                // Second Round
                Console.WriteLine("\nRound " + counter++ + "\n");

                new1Chal = Algorithm.CreateNewChallenge(new1Chal, 10, 1);
                new2Chal = Algorithm.CreateNewChallenge(new2Chal, 10, 3);
                new3Chal = Algorithm.CreateNewChallenge(new3Chal, 10, 6);
                new4Chal = Algorithm.CreateNewChallenge(new4Chal, 10, 10);

                new1Chal.PrintActions();
                new2Chal.PrintActions();
                new3Chal.PrintActions();
                new4Chal.PrintActions();
                List<Challenge> chromosomePool = Algorithm.SinglePointCrossover(new1Chal, new2Chal);// Maybe create as global "Chromosome Pool", for better selection inorder to do better CrossOver
                chromosomePool.AddRange(Algorithm.SinglePointCrossover(new4Chal, new3Chal));

                chromosomePool.Add(new1Chal);
                chromosomePool.Add(new2Chal);
                chromosomePool.Add(new3Chal);
                chromosomePool.Add(new4Chal);

                List<Challenge> nextLevel = Algorithm.SelectChromForNextLevel(chromosomePool);
                new1Chal = nextLevel[0];
                new2Chal = nextLevel[1];
                new3Chal = nextLevel[2];
                new4Chal = nextLevel[3];
                Console.WriteLine("\nAfter Crossover and Selection:");

                new1Chal.PrintActions();
                new2Chal.PrintActions();
                new3Chal.PrintActions();
                new4Chal.PrintActions();

                stop = Console.ReadLine();
            }
        }
    }
}
