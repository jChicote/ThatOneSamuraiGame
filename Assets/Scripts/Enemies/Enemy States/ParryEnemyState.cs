﻿using System.Collections;
using UnityEngine;

namespace Enemies.Enemy_States
{
    public class ParryEnemyState : EnemyState
    {
        private Vector3 _target;

        //Class constructor
        public ParryEnemyState(AISystem aiSystem) : base(aiSystem)
        {
        }

        public override IEnumerator BeginState()
        {
            AISystem.attackIndicator.ShowIndicator();
            
            // Disable canParry
            AISystem.eDamageController.enemyGuard.canParry = false;
            
            // Stop the enemy block sword effect and play the parry effect
            AISystem.swordEffects.EndBlockEffect();
            AISystem.parryEffects.PlayParry();
            
            // Make a decision to determine the next move
            int decision = Random.Range(0, 4);

            if (AISystem.enemyType == EnemyType.TUTORIALENEMY) //TUTORIAL ENEMIES CANNOT USE UNBLOCKABLE
                decision = 0;

            if (decision == 0 || decision == 1) // Normal Attack
            {
                // Set the attack trigger
                Animator.SetTrigger("TriggerLightAttack");
            }
            else if (decision == 2) // Heavy attack
            {
                if (AISystem.enemyType == EnemyType.GLAIVEWIELDER)
                {
                    Animator.SetTrigger("HeavyAttack");
                    AISystem.BeginUnblockable();
                }
                else
                {
                    Animator.SetTrigger("TriggerThrust");
                    AISystem.BeginUnblockable();
                }
            }
            
            else if (decision == 3) // Counter attack
            {
                Animator.SetTrigger("TriggerCounterAttack");

            }

            yield break;
            
            // NOTE: End state is called through an animation event at the end of the attack animation
        }
        
        public override void Tick()
        {
            // Get target position and face towards it
            // _target = AISystem.enemySettings.GetTarget().position + AISystem.floatOffset;
            // PositionTowardsTarget(AISystem.transform, _target);
        }

        // End state is called by animation event
        public override void EndState()
        {
            AISystem.attackIndicator.HideIndicator();
            
            // Ensure rotate to player is set back in end state
            bIsRotating = true;
            
            // End the unblockable sword and effect
            AISystem.EndUnblockable();

            ChooseActionUsingDistance(AISystem.enemySettings.GetTarget().position + AISystem.floatOffset);
        }
    }
}
