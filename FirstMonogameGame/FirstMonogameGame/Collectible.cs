using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FirstMonogameGame
{
    class Collectible : GameObject
    {
        //fields
        private bool active;

        //properties
        public bool Active { get { return active; } set { active = value; } }

        public Collectible(int x, int y, int width, int height) : base(x, y, width, height)
        {
            active = true;
        }

        /// <summary>
        /// Detects a collision with the given game object
        /// </summary>
        /// <param name="collision">the object to check for a collision with</param>
        /// <returns>true if the position of the objects overlaps</returns>
        public bool CheckColision(GameObject collision)
        {
            if (active)
                return collision.Position.Intersects(this.Position);
            else
                return false;
        }

        public override void Draw(SpriteBatch batch)
        {
            if (active)
                base.Draw(batch);
        }

        
    }
}
