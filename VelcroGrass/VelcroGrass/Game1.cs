using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using VelcroPhysics.Collision.Filtering;
using VelcroPhysics.Dynamics;
using VelcroPhysics.Dynamics.Joints;
using VelcroPhysics.Factories;
using VelcroPhysics.Utilities;

using con = VelcroPhysics.Utilities.ConvertUnits;

namespace VelcroGrass
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch           spriteBatch;

        // ART 
        Rectangle gras1, gras2, gras3, BigGrass;
        Rectangle pixel;
        Texture2D tex;                             // sprite sheet 

        // PHYSICS
        World           world;
        FixedMouseJoint mouse_joint;        
        Body            collider_body;
        Body            big_grass_body;
        //-- grass physics: 
        Body            gras_bod1;
        WeldJoint       gras_joint1;
        Body            gras_bod2;
        WeldJoint       gras_joint2;

        #region GEO        
        VertexPositionColorTexture[] grassVerts;
        short[]      grassInds;
        VertexBuffer vertexBuffer;
        IndexBuffer  indexBuffer;
        #endregion

        // C O N S T R U C T 
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth    = 1024,     PreferredBackBufferHeight = 768,   PreferredBackBufferFormat = SurfaceFormat.Color,
                PreferredDepthStencilFormat = DepthFormat.None,                            SynchronizeWithVerticalRetrace = true,
                GraphicsProfile             = GraphicsProfile.HiDef,
            };
            Content.RootDirectory = "Content";
        }


        // I N I T
        protected override void Initialize()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            pixel = new Rectangle(382, 0, 1, 1);
            gras1 = new Rectangle(12, 144, 45, 92);
            gras2 = new Rectangle(76, 144, 45, 92);
            gras3 = new Rectangle(138, 144, 45, 92);
            BigGrass = new Rectangle(0, 0, 380, 122);

            world = new World(new Vector2(0f, 9.8f)); // world physics sim (provide gravity direction)          

            collider_body = BodyFactory.CreateCircle(world, con.ToSimUnits(8.0f), 1.0f);
            collider_body.Position    = con.ToSimUnits(400, 10);
            collider_body.BodyType    = BodyType.Dynamic; // moves
            collider_body.Mass        = 0.4f;
            collider_body.Restitution = 0.2f;             // bounciness
            collider_body.Friction    = 0.4f;             // grip
            collider_body.CollisionCategories = Category.Cat1; collider_body.CollidesWith = Category.All;
            collider_body.FixedRotation = false;

            // attach collider_body to mouse: 
            mouse_joint = JointFactory.CreateFixedMouseJoint(world, collider_body, collider_body.Position);
            mouse_joint.MaxForce = 500.0f;


            #region 大きな 草
            // <image url="..\..\Images\grass.png" bgcolor="0" /> ookina kusa
            #endregion
            big_grass_body = BodyFactory.CreateRectangle(world, con.ToSimUnits(BigGrass.Width*0.88f), con.ToSimUnits(BigGrass.Height*0.6f), 1.0f);
            big_grass_body.Position = con.ToSimUnits(300f+BigGrass.Width/2f-10, 600+BigGrass.Height/2f);
            big_grass_body.BodyType = BodyType.Static;
            big_grass_body.Restitution = 0.2f;    // bounciness
            big_grass_body.Friction    = 0.8f;    // surface grip
            big_grass_body.Mass        = 1.0f;
            big_grass_body.CollisionCategories = Category.Cat2;
            big_grass_body.CollidesWith        = Category.Cat1;            


            // DYNAMIC GRASS (BOTTOM PART): 
            gras_bod1 = BodyFactory.CreateRectangle(world, con.ToSimUnits(gras1.Width*0.5f), con.ToSimUnits(gras1.Height*0.4f), 1.0f);
            gras_bod1.Position = con.ToSimUnits(360, 600 - gras1.Height*0.2f); // use mid of rect (0.4/2) 
            gras_bod1.BodyType = BodyType.Dynamic;
            gras_bod1.Mass        = 0.2f;
            gras_bod1.Restitution = 0.2f;
            gras_bod1.Friction    = 0.2f;
            gras_bod1.IgnoreGravity = true;
            gras_bod1.CollisionCategories = Category.Cat3; gras_bod1.CollidesWith = Category.Cat1;
           
            float DAMP = 0.23f, HZ = 17.0f;
            gras_joint1 = JointFactory.CreateWeldJoint(world, big_grass_body, gras_bod1,
                con.ToSimUnits(new Vector2(360, 600)), con.ToSimUnits(new Vector2(360, 600)), true);            
            gras_joint1.CollideConnected = false;            
            gras_joint1.FrequencyHz      = HZ;
            gras_joint1.DampingRatio     = DAMP;


            gras_bod2 = BodyFactory.CreateRectangle(world, con.ToSimUnits(gras1.Width * 0.5f), con.ToSimUnits(gras1.Height * 0.4f), 1.0f);
            gras_bod2.Position = con.ToSimUnits(360, 600 - gras1.Height * 0.6);
            gras_bod2.BodyType = BodyType.Dynamic;
            gras_bod2.Mass        = 0.2f;
            gras_bod1.Restitution = 0.2f;
            gras_bod2.Friction    = 0.2f;
            gras_bod2.IgnoreGravity = true;
            gras_bod2.CollisionCategories = Category.Cat3; gras_bod1.CollidesWith = Category.Cat1;
            
            gras_joint2 = JointFactory.CreateWeldJoint(world, gras_bod1, gras_bod2,
                con.ToSimUnits(new Vector2(360, 600 - gras1.Height*0.4f)), con.ToSimUnits(new Vector2(360, 600 - gras1.Height*0.4f)), true);
            gras_joint2.CollideConnected = false;
            gras_joint2.FrequencyHz      = HZ;
            gras_joint2.DampingRatio     = DAMP;           

            base.Initialize();
        }

 
        // L O A D
        protected override void LoadContent()
        {
            tex = Content.Load<Texture2D>("grass");
        }        
        protected override void UnloadContent() {  }


        // U P D A T E
        Vector2 screenpos;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) Exit();
           
            MouseState ms = Mouse.GetState();           
            mouse_joint.WorldAnchorB = ConvertUnits.ToSimUnits(new Vector2(ms.X, ms.Y)); // next mouse position

            //world.Step(0.01666666f); // <-- if assume locked framerate = 60FPS 
            world.Step(Math.Min((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f, (1f / 30f)));

            screenpos = con.ToDisplayUnits(collider_body.Position);

            if (screenpos.Y > 768)  collider_body.SetTransform(new Vector2(collider_body.Position.X, 0f), 0f);
            if (screenpos.Y < 0)    collider_body.SetTransform(new Vector2(collider_body.Position.X, ConvertUnits.ToSimUnits(768)), 0f);
            if (screenpos.X > 1024) collider_body.SetTransform(new Vector2(0f, collider_body.Position.Y), 0f);
            if (screenpos.X < 0)    collider_body.SetTransform(new Vector2(ConvertUnits.ToSimUnits(1024), collider_body.Position.Y), 0f);

            base.Update(gameTime);
        }


        // D R A W 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.TransparentBlack);
            spriteBatch.Begin();
            spriteBatch.Draw(tex, new Vector2(300, 600-20), BigGrass, Color.White);            
            spriteBatch.Draw(tex, screenpos, pixel, Color.Red, collider_body.Rotation, new Vector2(0.5f, 0.5f), new Vector2(9.6f, 9.6f), SpriteEffects.None, 0f);
            Vector2 gras_vector = (con.ToDisplayUnits(gras_bod1.Position) - new Vector2(360, 600)) * 2;
            Vector2 realpos = gras_vector + new Vector2(360,600);          
            spriteBatch.DrawLine(tex, pixel, new Vector2(360, 600), realpos, Color.Green, 2);

            gras_vector = (con.ToDisplayUnits(gras_bod2.Position) - realpos) * 2;
            Vector2 realpos2 = realpos + gras_vector;
            spriteBatch.DrawLine(tex, pixel, realpos, realpos2, Color.Green, 2);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
