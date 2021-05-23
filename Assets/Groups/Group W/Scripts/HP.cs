using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HP : MonoBehaviour
{
    TextMesh hpTextMesh;
    public string playerName;

    // Start is called before the first frame update
    void Start()
    {
        hpTextMesh = gameObject.GetComponent<TextMesh>();
    }

    // Update is called once per frame
    void Update()
    {
        List<PlayerProperties> players = PhaseHandler.players;
        List<PlayerProperties> matchingPlayers = players.FindAll(player => player.name == playerName);
        if(matchingPlayers.Count == 1)
        {
            PlayerProperties player = matchingPlayers[0];
            hpTextMesh.text = $"{player.currentHp}/{player.maxHp}\n";
            // only allow z position to change, others should always stay the same
            // transform.position = new Vector3(player.startPosition.x, player.startPosition.y, player.transform.position.z);
        }

        else
        {
            print($"no matching players found for player name {playerName}. can not show hp.");
        }

       
    }
}
