using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public enum ActionType
        {
            T2 = 0, T3 = 1, P2 = 2, P3 = 3, L2 = 4
        }

        public class Action
        {
            public ActionType Type;
            public int DifficultyLevel;// dm
            public int DifficultyPerPlayer;// dp
            public Action(ActionType t, int dp, int diffScore = -1)
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

            private int version = 1;

            public Challenge(List<Action> Actions = null, string n = "Challenge")
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
                if (original.Source == null)
                {
                    this.Source = original;
                }
                else
                {
                    this.Source = original.Source;
                }
            }

            public int GetChallengeDifficulty()
            {
                int answer = 0;
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
                ActionType newActType = (ActionType)(Program.RandGenerator.Next(0, 4 + 1));
                if (Difficulty == -1)
                {
                    Difficulty = Program.RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);
                }
                switch (newActType)
                {
                    case ActionType.T2:
                        ActionsList.Add(new Action(newActType, dpT2, Difficulty));
                        break;
                    case ActionType.P2:
                        ActionsList.Add(new Action(newActType, dpP2, Difficulty));
                        break;
                    case ActionType.T3:
                        ActionsList.Add(new Action(newActType, dpT3, Difficulty));
                        break;
                    case ActionType.P3:
                        ActionsList.Add(new Action(newActType, dpP3, Difficulty));
                        break;
                    case ActionType.L2:
                        ActionsList.Add(new Action(newActType, dpL2, Difficulty));
                        break;
                }
            }

            public bool LowerDifficulty()
            {
                for (int i = ActionsList.Count - 1; i >= 0; i--)
                {
                    if (ActionsList[i].DifficultyLevel != _DifficultyMin)
                    {
                        ActionsList[i].DifficultyLevel--;
                        return true;
                    }
                }
                return false;
            }

            public bool HigherDifficulty()
            {
                for (int i = ActionsList.Count - 1; i >= 0; i--)
                {
                    if (ActionsList[i].DifficultyLevel != _DifficultyMax)
                    {
                        ActionsList[i].DifficultyLevel++;
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
                ActionType newActType = (ActionType)newActionIndex;
                int Difficulty = Program.RandGenerator.Next(_DifficultyMin, _DifficultyMax + 1);

                switch (newActType)
                {
                    case ActionType.T2:
                        ActionsList.Add(new Action(newActType, dpT2, Difficulty));
                        break;
                    case ActionType.P2:
                        ActionsList.Add(new Action(newActType, dpP2, Difficulty));
                        break;
                    case ActionType.T3:
                        ActionsList.Add(new Action(newActType, dpT3, Difficulty));
                        break;
                    case ActionType.P3:
                        ActionsList.Add(new Action(newActType, dpP3, Difficulty));
                        break;
                    case ActionType.L2:
                        ActionsList.Add(new Action(newActType, dpL2, Difficulty));
                        break;
                }
            }
        }

        public static class Algorithm
        {
            const float CL = 1f;// Coefficient of Learning
            const long StuckRefreshMaxCount = 3;// Coefficient of Learning
            static float LF = 0f;
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
                            newChal.LowerDifficulty();
                            //if (!newChal.LowerDifficulty())
                            //{// All the actions are at the minimum difficulty
                            //    newChal.AddRandomAction(_DifficultyMin);
                            //}
                        }
                    }

                    int Hdif = newChal.GetChallengeDifficulty() - original.GetChallengeDifficulty();
                    LF = CL * CR;

                    if (Hdif < LF)
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
                    else if (Hdif > LF)
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
                    else if (Hdif == LF) break;
                }
                return newChal;
            }

            public static List<Challenge> SinglePointCrossover(Challenge c1, Challenge c2)
            {
                int crossPoint = (Program.RandGenerator.Next(1, Math.Min(c1.ActionsList.Count, c2.ActionsList.Count)));
                Challenge offspring1 = new Challenge();
                Challenge offspring2 = new Challenge();

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
                int min = 200;
                int minIndex = 0;
                int minIndex2 = 0;

                for (int i = 0; i < chromosomePool.Count; i++)
                {
                    if (Math.Abs(chromosomePool[i].GetChallengeDifficulty() - LF) < min)
                    {
                        minIndex2 = minIndex;
                        minIndex = i;
                        min = (int)Math.Abs(chromosomePool[i].GetChallengeDifficulty() - LF);
                    }
                }
                theChosenOnes.Add(chromosomePool[minIndex]);
                theChosenOnes.Add(chromosomePool[minIndex2]);
                return theChosenOnes;
            }
        }

        static void Main()
        {
            long counter = 1;
            // Creating Challenges:
            Challenge Challenge1 = new Challenge(new List<Action>()
            {
                new Action(ActionType.T2, dpT2,2),
                new Action(ActionType.P2, dpP2,2),
                new Action(ActionType.T2, dpT2,4),
                new Action(ActionType.L2, dpL2,1)
            }, "Challenge1");

            Challenge Challenge2 = new Challenge(new List<Action>()
            {
                new Action(ActionType.T3, dpT3,2),
                new Action(ActionType.P3, dpP3,2),
                new Action(ActionType.T3, dpT3,2),
                new Action(ActionType.L2, dpL2,1)
            }, "Challenge2");

            Challenge1.PrintActions();
            Challenge2.PrintActions();
            // Create new challenges:
            // First Round
            //Console.WriteLine("\nRound 0\n");
            //Challenge new1Chal = Algorithm.CreateNewChallenge(Challenge1, 10, 4);
            //Challenge new2Chal = Algorithm.CreateNewChallenge(Challenge2, 10, 8);
            Challenge new1Chal = Challenge1;
            Challenge new2Chal = Challenge2;
            //new1Chal.PrintActions();
            //new2Chal.PrintActions();
            string stop = string.Empty;
            while (stop != "s")
            {
                // Second Round
                Console.WriteLine("\nRound " + counter++ + "\n");
                //Console.WriteLine(new1Chal.Name + " CR:  " + new1Chal.CalculateCR(10, 5));
                //Console.WriteLine(new2Chal.Name + " CR:  " + new2Chal.CalculateCR(10, 6));

                new1Chal = Algorithm.CreateNewChallenge(new1Chal, 10, 3);
                new2Chal = Algorithm.CreateNewChallenge(new2Chal, 10, 7);

                new1Chal.PrintActions();
                new2Chal.PrintActions();
                List<Challenge> chromosomePool = Algorithm.SinglePointCrossover(new1Chal, new2Chal);// Maybe create as global "Chromosome Pool", for better selection inorder to do better CrossOver

                chromosomePool.Add(new1Chal);
                chromosomePool.Add(new2Chal);

                List<Challenge> nextLevel = Algorithm.SelectChromForNextLevel(chromosomePool);
                new1Chal = nextLevel[0];
                new2Chal = nextLevel[1];

                new1Chal.PrintActions();
                new2Chal.PrintActions();

                stop = Console.ReadLine();
            }
        }
    }
}
