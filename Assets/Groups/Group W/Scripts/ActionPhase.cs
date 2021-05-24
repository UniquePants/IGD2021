using System;
using System.Collections;
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
        // TODO player hp should stay on top of player instead of rotating with him
        // ... maybe attach PlayerProperties to MinifigCharacter insteadof Player Minifig XXXX
        // face to the correct direction again!(change rotation)
        print("now rotating back");
        var minifigController = activePlayer.GetComponent<MinifigController>();
        Vector3 originalRotation = targetPlayer.transform.position;
        // when the animation is finished, it's the next players turn
        // gameObject.GetComponent<PhaseHandler>()
        minifigController.TurnTo(originalRotation, onComplete: () => { PhaseHandler.SetNextActivePlayer(); });
    }

    // player will fall down to ground
    void KillPlayer(PlayerProperties player)
    {
        // only the minfig should fall down, not the attached components like the hp bar
        GameObject minifigCharacter = player.GetComponent<MinifigController>().Minifig;

        // TODO do this SLOWLY, maybe create a dying animation
        // TODO prevent player from being pushed around
        var rotationVector = minifigCharacter.transform.rotation.eulerAngles;
        rotationVector.x = -90;
        minifigCharacter.transform.rotation = Quaternion.Euler(rotationVector);

        //var rotationVector = player.transform.rotation.eulerAngles;
        //rotationVector.x = -90;
        //player.transform.rotation = Quaternion.Euler(rotationVector);

        print($"player ({player.name}) is dead now");
    }

    void RemoveLeftHandWeapon()
    {
        if (leftHandWeapon != null)
        {
            print("removed left hand weapon");
            DestroyImmediate(leftHandWeapon, true);
        }
    }

    public void ChangeLeftHandWeapon(PhaseHandler.RowPosition rowPosition, WeaponDefinitions.WeaponType weaponType)
    {
        // to prevent any weapons from laying around after being thrown...does this make sense?
        // alternative would be to spawn a new weapon and let any old ones where they are
        // RemoveLeftHandWeapon();
        Transform characterFingerPosition = player.transform.Find("Minifig Character/jointScaleOffset_grp/Joint_grp/detachSpine/spine01/spine02/spine03/spine04/spine05/spine06/shoulder_L/armUp_L/arm_L/wristTwist_L/wrist_L/hand_L/finger01_L").transform;
        if (leftHandWeapon == null)
        {
            print("equipping new left hand weapon");
            // spawns a new weapon and sets it as leftHandWeapon
            leftHandWeapon = SpawnNewWeapon(rowPosition, weaponType);
            leftHandWeapon.transform.parent = characterFingerPosition;
        }
    }

    GameObject SpawnNewWeapon(PhaseHandler.RowPosition rowPosition, WeaponDefinitions.WeaponType weaponType)
    {
        // load a gameobject with the correct prefab
        Weapon[] matchingWeapons = WeaponDefinitions.GetWeapon(weaponType, rowPosition);
        GameObject newWeapon;

        if (matchingWeapons.Length > 0)
        {
            string assetPath = matchingWeapons[0].asset;
            GameObject prefab = Resources.Load<GameObject>("Prefabs/" + assetPath) as GameObject;
            newWeapon = Instantiate(prefab, leftHandPosition, player.transform.rotation);
            newWeapon.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            //newWeapon.layer = 8; // layer 8 should be "player", such that players won't collide with each other
            //prefab.layer = 8;
            return newWeapon;
        }
        else
        {
            // no matching weapon type was found
            throw new InvalidOperationException();
        }
    }

    // throws the equipped weapon from activePlayer to targetPlayer
    IEnumerator ThrowWeapon(PlayerProperties activePlayer, PlayerProperties targetPlayer, Action onComplete)
    {
        print("is back row; now throwing weapon");
        GameObject throwableWeapon = SpawnNewWeapon(activePlayer.CurrentRowPosition, activePlayer.weapon);

        // should get a rigidbody to be able to throw it, if not already available
        if(throwableWeapon.GetComponent<Rigidbody>() != null)
        {
            throwableWeapon.AddComponent<Rigidbody>();
        }
        
        // following code is adapted from https://gist.github.com/marcelschmidt1337/e46d166b639c06af3ba896fcb8412be4
        float throwAngle = 45.0f;
        float gravity = 9.8f;
        // TODO should land on the ground, not on the middle of the target character
        Vector3 target = new Vector3(targetPlayer.transform.position.x, targetPlayer.transform.position.y, targetPlayer.transform.position.z);
        print($"target: {target}, player position: {targetPlayer.transform.position}");
        float targetDistance = Vector3.Distance(throwableWeapon.transform.position, target);

        // Calculate the velocity needed to throw the object to the target at specified angle
        float weaponVelocity = targetDistance / (Mathf.Sin(2 * throwAngle * Mathf.Deg2Rad) / gravity);

        // Extract the X & Y componenent of the velocity
        float Vx = Mathf.Sqrt(weaponVelocity) * Mathf.Cos(throwAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(weaponVelocity) * Mathf.Sin(throwAngle * Mathf.Deg2Rad);

        // Calculate flight time.
        float flightDuration = targetDistance / Vx;
        print($"flight duration: {flightDuration}");

        // remove the equipped weapon and throw the new one;
        RemoveLeftHandWeapon();

        float elapsedTime = 0;
        while (elapsedTime < flightDuration)
        {
            throwableWeapon.transform.Translate(0, (Vy - (gravity * elapsedTime)) * Time.deltaTime, Vx * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if(elapsedTime >= flightDuration)
        {
            print("finished throwing weapon");
            onComplete.Invoke();
            DestroyImmediate(throwableWeapon, true);
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
        PlayerProperties targetPlayer = GetTargetPlayer(player.team, player.targetRow);

        // checks for restrictions before attacking
        if (CanPlayerAttack(player, targetPlayer))
        {
            // TODO check if player is front or back row to choose whether player should move or throw weapon
            // -> front should move and swing weapon, back should throw weapon
            if(player.CurrentRowPosition == PhaseHandler.RowPosition.Back)
            {
                Coroutine throwWeaponCoroutine = StartCoroutine(ThrowWeapon(player, targetPlayer, onComplete: () => {
                    DealDamage(player, targetPlayer);
                    PhaseHandler.SetNextActivePlayer();
                }));
            }

            else
            {
                MoveToAttackTarget(player, targetPlayer);
            }
        }
        else
        {
            print($"target ({targetPlayer.name}) can currently not be attacked. Switching to next player now.");
            // somehow this has to be done via callback, otherwise the next phase won't be triggered
            // PhaseHandler.SetNextActivePlayer();
            // StartCoroutine(SetNextActivePlayer(onComplete: () => { PhaseHandler.SetNextActivePlayer(); }));

            MinifigController.SpecialAnimation specialAnimation = player.currentHp <= 0 ? MinifigController.SpecialAnimation.LookingDown : MinifigController.SpecialAnimation.IdleImpatient;

            var minifigController = player.GetComponent<MinifigController>();
            minifigController.PlaySpecialAnimation(specialAnimation, onSpecialComplete: (x) => {
                PhaseHandler.SetNextActivePlayer();
            });
        }

    }

    // this was just a test, can be deleted
    IEnumerator SetNextActivePlayer(Action onComplete)
    {
        print("setting next active player from ActionPhase.cs");
        // PhaseHandler.SetNextActivePlayer();
        onComplete.Invoke();
        yield return null;
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
