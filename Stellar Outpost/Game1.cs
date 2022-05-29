using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Farseer;
using Nez.Sprites;
using Stellar_Outpost.Components;
using System.Collections;

namespace Stellar_Outpost
{
    public class Game1 : Core
    {
		public Game1() : base(1440, 600)
        {
			Core.DebugRenderEnabled = true;
		}
		
		protected override void Initialize()
		{
			base.Initialize();

			Screen.SetSize(1440, 600);
			

			var scene = Scene.CreateWithDefaultRenderer(Color.LightSkyBlue);


			// create a physics world to manage the physics simulation
			var world = scene.AddSceneComponent<FSWorld>()
				.SetEnableMousePicking(true);
			world.SetEnabled(true);

			scene.CreateEntity("debug-view")
				.AddComponent(new FSDebugView(world))
				.AppendFlags(FSDebugView.DebugViewFlags.ContactPoints);


			StartCoroutine(spawnPlayer());

			var grassTex = Content.LoadTexture(Nez.Content.Grass);
			
			for (var i=0; i< 5; i++)
            {
				var grass = scene.CreateEntity($"grass-{i}");
				grass.AddComponent(new SpriteRenderer(grassTex))
					 .AddComponent<FSRigidBody>()
					 .SetBodyType(FarseerPhysics.Dynamics.BodyType.Static)
					 .AddComponent<FSCollisionBox>()
					 .SetSize(grassTex.Width, grassTex.Height);
				grass.Position = new Vector2(640 + i*grassTex.Width, 410);
			}

			


			scene.Camera.Zoom = 0;
			Scene = scene;

		}

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
			Scene.GetSceneComponent<FSWorld>().Update();
        }

        IEnumerator spawnPlayer()
        {
			yield return Coroutine.WaitForSeconds(0.3f);

			var player = Scene.CreateEntity("player");
			player.AddComponent(new Player());
		}
	}
}
