using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez.Sprites;
using Microsoft.Xna.Framework;

namespace Stellar_Outpost.Components
{
    internal class Player : Component, IUpdatable
    {
        static float GravityAcceleration = 190.8f;
        static float MaxFallSpeed = 550;

        Vector2 velocity = Vector2.Zero;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            var texture = Entity.Scene.Content.LoadTexture(Content.Player);
            Entity.AddComponent(new SpriteRenderer(texture));
            Entity.AddComponent<Mover>();
            Entity.AddComponent<BoxCollider>();
            Entity.Position = new Vector2(40, 90);

            
        }

        void IUpdatable.Update()
        {
            ApplyPhysics();
        }

        void ApplyPhysics()
        {
            float elapsed = Time.DeltaTime;

            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            Debug.Log($"Velocity: {velocity.Y}");
            var deltaMovement = velocity * elapsed;
            HandleCollisions(ref deltaMovement);
            Entity.Position += deltaMovement;
            Entity.Position = new Vector2((float)Math.Round(Entity.Position.X), (float)Math.Round(Entity.Position.Y));
        }

        void HandleCollisions(ref Vector2 deltaMovement)
        {
            CollisionResult collisionResult;
            if (Entity.GetComponent<Collider>().CollidesWithAny(ref deltaMovement, out collisionResult))
            {
                // log the CollisionResult. You may want to use it to add some particle effects or anything else relevant to your game.
                //Debug.Log("collision result: {0}", collisionResult);
            }
        }
    }
}
