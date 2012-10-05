using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;

namespace MinMaxMoola
{
    public class GoldRush
    {
        public enum Turn : short
        {
            Start = 0,
            First,
            Second,
            Third,
            Fourth,
            Fifth,
            Sixth,
            Done
        }

        public enum BoardState
        {
            Win,
            Lose,
            Draw,
            Unfinished
        }

        private const string History = "History.txt";
        private const string Record = "Record.txt";
        private const string PlayerInfo = "PlayerInfo.txt";

        private int User = 0;
        private int Computer = 1;
        private int Opponent = 2;
        private int Players = 3;
        private int N = 6;

        private short[,] m_board;

        private Turn m_currentTurn;
        private List<short>[] m_numbersLeft;

        private string m_opponentName;
        private bool m_isGuaranteedPerson;
        private DateTime m_joinDate;
        private Dictionary<string, List<short[]>> m_history;
        private List<string> m_guaranteedPerson;
        private Dictionary<DateTime, List<string>> m_dateToPlayers;

        private int m_wins;
        private int m_losses;
        private int m_draws;

        public GoldRush()
        {
            m_currentTurn = Turn.Start;
            m_numbersLeft = new List<short>[3];
            m_board = new short[Players, N];

            for (int i = 0; i < Players; i++)
            {
                m_numbersLeft[i] = new List<short>();
            }
            for (short i = 1; i <= N; i++)
            {
                m_numbersLeft[User].Add(i);
                m_numbersLeft[Opponent].Add(i);
                m_numbersLeft[Computer].Add(i);
            }

            m_history = new Dictionary<string, List<short[]>>();
            m_guaranteedPerson = new List<string>();
            m_dateToPlayers = new Dictionary<DateTime, List<string>>();
            m_isGuaranteedPerson = false;
            m_joinDate = DateTime.MinValue;
            Load();
         //   OptimalBet();
            Go();
        }

        private void OptimalBet()
        {
            const int MaxCount = 30;
            const double Start = .01;

            double[] bets = new double[MaxCount];
            bets[0] = Start;

            for (int i = 1; i < MaxCount; i++)
            {
                bets[i] = bets[i - 1] * 2;
            }

            double start = .01;
            DefineBalance:
            Console.Write("\nCurrent balance: ");
            string str = Console.ReadLine();
            try
            {
                start = Convert.ToDouble(str);
            }
            catch
            {
                goto DefineBalance;
            }

            int maxBetIndex = GetMaxBetIndex(bets, start);
            Console.WriteLine("Optimal Size = 1/4th of balance = " + (Math.Max(.01, Math.Min(bets[maxBetIndex] / 2.0, bets[maxBetIndex] / 4.0))) + ".");
        }

        private static int GetMaxBetIndex(double[] bets, double value)
        {
            for (int i = 1; i < bets.Length; i++)
            {
                if (bets[i] > value)
                {
                    return i - 1;
                }
            }
            return 0;
        }

        private void FirstThrowHistory(List<string> validOpponentNames)
        {
            short[] firstThrows = new short[N];

            IDictionaryEnumerator e = m_history.GetEnumerator();
            while (e.MoveNext())
            {
                string opp = (string)e.Key;
                List<short[]> games = (List<short[]>)e.Value;

                if (validOpponentNames.Contains(opp))
                {
                    foreach (short[] game in games)
                    {
                        foreach (int y in m_numbersLeft[Opponent])
                        {
                            if (game[N * Opponent + y - 1] == 1)
                            {
                                firstThrows[y - 1]++;
                                break;
                            }
                        }
                    }
                }
            }

            if (validOpponentNames.Count == 1 && validOpponentNames[0] == m_opponentName)
            {
                Console.WriteLine("First Throw History[" + m_opponentName + "]:");
            }
            else
            {
                Console.WriteLine("First Throw History[" + m_joinDate.ToShortDateString() + "]:");
            }
            for (int i = 0; i < N; i++)
            {
                Console.WriteLine((i + 1) + ": " + firstThrows[i]);
            }
            Console.WriteLine();

            if (validOpponentNames.Count == 1)
            {                
                foreach (string person in validOpponentNames)
                {
                    if (m_history.ContainsKey(person))
                    {
                        int wins = 0;
                        int losses = 0;
                        int ties = 0;

                        foreach (short[] game in m_history[person])
                        {
                            BoardState status = Status(game);
                            if (status == BoardState.Win)
                            {
                                wins++;
                            }
                            if (status == BoardState.Lose)
                            {
                                losses++;
                            }
                            if (status == BoardState.Draw)
                            {
                                ties++;
                            }
                        }

                        Console.WriteLine(validOpponentNames[0] + " win rate = " + (100 * losses / (wins + losses + ties)) + "%\n");
                    }
                }                
            }
        }

        private void Histogram(int computerDisplays, List<string> validOpponentNames)
        {
            int[] userPicks = new int[N];
            int userTotalPicks = 0;

            int[] allPicks = new int[N];
            int allTotalPicks = 0;

            IDictionaryEnumerator e = m_history.GetEnumerator();
            while (e.MoveNext())
            {
                string opp = (string)e.Key;
                List<short[]> games = (List<short[]>)e.Value;

                if (validOpponentNames.Contains(opp))
                {
                    foreach (short[] game in games)
                    {
                        foreach (int x in m_numbersLeft[User])
                        {
                            if (game[N * User + x - 1] < (int)m_currentTurn) goto Next;
                        }
                        foreach (int y in m_numbersLeft[Opponent])
                        {
                            if (game[N * Opponent + y - 1] < (int)m_currentTurn) goto Next;
                        }
                        foreach (int z in m_numbersLeft[Computer])
                        {
                            if (game[N * Computer + z - 1] <= (int)m_currentTurn) goto Next;
                        }
                        if (game[N * Computer + computerDisplays - 1] != (int)m_currentTurn) goto Next;

                        for (int i = 0; i <= 5; i++)
                        {
                            if (game[N * Opponent + i] == (int)m_currentTurn)
                            {
                                userPicks[i]++;
                            }                            
                        }

                        userTotalPicks++;

                    Next: ;
                    }
                }

                foreach (short[] game in games)
                {
                    foreach (int x in m_numbersLeft[User])
                    {
                        if (game[N * User + x - 1] < (int)m_currentTurn) goto NextAll;
                    }
                    foreach (int y in m_numbersLeft[Opponent])
                    {
                        if (game[N * Opponent + y - 1] < (int)m_currentTurn) goto NextAll;
                    }
                    foreach (int z in m_numbersLeft[Computer])
                    {
                        if (game[N * Computer + z - 1] <= (int)m_currentTurn) goto NextAll;
                    }
                    if (game[N * Computer + computerDisplays - 1] != (int)m_currentTurn) goto NextAll;

                    for (int i = 0; i <= 5; i++)
                    {
                        if (game[N * Opponent + i] == (int)m_currentTurn)
                        {
                            allPicks[i]++;
                        }                        
                    }

                    allTotalPicks++;

                NextAll: ;
                }
            }

            Console.WriteLine("\nThrow History:");
            for (int i = 0; i < N; i++)
            {
                if (validOpponentNames.Count == 1 && validOpponentNames[0] == m_opponentName)
                {
                    Console.Write(m_opponentName + ": " + (i + 1) + " " + (100 * (float)userPicks[i] / (float)userTotalPicks) + "% (" + userPicks[i] + "/" + userTotalPicks + ")");
                }
                else
                {
                    Console.Write(m_joinDate.ToShortDateString() + ": " + (i + 1) + " " + (100 * (float)userPicks[i] / (float)userTotalPicks) + "% (" + userPicks[i] + "/" + userTotalPicks + ")");
                }
                Console.WriteLine("\t[all: " + (i + 1) + " " + (100 * (float)allPicks[i] / (float)allTotalPicks) + "% (" + allPicks[i] + "/" + allTotalPicks + ")]");
            }
            Console.WriteLine();            
        }

        private void Load()
        {
            if (File.Exists(PlayerInfo))
            {
                string playerinfo = File.ReadAllText(PlayerInfo);
                string[] people = playerinfo.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string person in people)
                {
                    string[] info = person.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string name = info[0];
                    string dateOrGuarantee = info[1];
                    if (dateOrGuarantee.StartsWith("*"))
                    {
                        m_guaranteedPerson.Add(name);
                    }
                    else
                    {
                        DateTime date = Convert.ToDateTime(dateOrGuarantee);
                        if (!m_dateToPlayers.ContainsKey(date))
                        {
                            m_dateToPlayers.Add(date, new List<string>());
                        }

                        m_dateToPlayers[date].Add(name);
                    }
                }
            }

            if (File.Exists(History))
            {
                string history = File.ReadAllText(History);
                string[] games = history.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);                

                foreach (string game in games)
                {
                    string[] gameArray = game.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string opponent = gameArray[0];
                    short[] turns = new short[N * Players];
                    for (int i = 1; i < gameArray.Length; i++)
                    {
                        turns[i - 1] = (short)Convert.ToInt16(gameArray[i]);
                    }
                    Status(turns);
                    if (m_history.ContainsKey(opponent))
                    {
                        m_history[opponent].Add(turns);
                    }
                    else
                    {
                        List<short[]> turnList = new List<short[]>();
                        turnList.Add(turns);
                        m_history.Add(opponent, turnList);
                    }
                }
            }

            if (File.Exists(Record))
            {
                string record = File.ReadAllText(Record);
                string[] wlSplit = record.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                m_wins = Convert.ToInt16(wlSplit[0]);
                m_losses = Convert.ToInt16(wlSplit[1]);
                m_draws = Convert.ToInt16(wlSplit[2]);
            }
        }

        private BoardState Status(short[] game)
        {
            int[] scores = new int[Players];

            for (int i = (short)Turn.First; i <= (short)Turn.Sixth; i++)
            {
                int[] currentTurn = new int[Players];

                for (int k = 0; k < Players; k++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (game[(k * N) + j] == i)
                        {
                            currentTurn[k] = (j + 1);
                        }
                    }
                }

                if (currentTurn[User] > currentTurn[Opponent])
                {
                    scores[User] += (currentTurn[User] + currentTurn[Computer] + currentTurn[Opponent] + scores[Computer]);
                    scores[Computer] = 0;
                }
                else if (currentTurn[User] < currentTurn[Opponent])
                {
                    scores[Opponent] += (currentTurn[User] + currentTurn[Computer] + currentTurn[Opponent] + scores[Computer]);
                    scores[Computer] = 0;
                }
                else
                {
                    scores[Computer] += (currentTurn[User] + currentTurn[Computer] + currentTurn[Opponent]);
                }
            }

            BoardState state = BoardState.Unfinished;

            if (scores[User] > scores[Opponent])
            {
                state = BoardState.Win;
            }
            else if (scores[User] < scores[Opponent])
            {
                state = BoardState.Lose;
            }
            else
            {
                state = BoardState.Draw;
            }

            return state;
        }

        private bool GuaranteeLookup()
        {
            if (m_guaranteedPerson.Contains(m_opponentName))
            {
                return true;
            }
            return false;
        }

        private DateTime DateLookup()
        {
            IDictionaryEnumerator e = m_dateToPlayers.GetEnumerator();
            while (e.MoveNext())
            {
                DateTime key = (DateTime)e.Key;
                List<string> value = (List<string>)e.Value;
                foreach (string s in value)
                {
                    if (s == m_opponentName)
                    {
                        return key;
                    }
                }
            }
            return DateTime.MinValue;
        }

        private void Go()
        {            
            if (m_currentTurn == Turn.Start)
            {
                Console.Write("\nOpponent's Name: ");
                m_opponentName = Console.ReadLine();
                Console.WriteLine();
                if (!GuaranteeLookup())
                {
                    Console.Write("Is " + m_opponentName + " definitely a person (y/n)? ");
                    string guarantee = Console.ReadLine();
                    if (guarantee.ToLower() == "y")
                    {
                        m_isGuaranteedPerson = true;
                    }
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("This player is definitely a person.");
                    Console.WriteLine();
                }
            JoinDate:
                if (DateLookup() == DateTime.MinValue)
                {
                    Console.Write("When did this player join (mmddyy)? ");
                    string startdate = Console.ReadLine();
                    foreach (char c in startdate)
                    {
                        if (!char.IsDigit(c))
                        {
                            goto JoinDate;
                        }
                    }
                    string month = startdate.Substring(0, 2);
                    string day = startdate.Substring(2, 2);
                    string year = startdate.Substring(4, 2);
                    try
                    {
                        int intMonth = Convert.ToInt16(month);
                        int intDay = Convert.ToInt16(day);
                        int intYear = Convert.ToInt16(year) + 2000;
                        m_joinDate = new DateTime(intYear, intMonth, intDay);
                    }
                    catch
                    {
                        goto JoinDate;
                    }
                }
                else
                {
                    m_joinDate = DateLookup();
                    Console.WriteLine("This player started on " + m_joinDate.ToShortDateString() + ".");
                    Console.WriteLine();
                }
                List<string> opponents = new List<string>();
                opponents.Add(m_opponentName);
                FirstThrowHistory(opponents);

                if (m_dateToPlayers.ContainsKey(m_joinDate))
                {
                    if (m_dateToPlayers[m_joinDate].Count == 1 &&
                        m_dateToPlayers[m_joinDate][0] == m_opponentName)
                    {
                        // no op
                    }
                    else
                    {
                        FirstThrowHistory(m_dateToPlayers[m_joinDate]);
                    }
                }
            }

            Display();
            m_currentTurn++;

            if (m_currentTurn == Turn.Done || EvaluateBoard() != BoardState.Unfinished)
            {
                BoardState state = EvaluateBoard();
                Console.WriteLine("Game Complete: " + state.ToString());

                switch (state)
                {
                    case BoardState.Win:
                        m_wins++;
                        break;
                    case BoardState.Lose:
                        m_losses++;
                        break;
                    case BoardState.Draw:
                        m_draws++;
                        break;
                }

                Console.WriteLine("(" + m_wins + "/" + m_losses + "/" + m_draws + ")");
                Console.WriteLine("Win% = " + (m_wins * 100.0 / (m_wins + m_losses + m_draws)) + "%");

                Save();
                return;
            }

            string input;

        Computer:
            Console.Write("Computer displays: ");
            input = Console.ReadLine();
            if (input.ToLower() == "q")
            {
                m_currentTurn = Turn.Done;
                return;
            }
            if (ValidShort(input, Computer))
            {
                m_board[Computer, ToShort(input) - 1] = (short)m_currentTurn;
                m_numbersLeft[Computer].Remove(ToShort(input));
            }
            else
            {
                goto Computer;
            }

            List<string> opponents2 = new List<string>();
            opponents2.Add(m_opponentName);
            Histogram(ToShort(input), opponents2);
            if (m_dateToPlayers.ContainsKey(m_joinDate))
            {
                if (m_dateToPlayers[m_joinDate].Count == 1 &&
                    m_dateToPlayers[m_joinDate][0] == m_opponentName)
                {
                    // no op
                }
                else
                {
                    Histogram(ToShort(input), m_dateToPlayers[m_joinDate]);
                }
            }

            if (m_currentTurn > Turn.Second)
            {
                int rating = 0;
                List<short> bestMoves = FindBestMove(User, ref rating, int.MaxValue, 0);
                string moves = "{";
                for (int i = 0; i < bestMoves.Count; i++)
                {
                    moves += bestMoves[i].ToString();
                    if (i != bestMoves.Count - 1)
                    {
                        moves += ", ";
                    }
                    else
                    {
                        moves += "}";
                    }
                }
                Console.WriteLine("Best Move: " + moves);
                Console.WriteLine("Rating: " + rating);

                User = 2;
                Opponent = 0;

                rating = 0;
                bestMoves = FindBestMove(User, ref rating, int.MaxValue, 0);
                moves = "{";
                for (int i = 0; i < bestMoves.Count; i++)
                {
                    moves += bestMoves[i].ToString();
                    if (i != bestMoves.Count - 1)
                    {
                        moves += ", ";
                    }
                    else
                    {
                        moves += "}";
                    }
                }
                Console.WriteLine("Opponent's Best Move: " + moves);
                Console.WriteLine("Opponent's Rating: " + rating + "\n");

                User = 0;
                Opponent = 2;
            }

        User:
            Console.Write("User chooses: ");
            input = Console.ReadLine();
            if (input.ToLower() == "q")
            {
                m_currentTurn = Turn.Done;
                return;
            }
            if (ValidShort(input, User))
            {
                m_board[User, ToShort(input) - 1] = (short)m_currentTurn;
                m_numbersLeft[User].Remove(ToShort(input));
            }
            else
            {
                goto User;
            }

        Opponent:
            Console.Write("Opponent displays: ");
            input = Console.ReadLine();
            if (input.ToLower() == "q")
            {
                m_currentTurn = Turn.Done;
                return;
            }
            if (ValidShort(input, Opponent))
            {
                m_board[Opponent, ToShort(input) - 1] = (short)m_currentTurn;
                m_numbersLeft[Opponent].Remove(ToShort(input));
            }
            else
            {
                goto Opponent;
            }

            Console.WriteLine();
            Go();
        }

        private void Save()
        {
            if (!File.Exists(History))
            {
                FileStream fs = File.Create(History);
                fs.Close();
            }
            File.AppendAllText(History, ToString());

            if (!File.Exists(Record))
            {
                FileStream fs = File.Create(Record);
                fs.Close();
            }
            File.WriteAllLines(Record, new string[] {m_wins + " " + m_losses + " " + m_draws});

            if (!File.Exists(PlayerInfo))
            {
                FileStream fs = File.Create(PlayerInfo);
                fs.Close();
            }
            if (!m_guaranteedPerson.Contains(m_opponentName) && m_isGuaranteedPerson)
            {
                File.AppendAllText(PlayerInfo, m_opponentName + " *\r\n");
            }
            if (!m_dateToPlayers.ContainsKey(m_joinDate) || !m_dateToPlayers[m_joinDate].Contains(m_opponentName))
            {
                File.AppendAllText(PlayerInfo, m_opponentName + " " + m_joinDate.ToShortDateString() + "\r\n");
            }
        }

        private new string ToString()
        {
            string s = m_opponentName + " ";
            for (int i = 0; i < Players; i++)
            {
                for (int j = 0; j < N; j++)
                {
                    s += m_board[i, j].ToString() + " ";
                }
            }
            s += "\r\n";
            return s;
        }

        private short ToShort(string input)
        {
            return (short)Convert.ToInt16(input);
        }

        private bool ValidShort(string input, int player)
        {
            short s;
            try
            {
                s = (short)Convert.ToInt16(input);
            }
            catch
            {
                Console.WriteLine("Not a number.");
                return false;
            }

            if (s < 1 || s > N)
            {
                Console.WriteLine("Number must be between 1 and " + N + ".");
                return false;
            }

            if (!m_numbersLeft[player].Contains(s))
            {
                Console.WriteLine("That number has been used already.");
                return false;
            }

            return true;
        }

        private void Display()
        {
            string a = "      |";
            for (int i = 1; i <= N; i++)
            {
                a += " " + i.ToString() + " |";
            }
            string b = "user  |";
            string c = "comp  |";
            string d = "opp   |";
            string hit = " X |";
            string miss = "   |";

            for (int i = 0; i < N; i++)
            {
                if (m_board[User, i] != 0)
                {
                    b += hit;
                }
                else
                {
                    b += miss;
                }
                if (m_board[Computer, i] != 0)
                {
                    c += hit;
                }
                else
                {
                    c += miss;
                }
                if (m_board[Opponent, i] != 0)
                {
                    d += hit;
                }
                else
                {
                    d += miss;
                }
            }

            int[] status = ScoreBoard();

            string info = "Turn: " + ((Turn)(m_currentTurn + 1)).ToString() + "\n";
            info += "User Score: " + status[User].ToString() + "\n";
            info += "Opp(" + m_opponentName + ")" + " Score: " + status[Opponent].ToString() + "\n";
            info += "Computer Shows: " + status[Computer].ToString() + "\n\n";
            info += a + "\n" + b + "\n" + c + "\n" + d + "\n";
            Console.WriteLine(info);
        }

        private BoardState EvaluateBoard()
        {
            BoardState state = BoardState.Unfinished;

            int[] scores = ScoreBoard();

            if (m_currentTurn == Turn.Done)
            {
                if (scores[User] > scores[Opponent])
                {
                    state = BoardState.Win;
                }
                else if (scores[User] < scores[Opponent])
                {
                    state = BoardState.Lose;
                }
                else
                {
                    state = BoardState.Draw;
                }
            }

            return state;
        }

        private int[] ScoreBoard()
        {
            int[] scores = new int[Players];

            for (int i = (short)Turn.First; i <= (short)m_currentTurn; i++)
            {
                int[] currentTurn = new int[Players];

                for (int k = 0; k < Players; k++)
                {
                    for (int j = 0; j < N; j++)
                    {
                        if (m_board[k, j] == i)
                        {
                            currentTurn[k] = (j + 1);
                        }
                    }
                }

                if (currentTurn[User] > currentTurn[Opponent])
                {
                    scores[User] += (currentTurn[User] + currentTurn[Computer] + currentTurn[Opponent] + scores[Computer]);
                    scores[Computer] = 0;
                }
                else if (currentTurn[User] < currentTurn[Opponent])
                {
                    scores[Opponent] += (currentTurn[User] + currentTurn[Computer] + currentTurn[Opponent] + scores[Computer]);
                    scores[Computer] = 0;
                }
                else
                {
                    scores[Computer] += (currentTurn[User] + currentTurn[Computer] + currentTurn[Opponent]);
                }
            }

            return scores;
        }

        private List<short> FindBestMove(int player, ref int rating, int initialBestRating, short depth)
        {
            List<short> bestMoves = new List<short>();
            int currentRating = -1;
            int bestRating = -initialBestRating;

            m_currentTurn += depth;
            BoardState state = EvaluateBoard();
            m_currentTurn -= depth;

            if (state != BoardState.Unfinished)
            {
                switch (state)
                {
                    case BoardState.Win:
                        rating = 1;
                        break;
                    case BoardState.Lose:
                        rating = -1;
                        break;
                    case BoardState.Draw:
                        rating = 0;
                        break;
                }

                return bestMoves;
            }

            for (int i = 0; i < m_numbersLeft[player].Count; i++)
            {
                short s = m_numbersLeft[player][i];

                m_board[player, s - 1] = (short)(m_currentTurn + depth);
                m_numbersLeft[player].Remove(s);

                if (player == User)
                {
                    FindBestMove(Opponent, ref currentRating, -initialBestRating, depth);
                }
                else if (player == Opponent)
                {
                    if (m_currentTurn + depth + 1 == Turn.Done)
                    {
                        FindBestMove(User, ref currentRating, -initialBestRating, (short)(depth + 1));
                    }
                    else
                    {
                        for (int j = 0; j < m_numbersLeft[Computer].Count; j++)
                        {
                            short t = m_numbersLeft[Computer][j];

                            m_board[Computer, t - 1] = (short)(m_currentTurn + depth + 1);
                            m_numbersLeft[Computer].Remove(t);
                            FindBestMove(User, ref currentRating, -initialBestRating, (short)(depth + 1));
                            m_board[Computer, t - 1] = 0;
                            m_numbersLeft[Computer].Insert(j, t);
                        }
                    }
                }

                if (player == User)
                {
                    if (currentRating > bestRating)
                    {
                        bestMoves.Clear();
                        bestMoves.Add(s);

                        bestRating = currentRating;
                    }
                    else if (currentRating == bestRating)
                    {
                        bestMoves.Add(s);
                    }
                }
                else if (player == Opponent)
                {
                    if (currentRating < bestRating)
                    {
                        bestMoves.Clear();
                        bestMoves.Add(s);

                        bestRating = currentRating;
                    }
                    else if (currentRating == bestRating)
                    {
                        bestMoves.Add(s);
                    }
                }

                m_board[player, s - 1] = 0;
                m_numbersLeft[player].Insert(i, s);
            }

            rating = bestRating;
            return bestMoves;
        }
    }
}