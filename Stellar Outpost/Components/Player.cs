using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Farseer;
using Nez.Sprites;
using System;

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
        private const float GravityAcceleration = 340.0f;
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
            Entity
                .AddComponent(new SpriteRenderer(texture))
                .AddComponent<FSRigidBody>()
                .SetBodyType(BodyType.Kinematic)
                .SetGravityScale(9.8f)
                .AddComponent<FSCollisionBox>()
                .SetSize(texture.Width, texture.Height);

            var rigidbody = Entity.GetComponent<FSRigidBody>();
            rigidbody.Body.OnCollision += OnBodyCollision;
            rigidbody.Body.OnSeparation += OnBodySeparation;
            rigidbody.Body.FixedRotation = true;
            
            Entity.Position = new Vector2(640, 90);
        }

        private void OnBodySeparation(Fixture fixtureA, Fixture fixtureB)
        {
            Debug.Log("Body collision separation");
            isOnGround = false;
        }

        private bool OnBodyCollision(Fixture fixtureA, Fixture fixtureB, FarseerPhysics.Dynamics.Contacts.Contact contact)
        {
            Debug.Log("Body collision detect");
            isOnGround = true;
            return true;
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
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);
            velocity.Y = DoJump(velocity.Y);
            //velocity.Y = 0;

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            var deltaMovement = velocity * elapsed;
            // Prevent the player from running faster than his top speed.            
            deltaMovement.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            if (isJumping)
            {
                deltaMovement.Y = 5000;
            }

            var body = Entity.GetComponent<FSRigidBody>();
            //body.SetLinearVelocity(deltaMovement);
            

            Entity.Position += deltaMovement;


            //Debug.Log("deltaMovement: {0}", deltaMovement);

            var collider = Entity.GetComponent<FSCollisionBox>();
            

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
