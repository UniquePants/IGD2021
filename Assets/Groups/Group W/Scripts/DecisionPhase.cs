using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DecisionPhase : MonoBehaviour
{
    public PhaseHandler.Phase phase;
    public WeaponDefinitions.WeaponType selectedWeapon;
    public PhaseHandler.RowPosition selectedTargetRow;
    public Vector3 leftHandPosition;
    public GameObject leftHandWeapon;
    PlayerProperties player;
    bool isDecisionPhase;

    private void ChangeEquippedWeapon(WeaponDefinitions.WeaponType weapon)
    {
        if (phase == PhaseHandler.Phase.Decision)
        {
            print($"Changing Weapon to {selectedWeapon}");
            selectedWeapon = weapon;
        }

        else
        {
            print("Changing Weapon is currently not allowed");
        }
    }

    private void ChangeTargetRow(PhaseHandler.RowPosition targetRow)
    {
        if (phase == PhaseHandler.Phase.Decision)
        {
            print($"Changing TargetRow to {targetRow}");
            selectedTargetRow = targetRow;
        }
        else
        {
            print("Changing TargetRow is currently not allowed");
        }
    }

    // listen to input events to change the Weapon / targetRow accordingly (only if in Decision phase, of course)
    #region input handling
    private void OnWestPress()
    {
        ChangeTargetRow(PhaseHandler.RowPosition.Back);
    }

    private void OnSouthPress()
    {
        ChangeEquippedWeapon(WeaponDefinitions.WeaponType.Scissors);
    }

    private void OnEastPress()
    {
        ChangeEquippedWeapon(WeaponDefinitions.WeaponType.Lego);
    }

    private void OnNorthPress()
    {
        ChangeTargetRow(PhaseHandler.RowPosition.Front);
    }

    private void OnMoveDpad(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        input.Normalize();

        if (input.Equals(Vector2.up))
        {
            ChangeEquippedWeapon(WeaponDefinitions.WeaponType.Paper);
        }
    }
    #endregion

    // TODO call from ActionPhase instead, this is a duplicate right now
    void RemoveLeftHandWeapon()
    {
        if (leftHandWeapon != null)
        {
            DestroyImmediate(leftHandWeapon, true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // access specific PlayerProperties of the gameobject where this script is attached to
        player = gameObject.GetComponent<PlayerProperties>();
        leftHandPosition = player.leftHandPosition;
    }

    // Update is called once per frame
    void Update()
    {
        phase = PhaseHandler.phase;
        isDecisionPhase = phase == PhaseHandler.Phase.Decision;

        if (isDecisionPhase)
        {
            RemoveLeftHandWeapon();
        }
    }
}
