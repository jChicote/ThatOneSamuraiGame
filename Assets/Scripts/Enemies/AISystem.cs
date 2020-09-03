﻿using System;
using System.Collections;
using Enemies.Enemy_States;
using Enemy_Scripts;
using UnityEngine;
using UnityEngine.AI;

public interface IEnemyStates
{
    void OnEnemyStun();
    void OnApproachPlayer();
}

namespace Enemies
{
    // AI SYSTEM INFO
    // AISystem is responsible for receiving calls to tell the enemy what to perform. It should also
    // Be responsible for storing enemy data (i.e. Guard meter, remaining guard etc.) BUT
    // any enemy behaviours should be handled through the state machine
    public class AISystem : EnemyStateMachine, IEnemyStates
    {
        #region Fields and Properties

        //ENEMY SETTINGS [See EntityStatData for list of stats]
        public EnemySettings enemySettings; // Taken from EnemySettings Scriptable object in start
        public StatHandler statHandler;
        private EnemyTracker _enemyTracker;
        
        //ANIMATOR
        public Animator animator;
        public bool bPlayerFound = false;
        
        //NAVMESH
        public NavMeshAgent navMeshAgent;
        
        //DAMAGE CONTROLS
        private EDamageController _eDamageController;
        public bool bIsDead = false;

        //Float offset added to the target location so the enemy doesn't clip into the floor 
        //because the player's origin point is on the floor
        public Vector3 floatOffset = Vector3.up * 2.0f;
        
        #endregion

        #region Unity Monobehaviour Functions

        private void Start()
        {
            // Grab the enemy settings from the Game Manager > Game Settings > Enemy Settings
            enemySettings = GameManager.instance.gameSettings.enemySettings;

            // Start the enemy in an idle state
            SetState(new IdleEnemyState(this));
            
            // Set up animator parameters
            animator = GetComponent<Animator>();
            animator.SetFloat("ApproachSpeedMultiplier", enemySettings.enemyData.moveSpeed);
            
            // Set up nav mesh parameters
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            // Set up Damage Controller
            _eDamageController = GetComponent<EDamageController>();
            statHandler = new StatHandler(); // Stat handler = stats that can be modified
            statHandler.Init(enemySettings.enemyData); // enemySettings.enemyData = initial scriptable objects values
            _eDamageController.Init(statHandler);
            _eDamageController.EnableDamage();
        }
        
        protected new void Update()
        {
            base.Update();
        }

        #endregion
        
        #region Enemy Utility Funcitons

        public float GetAnimationLength(string animationName)
        {
            AnimationClip animationClip = new AnimationClip();
            bool bFoundClip = false;
            
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == animationName)
                {
                    animationClip = clip;
                    bFoundClip = true;
                    break;
                }
            }

            if (bFoundClip)
            {
                return animationClip.length;
            }
            else
            {
                Debug.LogWarning("Animation " + animationName + " could not be found");
                return 0;
            }
        }
        
        public void ApplyHit(GameObject attacker)
        {
            // if (bIsParrying)
            // {
            //     TriggerParry(attacker); 
            // }
            // else if (bIsBlocking)
            // {
            //     TriggerBlock(attacker); 
            // }
            // else
            // {
            //     KillPlayer();
            // }
            if(attacker.GetComponent<AISystem>())
                Debug.Log("Friendly Fire hit");
            else if (attacker.GetComponent<PlayerController>())
            {
                Debug.Log("Enemy dead");
                bIsDead = true;
                TempWinTracker.instance.enemyCount--;
                navMeshAgent.SetDestination(transform.position);
                animator.SetBool("isDead", true);
                OnEnemyDeath();
                StartCoroutine(RemovePlayerFromScene());
            }
            else
                Debug.LogWarning("Unknown attacker");
        }

        public IEnumerator RemovePlayerFromScene()
        {
            _enemyTracker = GameManager.instance.enemyTracker;
            _enemyTracker.RemoveEnemy(transform);
            
            EDamageController eDamageController = GetComponent<EDamageController>();
            eDamageController.DisableDamage();
            
            yield return new WaitForSeconds(2.0f);
            gameObject.SetActive(false);
        }

        #endregion
        
        // ENEMY STATE SWITCHING INFO
        // Any time an enemy gets a combat maneuver called, their state will switch
        // Upon switching states, they override the EnemyState Start() method to perform their action
        
        #region Enemy Combat Manuervers
        
        public void OnLightAttack()
        {
            SetState(new LightAttackEnemyState(this));
        }

        public void OnHeavyAttack()
        {
        
        }

        public void OnSpecialAttack()
        {
        
        }

        public void OnBlock()
        {
            SetState(new BlockEnemyState(this));
        }

        public void OnParry()
        {
        
        }

        public void OnDodge()
        {
        
        }
    
        #endregion

        #region Enemy Movement

        public void OnIdle()
        {
            SetState(new IdleEnemyState(this));
        }

        public void OnPatrol()
        {
        
        }

        public void OnApproachPlayer()
        {
            SetState(new ApproachPlayerEnemyState(this));
        }

        public void OnCirclePlayer()
        {
        
        }

        public void OnEnemyStun()
        {
            animator.SetBool("IsGuardBroken", true);
            SetState(new EnemyStunState(this));
        }

        public void OnEnemyRecovery()
        {
            
        }

        public void OnEnemyDeath()
        {
            SetState(new EnemyDeathState(this));
        }

        #endregion
    }
}
