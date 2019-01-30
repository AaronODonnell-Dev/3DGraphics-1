using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sample;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Graphics
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;

        InputEngine input;
        DebugEngine debug;
        ImmediateShapeDrawer shapeDrawer;

        List<GameObject3D> gameObjects = new List<GameObject3D>();
        Camera mainCamera;

        SpriteBatch spriteBatch;
        QuadTree quadTree;
        Octree octree;
        SpriteFont sfont;

        OcclusionQuery occQuery;
        Stopwatch time = new Stopwatch();
        long totalTime = 0;

        int objectsDrawn, totalObjects;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 768;
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();

            input = new InputEngine(this);
            debug = new DebugEngine();
            shapeDrawer = new ImmediateShapeDrawer();

            IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            GameUtilities.Content = Content;
            GameUtilities.GraphicsDevice = GraphicsDevice;

            debug.Initialize();
            shapeDrawer.Initialize();

            occQuery = new OcclusionQuery(GraphicsDevice);

            mainCamera = new Camera("cam", new Vector3(0, 0, 100), new Vector3(0, 0, -1));
            mainCamera.Initialize();

            quadTree = new QuadTree(100, Vector2.Zero, 5);
            octree = new Octree(100, Vector3.Zero, 5);
           // quadTree.SubDivide();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            sfont = Content.Load<SpriteFont>("debug");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Random ran = new Random();

            for (int i = 0; i < 1000; i++)
            {
                float x = ran.Next(-50, 50);
                float y = ran.Next(-50, 50);
                float z = ran.Next(-50, 50);

                AddModel(new SimpleModel("", "ball", new Vector3(x, y, z)));
            }

            //gameObjects.Add(new SimpleModel("wall0", "wall", new Vector3(0, 0, -10)));
            //gameObjects.Add(new SimpleModel("ball0", "ball", new Vector3(0, 2.5f, -40)));

            //gameObjects.ForEach(go => go.LoadContent());
        }
        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            GameUtilities.Time = gameTime;

            if(InputEngine.IsKeyHeld(Keys.Escape))
            {
                Exit();
            }

            mainCamera.Update();
            gameObjects.Clear();
            //quadTree.Process(mainCamera.Frustum, ref gameObjects);
            octree.Process(mainCamera.Frustum, ref gameObjects);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.HotPink);

            debug.Draw(mainCamera);

            time.Reset();

            foreach (SimpleModel item in gameObjects)
            {
                if (FrustumContains(item))
                {
                    if (!IsOccluded(item))
                    {
                        item.Draw(mainCamera);
                        objectsDrawn++;
                    }
                }
            }
            spriteBatch.Begin();

            spriteBatch.DrawString(
                sfont,
                "Objects Drawn: " + objectsDrawn + " Occlusion Time: " + totalTime,
                new Vector2(10, 10),
                Color.White);

            spriteBatch.End();
            
            objectsDrawn = 0;
            totalTime = 0;

            GameUtilities.SetGraphicsDeviceFor3D();

            base.Draw(gameTime);
        }

        private void AddModel(SimpleModel simpleModel)
        {
            simpleModel.Initialize();
            simpleModel.LoadContent();
            //gameObjects.Add(simpleModel);
            //quadTree.AddObject(simpleModel);
            octree.AddObject(simpleModel);
            totalObjects++;
            
        }

        private bool FrustumContains(SimpleModel simpleModel)
        {
            bool inFrustum = false;

            foreach (var go in gameObjects)
            {
                if (mainCamera.Frustum.Contains(simpleModel.AABB) != ContainmentType.Disjoint)
                {
                    inFrustum = true;
                }
            }

            return inFrustum;
        }

        private bool IsOccluded(SimpleModel go)
        {
            bool value = true;
            
            occQuery.Begin();
            shapeDrawer.DrawBoundingBox(go.AABB, mainCamera);
            occQuery.End();

            while(!occQuery.IsComplete) { }

            if (occQuery.IsComplete && occQuery.PixelCount > 0)
                value = false;

            time.Stop();

            totalTime += time.ElapsedMilliseconds; 

            return value;
        }
    }
}
