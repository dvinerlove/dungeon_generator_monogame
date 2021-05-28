using MonoGame.Extended.Collisions;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace dungeon_generator
{
    class Room : ICollisionActor
    {
        public int tileSize { get; set; }
        public Room(RectangleF rectangle)
        {
            Bounds = rectangle;
            Color = Color.White;
            Center = rectangle.Center;
            tileSize = 16;
        }

        public IShapeF Bounds { get; set; }
        private Vector2 Center;
        public Vector2 GetCenter()
        {
            RectangleF rectangle = (RectangleF)Bounds;
            return rectangle.Center;
        }
        public Color Color { get; internal set; }
        public bool IsConnected { get; internal set; }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            Bounds.Position -= collisionInfo.PenetrationVector;
            Bounds.Position = new Point2(Bounds.Position.X - collisionInfo.PenetrationVector.X- collisionInfo.PenetrationVector.X%16, Bounds.Position.Y - collisionInfo.PenetrationVector.Y - collisionInfo.PenetrationVector.Y % 16);
            RectangleF rectangle = (RectangleF)Bounds;
            Center = rectangle.Center;
        }

        internal void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch _spriteBatch)
        {
            if (Color == Color.Red)
            {

                if (!IsConnected)
                    _spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Yellow, 10);
                else
                    _spriteBatch.DrawRectangle((RectangleF)Bounds, Color, 10);
            }
            else
            {

                _spriteBatch.DrawRectangle((RectangleF)Bounds, Color, 10);
            }

        }
    }
}
