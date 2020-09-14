﻿using System.Collections;
using Enemy_Scripts;
using UnityEngine;

namespace Enemies.Enemy_States
{
    public class ApproachPlayerEnemyState : EnemyState
    {
        private float _stopApproachingRange;
        private Vector3 _target;
        
        //Class constructor
        public ApproachPlayerEnemyState(AISystem aiSystem) : base(aiSystem)
        {
        }

        public override IEnumerator BeginState()
        {
            // Start the navMeshAgent tracking
            AISystem.navMeshAgent.isStopped = false;
            
            AISystem.animator.SetBool("IsApproaching", true);
            
            // Set player to be found in AISystem
            AISystem.bPlayerFound = true;
            AISystem.animator.SetBool("PlayerFound", true);
            
            // Get stopApproachRange, target object and current enemy transform
            _stopApproachingRange = AISystem.enemySettings.stopApproachingRange;
            _target = AISystem.enemySettings.GetTarget().position + AISystem.floatOffset;

            PositionTowardsTarget(AISystem.transform, _target);
            
            yield break;
        }

        public override void Tick()
        {
            // Get the true target point (float offset is added to get a more accurate player-enemy target point)
            _target = AISystem.enemySettings.GetTarget().position + AISystem.floatOffset;

            // Enemy movement itself is handled with root motion and navMeshAgent set destination
            AISystem.navMeshAgent.SetDestination(_target);
            
            // Change to circling state when close enough to the player
            if (InRange(AISystem.transform.position, _target, _stopApproachingRange))
            {
                EndState();
            }
        }

        public override void EndState()
        {
            AISystem.animator.SetBool("IsApproaching", false);
            
            AISystem.OnCirclePlayer();
        }
    }
}
