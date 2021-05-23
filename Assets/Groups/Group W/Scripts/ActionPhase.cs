using System;
using System.Collections.Generic;
using UnityEngine;

public class ActionPhase : MonoBehaviour
{
    public PhaseHandler.Phase phase;
    public TextAsset weaponTypesJsonFile;
    public WeaponTypes weaponTypes;

    private float moveStopDistance = 1f;
    public static int activePlayerIndex = 0;
    PlayerProperties player;
    public static List<PlayerProperties> players;
    bool isActionPhase;
    public GameObject leftHandWeapon;
    public Vector3 leftHandPosition;

    // calculates the damage dealt by currentPlayer to targetPlayer
    // takes the base damage and its multiplier (derived from the weapon types) into account
    float CalculateDamage(PlayerProperties currentPlayer, PlayerProperties targetPlayer)
    {
        Weapon[] matchingWeapons = WeaponDefinitions.GetWeapon(currentPlayer.weapon, currentPlayer.rowPosition);
        if (matchingWeapons.Length > 0)
        {
            int baseDamage = Int16.Parse(matchingWeapons[0].power);
            float multiplier = GetMultiplier(currentPlayer.weapon, targetPlayer.weapon);
            return baseDamage * multiplier;
        }

        else
        {
            return 0f;
        }
    }

    // returns a damage multipler based on the equipped weapon type and the target weapon type strength/weakness
    float GetMultiplier(WeaponDefinitions.WeaponType equippedWeaponType, WeaponDefinitions.WeaponType targetWeaponType)
    {
        WeaponType equippedWeaponTypeInfo = Array.FindAll<WeaponType>(weaponTypes.weaponTypes, weaponType => weaponType.type == equippedWeaponType.ToString())[0];
        if (targetWeaponType.ToString() == equippedWeaponTypeInfo.strength)
        {
            return 2f;
        }

        else if (targetWeaponType.ToString() == equippedWeaponTypeInfo.weakness)
        {
            return 0.5f;
        }

        else
        {
            return 1f;
        }
    }

    // searches for the player of the other team by chosing the other team and the target row
    PlayerProperties GetTargetPlayer(PhaseHandler.Team ownTeam, PhaseHandler.RowPosition targetRow)
    {

        // opponent team is the team that is not the own team
        PhaseHandler.Team opponentTeam = ownTeam == PhaseHandler.Team.Left ? PhaseHandler.Team.Right : PhaseHandler.Team.Left;
        List<PlayerProperties> matchingPlayers = players.FindAll(player => player.team == opponentTeam
                                                && player.rowPosition == targetRow);

        if (matchingPlayers.Count > 0)
        {
            return matchingPlayers[0];
        }
        
        else
        {
            // no player found
            throw new InvalidOperationException();
        }
    }

    // moves to the target and attacks it
    void MoveToAttackTarget(PlayerProperties activePlayer, PlayerProperties targetPlayer)
    {
        var minifigController = activePlayer.GetComponent<MinifigController>();
        Vector3 targetPosition = targetPlayer.transform.position;

        // stop *in front of* target character, not on top
        // if activePlayer.z positive, +=; else -=
        if (Math.Sign(activePlayer.transform.position.z) == -1)
        {
            targetPosition.z -= moveStopDistance;
        }
        else
        {
            targetPosition.z += moveStopDistance;
        }
        
        
        minifigController.MoveTo(targetPosition, onComplete: () => { MeleeAttack(activePlayer, targetPlayer); });
    }

    bool CanPlayerAttack(PlayerProperties activePlayer, PlayerProperties targetPlayer)
    {
        bool bothOnFrontRow = activePlayer.CurrentRowPosition == PhaseHandler.RowPosition.Front && targetPlayer.CurrentRowPosition == PhaseHandler.RowPosition.Front;
        bool activePlayerIsBackRow = activePlayer.CurrentRowPosition == PhaseHandler.RowPosition.Back;
        bool targetIsAlive = targetPlayer.currentHp > 0;
        bool isPlayerAlive = activePlayer.currentHp > 0;

        if (targetIsAlive && isPlayerAlive)
        {
            return bothOnFrontRow || activePlayerIsBackRow;
        }

        else
        {
            return false;
        }
    }

    // plays an attack animation, deals damage and returns to the start position
    void MeleeAttack(PlayerProperties activePlayer, PlayerProperties targetPlayer)
    {
        var minifigController = activePlayer.GetComponent<MinifigController>();
        minifigController.PlaySpecialAnimation(MinifigController.SpecialAnimation.HatSwap, onSpecialComplete: (x) => {
            DealDamage(activePlayer, targetPlayer);
            print("finished attack");
            ReturnToStartPosition(activePlayer, targetPlayer);
        });
    }

    void DealDamage(PlayerProperties activePlayer, PlayerProperties targetPlayer)
    {
        print("now dealing damage");
        // calculate damage
        float damage = CalculateDamage(activePlayer, targetPlayer);
        // lower hp of target, but prevent hp from falling below 0
        float newHp = targetPlayer.currentHp - damage;
        targetPlayer.currentHp = newHp > 0 ? newHp : 0;
        print($"damage to target ({targetPlayer.name}): {damage}. New HP: {targetPlayer.currentHp}");

        if (targetPlayer.currentHp <= 0)
        {
            KillPlayer(targetPlayer);
        }
    }

    void ReturnToStartPosition(PlayerProperties activePlayer, PlayerProperties targetPlayer)
    {
        var minifigController = activePlayer.GetComponent<MinifigController>();
        minifigController.MoveTo(activePlayer.startPosition, onComplete: () => { RotateBack(activePlayer, targetPlayer); });
    }

    void RotateBack(PlayerProperties activePlayer, PlayerProperties targetPlayer)
    {
        // face to the correct direction again!(change rotation)
        print("now rotating back");
        var minifigController = activePlayer.GetComponent<MinifigController>();
        Vector3 originalRotation = targetPlayer.transform.position;
        // when the animation is finished, it's the next players turn
        // gameObject.GetComponent<PhaseHandler>()
        minifigController.TurnTo(originalRotation, onComplete: () => { PhaseHandler.SetNextActivePlayer(); });
    }

    // player will fall down to earth
    void KillPlayer(PlayerProperties player)
    {
        var minifigController = player.GetComponent<MinifigController>();
        //minifigController.PlaySpecialAnimation(MinifigController.SpecialAnimation.Crawl);
        //Invoke("minifigController.StopSpecialAnimation", 3.0f);
        // wait x seconds, then .StopSpecialAnimation()
        //minifigController.StopSpecialAnimation();

        // TODO do this SLOWLY 
        // TODO player hp should stay on top of player instead of rotating with him
        // TODO prevent player from being pushed around
        var rotationVector = player.transform.rotation.eulerAngles;
        rotationVector.x = 90;
        player.transform.rotation = Quaternion.Euler(rotationVector);

        //float RotationSpeed = 2.0f;
        //player.transform.Rotate(Vector3.up * (RotationSpeed * Time.deltaTime));

        print($"player ({player.name}) is dead now");
    }

    public void ChangeLeftHandWeapon(PhaseHandler.RowPosition rowPosition, WeaponDefinitions.WeaponType weaponType)
    {
        // load a gameobject with the correct prefab

        Weapon[] matchingWeapons = WeaponDefinitions.GetWeapon(weaponType, rowPosition);
        if (matchingWeapons.Length > 0 && leftHandWeapon == null)
        {
            string assetPath = matchingWeapons[0].asset;
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + assetPath) as GameObject;
            leftHandWeapon = Instantiate(prefab, leftHandPosition, player.transform.rotation);

            // set the weapon as a child of left hand
            leftHandWeapon.transform.parent = player.transform.Find("Minifig Character/jointScaleOffset_grp/Joint_grp/detachSpine/spine01/spine02/spine03/spine04/spine05/spine06/shoulder_L/armUp_L/arm_L/wristTwist_L/wrist_L/hand_L/finger01_L").transform;
            leftHandWeapon.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        weaponTypes = JsonUtility.FromJson<WeaponTypes>(weaponTypesJsonFile.text);
        player = gameObject.GetComponent<PlayerProperties>();
    }

    public void DoAction()
    {
        // ChangeLeftHandWeapon(player.rowPosition, player.weapon);
        PlayerProperties targetPlayer = GetTargetPlayer(player.team, player.targetRow);

        // if the preceding player is finished, its the next ones turn
        // print($"current active player is {player.name}");


        // checks for restrictions before attacking
        if (CanPlayerAttack(player, targetPlayer))
        {
            // TODO check if player is front or back row to choose whether player should move or throw weapon
            // -> front should move and swing weapon, back should throw weapon
            MoveToAttackTarget(player, targetPlayer);
        }
        else
        {
            // TODO switch to next player
            print($"target ({targetPlayer.name}) can currently not be attacked. Switching to next player now.");
            PhaseHandler.SetNextActivePlayer();
        }

    }

    // Update is called once per frame
    void Update()
    {
        leftHandPosition = player.leftHandPosition;
        players = PhaseHandler.players;
    }
}


[System.Serializable]
public class WeaponTypes
{
    public WeaponType[] weaponTypes;
}


[System.Serializable]
public class WeaponType
{
    public string type;
    public string weakness;
    public string strength;
}
