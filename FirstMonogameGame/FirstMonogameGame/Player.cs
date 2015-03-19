using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirstMonogameGame
{
    class Player : GameObject
    {
        //fields
        int levelScore;
        int totalScore;

        //Properties
        public int LevelScore { get { return levelScore; } set { levelScore = value; } }
        public int TotalScore { get { return totalScore; } set { totalScore = value; } }

        //constructor
        public Player(int x, int y, int width, int height):base(x, y, width, height)
        {
            levelScore = 0;
            totalScore = 0;
        }

    }
}
