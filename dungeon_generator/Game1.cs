using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGame.Extended;
using System.Collections.Generic;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.ViewportAdapters;
using System.Diagnostics;
using System.Linq;

namespace dungeon_generator
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Random Random = new Random();
        int tileSize = 16;
        IShapeF circle;
        List<IShapeF> rectangles;
        List<Room> rooms;
        private CollisionComponent _collisionComponent;
        private OrthographicCamera _camera;
        Matrix transformMatrix;
        private int windowWidth;
        private int windowHeight;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        void GenerateMap()
        {

            var Display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;//MonoGame.Extended.Screens.Screen.AllScreens[0]; // Change 0 for other screens
            windowWidth = _graphics.PreferredBackBufferWidth;
            windowHeight = _graphics.PreferredBackBufferHeight;
            var center = new Point2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);


            circle = new CircleF(center, _graphics.PreferredBackBufferHeight * 2);
            CircleF c = (CircleF)circle;
            rectangles = new List<IShapeF>();
            for (int i = 0; i < 100; i++)
            {
                var size = new Size2(tileSize * Random.Next(0, 32), tileSize * Random.Next(0, 32));
                var rect = new RectangleF();
                rect.Size = size;
                rect.Position = new Point2(Random.Next((int)(c.Center.X - c.Radius), 1 + (int)(c.Center.X + c.Radius + 420)), c.Center.Y);
                rect.Position = new Point2(rect.Position.X, Random.Next((int)(c.Center.Y - c.Radius), 1 + (int)(c.Center.Y + c.Radius + 420)));
                rect.Position = new Point2(rect.Position.X - rect.Position.X % tileSize, rect.Position.Y - rect.Position.Y % tileSize);
                rectangles.Add(rect);
            }
            foreach (RectangleF item in rectangles.ToArray())
            {
                if (!circle.Intersects(item))
                {
                    rectangles.Remove(item);
                }
            }
            rooms = new List<Room>();
            foreach (RectangleF item in rectangles.ToArray())
            {
                rooms.Add(new Room(item));
            }
            foreach (var item in rooms)
            {
                _collisionComponent.Insert(item);
            }
            foreach (var item in rooms)
            {
                item.Bounds.Position = new Point2(item.Bounds.Position.X - item.Bounds.Position.X % tileSize, item.Bounds.Position.Y - item.Bounds.Position.Y % tileSize);
            }




            Window.Title = rectangles.Count.ToString();

        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _collisionComponent = new CollisionComponent(new RectangleF(-_graphics.PreferredBackBufferWidth * 2, -_graphics.PreferredBackBufferHeight * 2, _graphics.PreferredBackBufferWidth * 10, _graphics.PreferredBackBufferHeight * 10));
            GenerateMap();

            //   var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, windowWidth, windowHeight);
            var Display = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;//MonoGame.Extended.Screens.Screen.AllScreens[0]; // Change 0 for other screens
                                                                            //_graphics.PreferredBackBufferWidth = Display.Width;
                                                                            //_graphics.PreferredBackBufferHeight = Display.Height;
                                                                            //_graphics.ApplyChanges();
                                                                            // var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, Display.Width/2, Display.Height/2);

            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, windowWidth, windowHeight);

            _camera = new OrthographicCamera(viewportAdapter);


            _camera.Zoom = 0.1f;
            //_camera.Position = Vector2.Zero;

            //Window.Title = GetRandomPointCircle(2).ToString();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.OemPlus) && _camera.Zoom < 1.1f)
            {
                _camera.Zoom += 0.05f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.OemMinus) && _camera.Zoom > 0.1f)
            {
                _camera.Zoom -= 0.05f;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                foreach (Room item in rooms)
                {
                    RectangleF rectangle = (RectangleF)item.Bounds;
                    if ((rectangle.Width / tileSize + rectangle.Height / tileSize) / 2 > 20)
                    {
                        item.Color = Color.Red;
                    }

                    points = new List<Point2>();
                    foreach (Room item1 in rooms)
                    {
                        if (item1.Color == Color.Red)
                        {
                            points.Add(item1.GetCenter());

                        }
                    }




                }

                foreach (Room item1 in rooms.ToArray())
                {
                    foreach (Room item2 in rooms.ToArray())
                    {
                        if (item1.Bounds.Intersects(item2.Bounds) && item2 != item1 && item1.Color != Color.Red)
                        {
                            rooms.Remove(item1);
                        }
                    }
                }
                if (points.Count > 0)
                    if (rooms.Where(x => x.Color == Color.Red).Count() > 1)
                        if (!isLinesReady)
                        {
                            GetConnectors();

                            isLinesReady = true;
                        }
                BuildConnectors();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                _collisionComponent.Update(gameTime);
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        List<Point2> points;
        List<Line> lines = new List<Line>();
        private bool isLinesReady;

        void GetConnectors()
        {
            Debug.WriteLine(rooms.Where(x => x.Color == Color.Red).Count());
            int cointer = 0;

            foreach (var room in rooms)
            {
                IShapeF circleShape = new CircleF(room.GetCenter(), tileSize * 60);
                CircleF c = (CircleF)circleShape;
                cointer = 0;

                do
                {
                    circleShape = new CircleF(room.GetCenter(), c.Radius + tileSize * 2);
                    c = (CircleF)circleShape;
                    foreach (var point in points)
                    {
                        var s = new CircleF(point, 4);
                        if (c.Intersects(s) && room.Color == Color.Red && point != room.GetCenter().ToPoint())
                        {
                            Line line = new Line();
                            line.Point1 = room.GetCenter();
                            line.Point2 = point;
                            room.IsConnected = true;
                            lines.Add(line);
                            cointer++;
                            if (Random.Next(100) <= 15)
                            {
                                if (cointer > 3)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (cointer > 1)
                                {
                                    break;
                                }
                            }

                        }
                    }
                } while (cointer < 3 && c.Radius < 500);

            }


            foreach (var room in rooms.ToArray())
            {
                if (!room.IsConnected)
                {
                    rooms.Remove(room);
                }

            }




        }
        public static double ConvertRadiansToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }
        public static float ConvertDegreesToRadians(float degrees)
        {
            return (float)((Math.PI / 180) * degrees);
        }
        void BuildConnectors()
        {
            List<Room> mainRooms = new List<Room>();
            foreach (var room in rooms)
            {
                if (room.Color == Color.Red)
                {
                    mainRooms.Add(room);

                }
            }
            foreach (var line in lines)
            {
                var room1 = rooms.Where(x => x.GetCenter() == line.Point1).FirstOrDefault();
                var room2 = rooms.Where(x => x.GetCenter() == line.Point2).FirstOrDefault();

                var angle = Math.Atan((line.Point1.Y - line.Point2.Y) / (line.Point1.X - line.Point2.X));
                angle = Math.Atan2(line.Point2.X, line.Point1.X) - Math.Atan2(line.Point2.Y, line.Point1.Y);
                angle = Math.Atan2(line.Point2.Y - line.Point1.Y, line.Point2.X - line.Point1.X);
                if (angle < 0) { angle += 2 * Math.PI; }
                //if (ConvertRadiansToDegrees(angle) < 0)
                //{
                //    angle = 360 - ConvertRadiansToDegrees(angle);
                //    angle = ConvertDegreesToRadians((float)angle);
                //}
                //if (ConvertRadiansToDegrees(angle) > 360)
                //{
                //    angle = ConvertRadiansToDegrees(angle) - 360;
                //    angle = ConvertDegreesToRadians((float)angle);
                //}
                Debug.WriteLine(ConvertRadiansToDegrees(angle) + "   " + angle);

                //if (Math.Abs(ConvertRadiansToDegrees(angle)) >= 45 && Math.Abs(ConvertRadiansToDegrees(angle)) < 135)
                //{
                //    RectangleF r = (RectangleF)room1.Bounds;
                //    var rect = new RectangleF(r.Center.X, room1.Bounds.Position.Y + r.Height, 32, 32);
                //    rooms.Add(new Room(rect));
                //}
                if (Math.Abs(ConvertRadiansToDegrees(angle)) >= 225 && Math.Abs(ConvertRadiansToDegrees(angle)) < 315)
                {
                    RectangleF r1 = (RectangleF)room1.Bounds;
                    RectangleF r2 = (RectangleF)room2.Bounds;
                    var connector = new RectangleF(r2.Center.X, r2.Y + r2.Height - tileSize, tileSize, tileSize);

                    float d = r1.Y - r2.Y - r2.Height + tileSize * 2;
                    connector.Size = new Size2(32, d);
                    //RectangleF
                    //Room connector = new Room(rect);
                    rooms.Add(new Room(connector));

                    //if (connector.Intersects(r1) && connector.Intersects(r2)  )
                    //{

                    //}
                    //else
                    //{


                    //    if (r1.Position.X > r2.Position.X)
                    //    {
                    //        d = r1.X-r2.X-r2.Width/2;

                    //        connector.Position = new Point2(connector.X, connector.Y + connector.Height);
                    //        connector.Size = new Size2(d, 32 * 2);
                    //        rooms.Add(new Room(connector));
                    //    }
                    //    else
                    //    {
                    //        connector.Position = new Point2(connector.X, connector.Y + connector.Height);
                    //        connector.Size = new Size2(32 * 2, 32 * 2);
                    //        rooms.Add(new Room(connector));
                    //    }
                    //}
                    

                }
                //if (Math.Abs(ConvertRadiansToDegrees(angle)) >= 135 && Math.Abs(ConvertRadiansToDegrees(angle)) < 225)
                //{
                //    RectangleF r = (RectangleF)room1.Bounds;
                //    var rect = new RectangleF(r.X, r.Center.Y, 32, 32);
                //    rooms.Add(new Room(rect));
                //}
                //if (Math.Abs(ConvertRadiansToDegrees(angle)) >= 315 && Math.Abs(ConvertRadiansToDegrees(angle)) < 360 ||
                //    Math.Abs(ConvertRadiansToDegrees(angle)) >= 0 && Math.Abs(ConvertRadiansToDegrees(angle)) < 45)
                //{
                //    RectangleF r = (RectangleF)room1.Bounds;
                //    var rect = new RectangleF(r.X + r.Width, r.Center.Y, 32, 32);
                //    rooms.Add(new Room(rect));
                //}
                // room2.Bounds.Intersects()

            }
            for (int i = 0; i < mainRooms.Count; i++)
            {

            }

            //RectangleF r1 = (RectangleF)mainRooms[0].Bounds;
            //RectangleF r2 = (RectangleF)mainRooms[1].Bounds;
            //var d = r2.Center.X + r2.Width * 1.5f - r1.Center.X - r1.Width;
            //RectangleF connector = new RectangleF();

            //if (r1.Position.X < r2.Position.X)
            //{

            //    connector.Position = new Point2(r1.Center.X - 1 + r1.Width / 2, r1.Center.Y - 4);
            //    connector.Size = new Size2(d + 2 - r1.Width / 2 - r2.Width / 2, 8);
            //    rooms.Add(new Room(connector));
            //    if (connector.Intersects(r1) && connector.Intersects(r2))
            //    {

            //    }
            //    else
            //    {
            //        if (r1.Position.Y < r2.Position.Y)
            //        {
            //            connector.Position = new Point2(connector.X + connector.Width - 1, connector.Y);
            //            d = r2.Y - connector.Y + 1;
            //            connector.Size = new Size2(8, d);
            //            rooms.Add(new Room(connector));
            //        }
            //        else
            //        {
            //            d = connector.Y - r2.Y - r2.Height + connector.Height + 1;
            //            connector.Position = new Point2(connector.X + connector.Width - 1, r2.Y + r2.Height - 1);

            //            connector.Size = new Size2(8, d);
            //            rooms.Add(new Room(connector));

            //        }
            //    }

            //}
            //else
            //{
            //}





        }


        protected override void Draw(GameTime gameTime)
        {

            transformMatrix = _camera.GetViewMatrix();
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(blendState: BlendState.AlphaBlend, sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp, transformMatrix: transformMatrix);
            CircleF c = (CircleF)circle;
            //_spriteBatch.DrawLine(c.Center, 1000, ConvertDegreesToRadians(225), Color.Red, 50);
            _spriteBatch.DrawRectangle(new Vector2(circle.Position.X - c.Radius, circle.Position.Y - c.Radius), new Size2(tileSize * 2, tileSize * 2), Color.Black, 5);
            //_spriteBatch.DrawCircle((CircleF)circle, 50, Color.Black, 10);
            foreach (Room item in rooms)
            {
                item.Draw(_spriteBatch);
            }


            //foreach (var item in points)
            //{
            //    _spriteBatch.DrawPoint(item, Color.Yellow, 20);
            //    _spriteBatch.DrawLine(points[1], points[2], Color.Yellow, 8);
            //}

            foreach (var item in lines)
            {
                _spriteBatch.DrawLine(item.Point1, item.Point2, Color.Yellow, 8);
            }


            // _spriteBatch.DrawPolygon(Vector2.Zero, p, Color.Yellow, 20);
            //for (int i = 0; i < points.Count ; i++)
            //{
            //    for (int j = 0; j < points.Count; j++)
            //    {
            //        _spriteBatch.DrawLine(points[i], points[j], Color.Yellow, 8);
            //    }
            //}
            //foreach (RectangleF item in rectangles)
            //{
            //    _spriteBatch.DrawRectangle(item, Color.Black);
            //}
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
    class Line
    {
        public Vector2 Point1;
        public Vector2 Point2;

        internal float GetLenght()
        {
            return Vector2.Distance(Point1, Point2);
        }
    }
}
