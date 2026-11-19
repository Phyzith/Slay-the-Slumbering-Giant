using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataManager : MonoBehaviour
{
    public GameObject player;
    [SerializeField] private PlayerController playerController;

    void Start()
    {
        Debug.Log("The player joined the game!");
        LoadData();
    }


    void OnApplicationQuit()
    {
        Debug.Log("The player is leaving the game!");
        SaveData();
    }

    void SaveData() 
    {
        Data data = new Data();
        data.health = playerController.health;
        data.playerPosition = player.transform.position;
        data.playerRotation = player.transform.rotation;
        data.movement = playerController.playerMovement;

        string json = JsonUtility.ToJson(data);
        File.WriteAllText(Application.dataPath + "/playerData.json", json);
    }

    void LoadData() 
    {
        string json = File.ReadAllText(Application.dataPath + "/playerData.json");
        Data data = JsonUtility.FromJson<Data>(json);

        playerController.health = data.health;
        player.transform.position = data.playerPosition;
        player.transform.rotation = data.playerRotation;
        playerController.playerMovement = data.movement;
    }
}
