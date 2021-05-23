using System;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.UI;

public class PlayerProperties : MonoBehaviour
{
    [Header("Health")]
    public int maxHp;
    public float currentHp;

    [Header("Properties")]
    public WeaponDefinitions.WeaponType weapon;
    public PhaseHandler.RowPosition rowPosition;
    public PhaseHandler.RowPosition targetRow;
    public PhaseHandler.Team team;
    public bool isActive = false;
    public Vector3 leftHandPosition;
    public Vector3 startPosition;

    public Image healthStatus;

    [Header("External")]
    public static PhaseHandler.Phase phase;
    DecisionPhase decisionPhase;


    // if the row position changes, change the max hp accordingly
    public PhaseHandler.RowPosition CurrentRowPosition
    {
        get => rowPosition;

        set
        {
            rowPosition = value;
            SetMaxHp();
        }
    }

    public void SetMaxHp()
    {
        maxHp = rowPosition == PhaseHandler.RowPosition.Front ? 100 : 50;
    }

    void EquipRandomWeapon()
    {
        Array values = Enum.GetValues(typeof(WeaponDefinitions.WeaponType));
        System.Random random = new System.Random();
        WeaponDefinitions.WeaponType randomWeapon = (WeaponDefinitions.WeaponType)values.GetValue(random.Next(values.Length));
        weapon = randomWeapon;
        print($"equipped random weapon: {randomWeapon}");
    }

    void SelectRandomTargetRow()
    {
        Array values = Enum.GetValues(typeof(PhaseHandler.RowPosition));
        System.Random random = new System.Random();
        PhaseHandler.RowPosition randomTargetRow = (PhaseHandler.RowPosition)values.GetValue(random.Next(values.Length));
        targetRow = randomTargetRow;
    }


    // Start is called before the first frame update
    void Start()
    {
        // initialize properties
        SetMaxHp();
        EquipRandomWeapon();
        SelectRandomTargetRow();
        startPosition = transform.position;
        decisionPhase = gameObject.GetComponent<DecisionPhase>();
        healthStatus = transform.GetChild(1).GetChild(0).GetChild(1).GetChild(1).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        weapon = decisionPhase.selectedWeapon;
        targetRow = decisionPhase.selectedTargetRow;
        leftHandPosition = transform.Find("Minifig Character/jointScaleOffset_grp/Joint_grp/detachSpine/spine01/spine02/spine03/spine04/spine05/spine06/shoulder_L/armUp_L/arm_L/wristTwist_L/wrist_L/hand_L/finger01_L").GetComponent<Transform>().position;
        healthStatus.fillAmount = (currentHp / maxHp);
    }

    //Detect when there is a collision
    void OnCollisionStay(Collision collide)
    {
        //Output the name of the GameObject you collide with
        Debug.Log("I hit the GameObject : " + collide.gameObject.name);
    }
}
