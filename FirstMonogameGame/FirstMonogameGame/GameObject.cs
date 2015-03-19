using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FirstMonogameGame
{
    class GameObject
    {
        //Feilds
        private Texture2D texture;
        private Rectangle position;

        public Texture2D Texture { get { return texture; } set { texture = value; } }
        public Rectangle Position { get { return position; } set { position = value; } }
        public int X { get { return position.X; } set { position = new Rectangle(value, position.Y, position.Width, position.Height); } }
        public int Y { get { return position.Y; } set { position = new Rectangle(position.X, value, position.Width, position.Height); } }
        public int Width { get { return position.Width; } set { position = new Rectangle(position.X, position.Y, value, position.Height); } }
        public int Height { get { return position.Height; } set { position = new Rectangle(position.X, position.Y, position.Width, value); } }

        //constructor
        public GameObject(int x, int y, int width, int height)
        {
            position = new Rectangle(x, y, width, height);
        }

        public virtual void Draw(SpriteBatch batch)
        {
            batch.Draw(texture, position, Color.White);
        }

    }

}
