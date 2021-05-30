using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TournamentGame : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameField[] tournamentFields;
    [SerializeField] GameFieldManager gameFieldManager;
    public void SetUpTournament()
    {
        int i = 0;
        foreach (GameField field in tournamentFields)
        {
            GameFieldManager.gameFields.Add(field);
            field.InitialiseMap(i++);
        }
    }
}
