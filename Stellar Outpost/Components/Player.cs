using Nez;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nez.Sprites;
using Microsoft.Xna.Framework;
using Nez.Textures;

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
        private const float JumpLaunchVelocity = -950.0f;
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
        bool isJumping, wasJumping = false;
        float jumpTime;

        int animationIndex = 0;
        List<Sprite> IdleSprites;

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            //var texture = Entity.Scene.Content.LoadTexture(Content.Player);
            var texture = Entity.Scene.Content.LoadTexture(Content.RangerIdle);
            IdleSprites = Sprite.SpritesFromAtlas(texture, 48, 48);
            Entity.AddComponent(new SpriteRenderer(IdleSprites[0]));
            Entity.AddComponent<Mover>();
            Entity.AddComponent<BoxCollider>();
            var collider = Entity.GetComponent<BoxCollider>();
            collider.PhysicsLayer = 5;
            //collider.SetLocalOffset(new Vector2(0.1f));
            collider.SetSize(48, 48);
            Entity.Position = new Vector2(640, 90);

            Core.StartCoroutine(RollThroughAnimations());
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


            if (deltaMovement.X > 0)
            {
                Entity.GetComponent<SpriteRenderer>().FlipX = false;
            } else if (deltaMovement.X < 0)
            {
                Entity.GetComponent<SpriteRenderer>().FlipX = true;
            } // else stick with what was already set


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
            if (collider.CollidesWithAny(ref deltaMovement, out cr))
            {
                //isOnGround = true;
            }

            var rayStart = new Vector2(collider.Bounds.Center.X, collider.Bounds.Bottom);
            //Debug.DrawHollowRect(new Rectangle(new Point((int)rayStart.X, (int)rayStart.Y), new Point(20, 5)), Color.Black, 2);
            //Debug.DrawPixel(rayStart, 5, Color.Orange, 2);
            var hit = Physics.Linecast(rayStart, rayStart + new Vector2(0, 3f), 2);
            //Debug.DrawLine(rayStart, rayStart + new Vector2(0, 3f), Color.Orange, 2);
            var overlapRect = new RectangleF(rayStart, new Vector2(12, 4));
            var oppCol = Physics.OverlapRectangle(overlapRect, 2);
            Debug.DrawHollowRect(new Rectangle((int)overlapRect.Left, (int)overlapRect.Top, (int)overlapRect.Width, (int)overlapRect.Height), Color.Orange, 0.3f);
            if (oppCol != null)
            {
                //Debug.Log("overlapRect found collider: {0}", oppCol.Entity.Name);
                isOnGround = true;
            }

            //{
            //    if (collider.CollidesWithAny(out cr))
            //    {
            //        Debug.Log("overlap check found collider: {0}", cr.Collider.Entity.Name);
            //        isOnGround = true;
            //        deltaMovement.Y = -cr.MinimumTranslationVector.Y;
            //    }
            //}

            //if (hit.Collider != null)
            //{
            //    Debug.Log("linecast found collider: {0}", hit.Collider.Entity.Name);
            //    isOnGround = true;
            //    deltaMovement.Y = 0;
            //}
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
                    Debug.Log("Starting or continuing jump");
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                    Debug.Log("still going up in jump");
                }
                else
                {
                    Debug.Log("dud jump {0}. IsOnGround={1}", jumpTime, IsOnGround);
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

        IEnumerator<object> RollThroughAnimations()
        {
            while (true)
            {
                yield return Coroutine.WaitForSeconds(0.2f);

                animationIndex++;

                if (animationIndex >= IdleSprites.Count)
                {
                    animationIndex = 0;
                }
                Entity.GetComponent<SpriteRenderer>().Sprite = IdleSprites[animationIndex];
            }
        }
    }
}
