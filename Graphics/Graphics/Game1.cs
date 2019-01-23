using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Sample;
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

        List<SimpleModel> gameObjects = new List<SimpleModel>();
        Camera mainCamera;

        SpriteBatch spriteBatch;
        SpriteFont sfont;

        OcclusionQuery occQuery;
        Stopwatch time = new Stopwatch();
        long totalTime = 0;

        int objectsDrawn;

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

            mainCamera = new Camera("cam", new Vector3(0, 5, 10), new Vector3(0, 0, -1));
            mainCamera.Initialize();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            sfont = Content.Load<SpriteFont>("debug");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            gameObjects.Add(new SimpleModel("wall0", "wall", new Vector3(0, 0, -10)));
            gameObjects.Add(new SimpleModel("ball0", "ball", new Vector3(0, 2.5f, -40)));

            gameObjects.ForEach(go => go.LoadContent());
        }
        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            mainCamera.Update();

            if(InputEngine.IsKeyHeld(Keys.Escape))
            {
                Exit();
            }

            gameObjects.ForEach(go => go.Update());

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            totalTime = 0;

            time.Reset();

            foreach (var item in gameObjects)
            {
                if(FrustumContains(item))
                {
                    if(!IsOccluded(item))
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
            GameUtilities.SetGraphicsDeviceFor3D();

            base.Draw(gameTime);
        }

        private void AddModel(SimpleModel simpleModel)
        {
            simpleModel.Initialize();
            simpleModel.LoadContent();
            gameObjects.Add(simpleModel);
        }

        private bool FrustumContains(SimpleModel simpleModel)
        {
            bool inFrustum = false;

            //foreach (var go in gameObjects)
            //{
                if(mainCamera.Frustum.Contains(simpleModel.AABB) != ContainmentType.Disjoint)
                {
                    inFrustum = true;
                }
            //}

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
