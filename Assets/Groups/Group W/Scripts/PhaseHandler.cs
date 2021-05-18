﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Class used to switch between battle phases
 */
public class PhaseHandler : MonoBehaviour
{
    public static Phase phase;
    public static int roundCount;
    public static float timeLeft;
    public float secondsUntilActionPhase = 5f;
    public float secondsPassed = 0f;
    public static List<PlayerProperties> players;

    public static int activePlayerIndex;
    public bool isActionPhaseFinished;
    public List<bool> arePlayerActionsOver;

    public enum Phase
    {
        Decision,
        Action
    }

    // TODO replace RowPosition / Team Enums with simple lists, allowing for more flexibility in the future
    public enum RowPosition
    {
        Front,
        Back
    }


    public enum Team
    {
        Left,
        Right
    }

    public static void SetNextActivePlayer()
    {
        activePlayerIndex += 1;
        print($"next active player: {activePlayerIndex}");
    }

    // Start is called before the first frame update
    void Start()
    {
        print($"calling from: {gameObject.name}");
        phase = Phase.Decision;
        roundCount = 1;

        // gather all players
        players = new List<PlayerProperties>();
        foreach (Transform child in transform)
        {
            players.Add(child.GetComponent<PlayerProperties>());
            arePlayerActionsOver.Add(false);
        }

        // set the first player active
        activePlayerIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO 
        // array mit phasen (interface phase)
        // index der aktiven phase
        // if(update aktive phase) increment index modulo

        if (phase == Phase.Decision)
        {
            secondsPassed = secondsPassed += Time.deltaTime;
            timeLeft = secondsUntilActionPhase - secondsPassed;

            if (secondsPassed >= secondsUntilActionPhase)
            {
                print("its action phase now");
                phase = Phase.Action;
            }
        }

        if (phase == Phase.Action)
        {
            secondsPassed = 0f;
            isActionPhaseFinished = activePlayerIndex >= players.Count;

            if (isActionPhaseFinished)
            {
                print("its decision phase now");
                phase = Phase.Decision;
                roundCount += 1;
                activePlayerIndex = 0;
                arePlayerActionsOver = Enumerable.Repeat(false, players.Count).ToList();
            }
            else
            {
                // TODO it's sufficient to call this one time per round
                // equip new weapon for each player
                foreach (PlayerProperties player in players)
                {
                    ActionPhase actionPhase = player.GetComponent<ActionPhase>();
                    actionPhase.ChangeLeftHandWeapon(player.rowPosition, player.weapon);
                }

                // each player attacks sequentially
                // end if the last active player is finished
                ActionPhase activePlayerActionPhase = players[activePlayerIndex].GetComponent<ActionPhase>();

                bool isPlayerActionOver = arePlayerActionsOver[activePlayerIndex];

                if (!isPlayerActionOver)
                {
                    activePlayerActionPhase.DoAction();
                    arePlayerActionsOver[activePlayerIndex] = true;
                }
            }
        }


    }
}
