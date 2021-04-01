using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake_AI_2._0
{
    public partial class Form1 : Form
    {
        public const int POP = 10000;


        public SnakeGame[] snakes = new SnakeGame[POP];

        private SnakeGame snakeBeingWatched;
        private SnakeGame snakeOnDeck;

        public bool snakeIsDead = true;
        private bool prep = true;

        List<string> lines = new List<string>();

        Task run;


        public int generation;

        List<Task> tasks = new List<Task>();


        public Form1()
        {
            InitializeComponent();

            //Set settings to default

            //Set game speed and start timer
            gameTimer.Interval = 1200 / 16;
            gameTimer.Tick += UpdateScreen;
            gameTimer.Start();

        }



        public  void Gen()
        {
            tasks.Clear();
            prep = false;
            ValidateNoDupes(snakes);
            for (int i = 0; i < POP - 1; i++)
            {
                tasks.Add(Task.Run(() => snakes[i].Play()));
            }
            
            Task.WaitAll(tasks.ToArray());
            
        }
        public void ValidateNoDupes(SnakeGame[] snakes)
        {
            HashSet<int> refs = new HashSet<int>();
            foreach (var snake in snakes)
            {
                int hash = snake.GetHashCode();

                if (refs.TryGetValue(hash, out var existingSnake))
                {
                    if (object.ReferenceEquals(snake, existingSnake))
                    {
                        throw new InvalidOperationException("OH NO, A DUPE!");
                    }
                }
                else
                {
                    refs.Add(hash);
                }
            }
        }

        public void GenPrep()
        {
            //ExportNetworks(snakes);
            //File.WriteAllLines( generation + ".txt", lines.ToArray());
            generation++;
            SortSnakes(ref snakes);
            snakeOnDeck = snakes[0];
            for (int i = 0; i < POP / 2; i++)
            {
                snakes[POP / 2 + i] = new SnakeGame(snakes[i]);
                snakes[i] = new SnakeGame(snakes[POP / 2 + i]);
                snakes[POP / 2 + i].Mutate();
                
            }
            prep = true;
        }

        private void SortSnakes(ref SnakeGame[] unsorted)
        {
            int n = unsorted.Length;
            for (int i = 0; i < n - 1; i++)
                for (int j = 0; j < n - i - 1; j++)
                    if (unsorted[j].GetNet().GetFitness() < unsorted[j + 1].GetNet().GetFitness())
                    {
                        // swap temp and arr[i] 
                        SnakeGame temp = unsorted[j];
                        unsorted[j] = unsorted[j + 1];
                        unsorted[j + 1] = temp;
                    }

        }



        //Place random food object


        private void UpdateScreen(object sender, EventArgs e)
        {
            pbCanvas.Invalidate();

        }

        private void pbCanvas_Paint(object sender, PaintEventArgs e)
        {

            label7.Text = snakes[0].GetNet().GetFitness().ToString();
            label3.Text = generation.ToString();
            Graphics canvas = e.Graphics;

            /*if (false)
            if (snakeIsDead)
            {
                turn = 0;
                snakeBeingWatched = snakeOnDeck;
                snakeBeingWatched.Watch();
                snakeIsDead = false;
            }
            else
            {
                if (!snakeBeingWatched.gameOver)
                {
                    label1.Text = snakeBeingWatched.net.GetFitness().ToString();
                    for (int i = 0; i < snakeBeingWatched.GetLen(); i++)
                    {
                        Brush snakeColour;
                        if (i == 0)
                            snakeColour = Brushes.Black;     //Draw head
                        else
                            snakeColour = Brushes.Green;    //Rest of body

                        //Draw snake
                        canvas.FillEllipse(snakeColour,
                            new Rectangle(snakeBeingWatched.snake[i].X * 20,
                                          snakeBeingWatched.snake[i].Y * 20,
                                          20, 20));


                        //Draw Food
                        canvas.FillEllipse(Brushes.Red,
                            new Rectangle(snakeBeingWatched.food.X * 20,
                                 snakeBeingWatched.food.Y * 20, 20, 20));

                    }
                    lines.Add("Turn: " + turn);
                    lines.Add("{");
                    lines.Add("Fitness: " + snakeBeingWatched.net.GetFitness());
                    lines.Add("length: " + snakeBeingWatched.snake.Count);
                    lines.Add("Angle to food: " + snakeBeingWatched.GetAngleToFood());
                    lines.Add("}");
                    snakeBeingWatched.Simulation();
                }
                else
                {
                    snakeIsDead = true;
                }
            }*/


        }


        public void recordData()
        {

        }


        public void ExportNetworks(SnakeGame[] nets)
        {
            string netLocation = @"C:\Users\The Beast\Desktop\EVERYTHING\Programming\Visual Studio\Snake\Snake AI\DATA\NETWORKS";
            string filename;
            List<string> networkString = new List<string>();

            for (int i = 0; i < POP / 10; i++)
            {
                filename = "net" + i.ToString() + ".txt";
                filename = Path.Combine(netLocation, filename);

                for (int j = 0; j < nets[i].GetNet().weights.Length; j++)
                {
                    for (int k = 0; k < nets[i].GetNet().weights[j].Length; k++)
                    {
                        for (int l = 0; l < nets[i].GetNet().weights[j][k].Length; l++)
                        {
                            networkString.Add(nets[i].GetNet().weights[j][k][l].ToString());
                        }
                        networkString.Add(Environment.NewLine);
                    }
                    networkString.Add(Environment.NewLine);
                }

                File.WriteAllLines(filename, networkString);
                networkString.Clear();

            }

        }

        public void ImportNetwork()
        {

        }
        private void Form1_FormClosing(Object sender, FormClosingEventArgs e)
        {

            System.Text.StringBuilder messageBoxCS = new System.Text.StringBuilder();
            messageBoxCS.AppendFormat("{0} = {1}", "CloseReason", e.CloseReason);
            messageBoxCS.AppendLine();
            messageBoxCS.AppendFormat("{0} = {1}", "Cancel", e.Cancel);
            messageBoxCS.AppendLine();
            MessageBox.Show(messageBoxCS.ToString(), "FormClosing Event");

        }


        private void genration_Click(object sender, EventArgs e)
        {
            if (!prep)
                GenPrep();
            /*if(generation != 0)
            {
                if (Task.WhenAll(tasks).IsCompleted)
                    Gen();
            }
            else*/
                Gen();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < POP; i++)
                snakes[i] = new SnakeGame();
            snakeOnDeck = snakes[0];

        }
    }
}
