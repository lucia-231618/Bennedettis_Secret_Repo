using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Jugador")]
    public Transform playerTransform; // Arrastrar el jugador en el inspector


    // Estados generales del juego (Explorando, Dialogando, EnMenu, Cinematica, etc.)
    public enum GameState { Exploring, Dialogue, Menu, Cinematic } 
    public GameState CurrentState { get; private set; } = GameState.Exploring; //Cambia el estado global del juego

    // Diccionario de flags globales 
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();

    private void Awake()
    {
        // Singleton -- Evitamos duplicados de este script en la escena
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    // Cambiar estado global del juego
    public void SetState(GameState newState)
    {
        CurrentState = newState;

        // Aquí puedes bloquear controles o activar/desactivar sistemas según el estado
        // Por ejemplo: si CurrentState == Dialogue ? desactivar movimiento del jugador
    }

    // Verificar estado actual
    public bool IsState(GameState state)
    {
        return CurrentState == state;
    }


    // Activar o crear flag
    public void SetFlag(string flagName, bool value = true)
    {
        if (flags.ContainsKey(flagName))
            flags[flagName] = value;
        else
            flags.Add(flagName, value);
    }

    // Consultar flag
    public bool CheckState(string flagName)
    {
        if (flags.ContainsKey(flagName))
            return flags[flagName];
        return false; // Si no existe, lo considera falso
    }

}
