using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using Stellar_Outpost.Components;
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

			var player = scene.CreateEntity("player");
			player.AddComponent(new Player());


			var grassTex = Content.LoadTexture(Nez.Content.Grass);
			var grass = scene.CreateEntity("grass");
			grass.AddComponent(new SpriteRenderer(grassTex));
			grass.AddComponent(new BoxCollider());
			grass.Position = new Vector2(40, 410);
			


			scene.Camera.Zoom = 0;
			Scene = scene;

		}
	}
}
