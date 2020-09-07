﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---IMPROVEMENTS--- /////////////////////////////////////
//TODO: Smooth velocity on close slide

/// <summary>
/// Handling slide manuvers to target enemy during attacks
/// </summary>
public class AttackSlide
{
    private PCombatController _combatController;
    private Rigidbody _playerRB;
    private PlayerSettings _settings;

    /// <summary>
    /// Attack slide Init function
    /// </summary>
    public void Init(PCombatController controller, Rigidbody playerRB)
    {
        _combatController = controller;
        _playerRB = playerRB;
        _settings = GameManager.instance.gameSettings.playerSettings;
    }

    // Summary: Trigger sliding towards intended enemies
    //
    public void SlideToEnemy(Transform targetEnemy)
    {
        if (CheckWithinMinDist(targetEnemy))
        {
            //Debug.Log(">> AttackSlide: Is within minimum: " + (Vector3.Distance(_playerRB.transform.position, targetEnemy.position) <= _settings.minimumAttackDist));
            return;
        }

        _combatController.StopCoroutine(SlideOverTime(targetEnemy));
        _combatController.StartCoroutine(SlideOverTime(targetEnemy));
    }

    // Summary: Checks whether the player is within the minimum dist against enemy
    //
    private bool CheckWithinMinDist(Transform targetEnemy)
    {
        float distance = Vector3.Distance(_playerRB.transform.position, targetEnemy.position);
        return distance <= _settings.minimumAttackDist;
    }

    private void FaceTowardsEnemy(Transform targetEnemy)
    {
        Vector3 direction = targetEnemy.position - _playerRB.transform.position;
        float angle = Vector3.Angle(_playerRB.transform.forward, direction);
        angle *= Vector3.Dot(_playerRB.transform.right.normalized, direction.normalized) < 0 ? -1 : 1;

        _playerRB.transform.Rotate(0, angle, 0);
    }

    // Summary: Coroutine for sliding player to forward target
    //
    private IEnumerator SlideOverTime(Transform targetEnemy)
    {
        float velocity = _settings.slideSpeed;

        FaceTowardsEnemy(targetEnemy);

        while (velocity > 0)
        {
            if (CheckWithinMinDist(targetEnemy)) yield break;

            velocity -= _settings.slideSpeed * _settings.slideDuration;
            _playerRB.position += _playerRB.transform.forward * velocity;
            yield return null;
        }
    }
}
