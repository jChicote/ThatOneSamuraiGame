﻿using System.Collections;
using Enemies.Enemy_States;
using Enemy_Scripts;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyType
{
    SWORDSMAN,
    ARCHER,
    GLAIVEWIELDER,
    BOSS,
    TUTORIALENEMY
}

namespace Enemies
{
    // AI SYSTEM INFO
    // AISystem is responsible for receiving calls to tell the enemy what to perform. It should also
    // Be responsible for storing enemy data (i.e. Guard meter, remaining guard etc.) BUT
    // any enemy behaviours should be handled through the state machine
    public class AISystem : EnemyStateMachine
    {
        
        #region Fields and Properties
        
        //ENEMY TYPE, SET IN PREFAB INSPECTOR
        public EnemyType enemyType;
        //WEAPON COLLIDER, SET IN PREFAB INSPECTOR
        public CapsuleCollider meleeCollider;

        //ENEMY SETTINGS [See EntityStatData for list of stats]
        public EnemySettings enemySettings; // Taken from EnemySettings Scriptable object in start
        public StatHandler statHandler;
        public EnemyTracker enemyTracker;
        
        //ANIMATOR
        public Animator animator;
        public bool bPlayerFound = false;
        
        //NAVMESH
        public NavMeshAgent navMeshAgent;
        
        //DAMAGE CONTROLS
        public EDamageController eDamageController;
        public bool bIsDead = false;
        public bool bIsUnblockable = false;
        //NOTE: isStunned is handled in Guarding script, inside the eDamageController script
        
        //DODGE VARIABLES
        public float dodgeDirectionX, dodgeDirectionZ = 0;

        //Float offset added to the target location so the enemy doesn't clip into the floor 
        //because the player's origin point is on the floor
        public Vector3 floatOffset = Vector3.up * 2.0f;
        
        #endregion
        
        #region Unity Monobehaviour Functions

        private void Start()
        {
            // Grab the enemy settings from the Game Manager > Game Settings > Enemy Settings
            enemySettings = GameManager.instance.gameSettings.enemySettings;
            
            // Get the enemy tracker
            enemyTracker = GameManager.instance.enemyTracker;

            // Set up animator parameters
            animator = GetComponent<Animator>();
            animator.SetFloat("ApproachSpeedMultiplier", enemySettings.enemyData.moveSpeed);
            
            // Set up nav mesh parameters
            navMeshAgent = GetComponent<NavMeshAgent>();
            
            // Set up Damage Controller
            eDamageController = GetComponent<EDamageController>();
            statHandler = new StatHandler(); // Stat handler = stats that can be modified
            statHandler.Init(enemySettings.enemyData); // enemySettings.enemyData = initial scriptable objects values
            eDamageController.Init(statHandler);
            eDamageController.EnableDamage();

            // Start the enemy in an idle state
            OnIdle();
        }

        #endregion
        
        #region Enemy Utility Funcitons

        public void ApplyHit(GameObject attacker)
        {
            if (attacker.GetComponent<AISystem>())
            {
                Debug.Log("Friendly Fire hit");
            }
            else if (attacker.GetComponent<PlayerController>())
            {
                OnEnemyDeath();
            }
            else
            {
                Debug.LogWarning("Unknown attacker");
            }
        }

        // Called from dodgestate
        public void DodgeImpulse(Vector3 lastDir, float force)
        {
            StopAllCoroutines();
            StartCoroutine(DodgeImpulseCoroutine(lastDir, force));
        }

        // Called from an animation event in lightattackstate
        public void DodgeImpulseAnimationEvent()
        {
            StopAllCoroutines();
            if (Vector3.Distance(transform.position, enemySettings.GetTarget().position) > enemySettings.followUpAttackRange)
            {
                StartCoroutine(DodgeImpulseCoroutine(transform.parent.forward, enemySettings.dodgeForce));
            }
        }
        
        // Coroutines cannot exist in enemystate since it's not a monobehavior, so we handle it here
        private IEnumerator DodgeImpulseCoroutine(Vector3 lastDir, float force)
        {
            float dodgeTimer = .15f;
            while (dodgeTimer > 0f)
            {
                transform.Translate(lastDir.normalized * force * Time.deltaTime);
                
                dodgeTimer -= Time.deltaTime;
                yield return null;
            }
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
        
        public void OnQuickBlock()
        {
            SetState(new QuickBlockEnemyState(this));
        }

        public void OnBlock()
        {
            SetState(new BlockEnemyState(this));
        }

        public void OnParry()
        {
            SetState(new ParryEnemyState(this));
        }

        public void OnDodge()
        {
            SetState(new DodgeEnemyState(this));
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
            SetState(new CircleEnemyState(this));
        }

        public void OnEnemyStun()
        {
            SetState(new StunEnemyState(this));
        }
        
        public void OnParryStun()
        {
            SetState(new ParryStunEnemyState(this));
        }

        public void OnEnemyRecovery()
        {
            SetState(new RecoveryEnemyState(this));
        }

        public void OnEnemyDeath()
        {
            SetState(new DeathEnemyState(this));
        }

        public void OnEnemyRewind() 
        {
            SetState(new RewindEnemyState(this));
        }

        #endregion
    }
}
