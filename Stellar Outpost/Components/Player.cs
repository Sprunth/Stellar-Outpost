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
    // inspiration: https://github.com/MonoGame/MonoGame.Samples/blob/develop/Platformer2D/Platformer2D.Core/Game/Player.cs
    internal class Player : Component, IUpdatable
    {
        private const float MoveAcceleration = 130.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float JumpControlPower = 0.14f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        Vector2 velocity = Vector2.Zero;
        bool isJumping, wasJumping;
        float jumpTime;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            var texture = Entity.Scene.Content.LoadTexture(Content.Player);
            Entity.AddComponent(new SpriteRenderer(texture));
            Entity.AddComponent<Mover>();
            Entity.AddComponent<BoxCollider>();
            Entity.Position = new Vector2(640, 90);

            
        }

        void IUpdatable.Update()
        {
            GetInput();
            ApplyPhysics();

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        void GetInput()
        {
            if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                movement = -1;
            } else if (Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                movement = 1;
            } else
            {
                movement = 0;
            }

            isJumping = Input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space);
            if (isJumping)
            {
                Debug.Log("{0} jumping", Time.FrameCount);
            }
        }

        void ApplyPhysics()
        {
            float elapsed = Time.DeltaTime;
            velocity.X += movement * MoveAcceleration * elapsed;
            if (IsOnGround)
            {
                velocity.Y = 0;
            }
            else
            {
                velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            }
            
            
            
            velocity.Y = DoJump(velocity.Y);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            var deltaMovement = velocity * elapsed;
            // Prevent the player from running faster than his top speed.            
            deltaMovement.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            HandleCollisions(ref deltaMovement);

            Entity.Position += deltaMovement;
            Entity.Position = new Vector2((float)Math.Round(Entity.Position.X), (float)Math.Round(Entity.Position.Y));

            
        }

        void HandleCollisions(ref Vector2 deltaMovement)
        {
            // Reset flag to search for ground collision.
            isOnGround = false;

            var collider = Entity.GetComponent<Collider>();
            //var neighborColliders = Physics.BoxcastBroadphaseExcludingSelf(collider);
            //foreach(var opposingCollider in neighborColliders)
            //{
            //    //var depth = RectangleF.GetIntersectionDepth(collider.Bounds, opposingCollider.Bounds);
            //    //if (depth != Vector2.Zero)
            //    //{
            //    //    Entity.Position -= depth;
            //    //}

            //    CollisionResult cr;
            //    if (collider.CollidesWith(opposingCollider, deltaMovement, out cr))
            //    {
            //        deltaMovement -= cr.MinimumTranslationVector;
            //    }
            //}

            CollisionResult cr;
            collider.CollidesWithAny(ref deltaMovement, out cr);
        }

        float DoJump(float velocityY)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    //if (jumpTime == 0.0f)
                    //    jumpSound.Play();

                    jumpTime += (float)Time.DeltaTime;
                    //sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }
    }
}
