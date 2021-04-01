
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Snake_AI_2._0
{
    public enum Direction
    {
        STOP = -1,
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3
    };

    public class SnakeGame
    {
        private const int WIDTH = 20;
        private const int HEIGHT = 20;

        public static Random rand = new Random();

        private List<Point> snake = new List<Point>();
        private List<Point> possibleFoodPoints = new List<Point>();
        private Point food = new Point();

        private volatile bool gameOver;

        private Direction direction;

        private NeuralNetwork net;
        private int[] layers = new int[] { 4, 24, 24, 3 };
        private float[] inputs = new float[4];
        private float[] outputs = new float[3];

        private int time;

        /*
         * 12 input layers
         * 4 direction - distance to food - distance to tail - distance to wall
         * */

        public SnakeGame()
        {

            gameOver = false;

            direction = Direction.STOP;


            snake.Clear();
            snake.Add(new Point(10, 10));

            possibleFoodPoints.Clear();
            for (int i = 0; i < HEIGHT; i++)
                for (int j = 0; j < WIDTH; j++)
                    possibleFoodPoints.Add(new Point(j, i));
            possibleFoodPoints.Remove(snake[snake.Count - 1]);

            GenerateFood();

            net = new NeuralNetwork(layers);


        }

        public SnakeGame(SnakeGame copy)
        {

            gameOver = false;

            direction = Direction.STOP;


            snake.Clear();
            snake.Add(new Point(10, 10));

            possibleFoodPoints.Clear();
            for (int i = 0; i < HEIGHT; i++)
                for (int j = 0; j < WIDTH; j++)
                    possibleFoodPoints.Add(new Point(j, i));
            possibleFoodPoints.Remove(snake[snake.Count - 1]);

            GenerateFood();

            net = new NeuralNetwork(copy.GetNet());

        }

        public void Play()
        {
            while (true)
            {

                if (gameOver)
                {
                    return;
                }

                Simulation();
            }

        }

        /*public void Watch()
        {
            gameOver = false;
            direction = Direction.STOP;


            snake.Clear();
            snake.Add(new Point(10, 10));

            possibleFoodPoints.Clear();
            for (int i = 0; i < HEIGHT; i++)
                for (int j = 0; j < WIDTH; j++)
                    possibleFoodPoints.Add(new Point(j, i));
            possibleFoodPoints.Remove(snake[snake.Count -1]);

            GenerateFood();

            Simulation();

        }*/


        private void GenerateFood()
        {

            food = possibleFoodPoints[rand.Next(possibleFoodPoints.Count)];


        }

        public float GetAngleToFood()
        {

            var a = snake;
            if (food.X < snake[snake.Count - 1].X)
                return (float)Math.Atan((((double)(food.Y - snake[snake.Count - 1].Y)) / (food.X - snake[snake.Count - 1].X)) + Math.PI / 2);
            else
                return (float)Math.Atan((((double)(food.Y - snake[snake.Count - 1].Y)) / (food.X - snake[snake.Count - 1].X)) + Math.PI / 2);

        }

        public void Simulation()
        {

            //setting inputs and then recieving networks decision
            SetInputs();
            outputs = net.FeedForward(inputs);

            //picking move based on decision
            switch (Decision(outputs))
            {
                case 0:
                    direction = (Direction)(((int)direction - 1) % 4);
                    break;
                case 1:
                    break;
                case 2:
                    direction = (Direction)(((int)direction + 1) % 4);
                    break;
            }

            Point newHead;

            switch (direction)
            {
                case Direction.Right:
                    newHead = new Point(snake[snake.Count - 1].X + 1, snake[snake.Count - 1].Y);

                    if (food.Equals(newHead))
                    {
                        net.AddFitness(50);
                        time += 50;
                        snake.Add(newHead);
                        possibleFoodPoints.Remove(food);
                        GenerateFood();
                    }
                    else if (newHead.X >= WIDTH || snake.Contains(newHead))
                    {
                        Die();

                    }
                    else
                    {
                        snake.Add(newHead);
                        possibleFoodPoints.Add(snake[0]);
                        possibleFoodPoints.Remove(snake[snake.Count - 1]);
                        snake.RemoveAt(0);

                    }
                    break;
                case Direction.Left:
                    newHead = new Point(snake[snake.Count - 1].X - 1, snake[snake.Count - 1].Y);
                    if (food.Equals(newHead))
                    {
                        net.AddFitness(50);
                        time += 50;
                        snake.Add(newHead);
                        possibleFoodPoints.Remove(food);
                        GenerateFood();
                    }
                    else if (newHead.X < 0 || snake.Contains(newHead))
                    {
                        Die();

                    }
                    else
                    {
                        snake.Add(newHead);
                        possibleFoodPoints.Add(snake[0]);
                        possibleFoodPoints.Remove(snake[snake.Count - 1]);
                        snake.RemoveAt(0);

                    }
                    break;
                case Direction.Up:
                    newHead = new Point(snake[snake.Count - 1].X, snake[snake.Count - 1].Y - 1);
                    if (food.Equals(newHead))
                    {
                        net.AddFitness(50);
                        time += 50;
                        snake.Add(newHead);
                        GenerateFood();
                        possibleFoodPoints.Remove(food);

                    }
                    else if (newHead.Y < 0 || snake.Contains(newHead))
                    {
                        Die();

                    }
                    else
                    {
                        snake.Add(newHead);
                        possibleFoodPoints.Add(snake[0]);
                        possibleFoodPoints.Remove(snake[snake.Count - 1]);
                        snake.RemoveAt(0);

                    }
                    break;
                case Direction.Down:
                    newHead = new Point(snake[snake.Count - 1].X, snake[snake.Count - 1].Y + 1);
                    if (food.Equals(newHead))
                    {
                        net.AddFitness(50);
                        time += 50;
                        snake.Add(newHead);
                        possibleFoodPoints.Remove(food);
                        GenerateFood();
                    }
                    else if (newHead.Y >= HEIGHT || snake.Contains(newHead))
                    {
                        Die();

                    }
                    else
                    {
                        snake.Add(newHead);
                        possibleFoodPoints.Add(snake[0]);
                        possibleFoodPoints.Remove(snake[snake.Count - 1]);
                        snake.RemoveAt(0);

                    }
                    break;
            }

            time--;
            net.AddFitness(1);
            if (time <= 0)
            {
                Die();

            }


        }


        private void SetInputs()
        {

            Point possibleLocation;
            for (int i = -1; i < 2; i++)
                switch ((Direction)(((int)direction + i) % 4))
                {
                    case Direction.Right:
                        possibleLocation = new Point(snake[snake.Count - 1].X + 1, snake[snake.Count - 1].Y);
                        if (possibleLocation.X >= WIDTH || snake.Contains(possibleLocation))
                            inputs[i + 1] = 4;
                        else
                            inputs[i + 1] = -4;
                        break;
                    case Direction.Left:
                        possibleLocation = new Point(snake[snake.Count - 1].X - 1, snake[snake.Count - 1].Y);
                        if (possibleLocation.X < 0 || snake.Contains(possibleLocation))
                            inputs[i + 1] = 4;
                        else
                            inputs[i + 1] = -4;
                        break;

                    case Direction.Up:
                        possibleLocation = new Point(snake[snake.Count - 1].X, snake[snake.Count - 1].Y - 1);
                        if (possibleLocation.Y < 0 || snake.Contains(possibleLocation))
                            inputs[i + 1] = 4;
                        else
                            inputs[i + 1] = -4;
                        break;

                    case Direction.Down:
                        possibleLocation = new Point(snake[snake.Count - 1].X, snake[snake.Count - 1].Y + 1);
                        if (possibleLocation.Y >= HEIGHT || snake.Contains(possibleLocation))
                            inputs[i + 1] = 4;
                        else
                            inputs[i + 1] = -4;
                        break;


                }

            inputs[3] = GetAngleToFood();

        }

        private int Decision(float[] outputData)
        {


            int temp = 0;
            for (int i = 0; i < outputData.Length; i++)
                if (outputData[i] > outputData[temp])
                    temp = i;
            return temp;

        }



        public void Die()
        {

            gameOver = true;


        }

        public NeuralNetwork GetNet()
        {

            return net;

        }
        public int GetLen()
        {

            return snake.Count;

        }

        public void Mutate()
        {

            net.Mutate();

        }
    }
}