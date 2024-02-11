//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    //Data
    protected List<Card> physicalDeck = new List<Card>(); //Serves as a literal representation of the cards that are remaining in the deck.
    protected List<Card> virtualDeck = new List<Card>(); //A virtualization of all potential outcomes from drawing post weight adjustments for the player/dealer.
    protected Card drawnCard = new Card(); //A cached copy of the last drawn card.

    //System
    public virtual void Awake()
    {
        //Check to ensure game has been launched from the "Startup" scene for initialization purposes. Stop standalone process or editor if game instance is not found.
        if (!GameInstance.instance)
        {
            Debug.LogError("BUGSPLAT: Game Instance was not found! Please launch the game from 'Startup' scene.");
            Application.Quit();
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }

    //Interface for UI events to trigger scene changes.
    public void LoadLevel(string LevelName)
    {
        if (LevelName.Length == 0) return;
        GameInstance.instance.LoadLevel(LevelName);
    }

    //Virtualize outcomes from drawing by using weight parameters.
    //Create proxies based off of physical deck to represent adjusted chance to draw.
    protected bool VirtualizeDeck(bool IsDealer)
    {
        virtualDeck.Clear();
        for (int i = 0; i < physicalDeck.Count; i++)
        {
            if (IsDealer)
            {
                for (int n = 0; n < physicalDeck[i].dealerWeight; n++)
                {
                    virtualDeck.Add(physicalDeck[i]);
                }
            }
            else
            {
                for (int n = 0; n < physicalDeck[i].playerWeight; n++)
                {
                    virtualDeck.Add(physicalDeck[i]);
                }
            }
        }
        return true;
    }

    //Draw from the appropriate virtualized deck using weighted RNG and cached it for future use.
    protected bool DrawCard(bool IsDealer)
    {
        if (!VirtualizeDeck(IsDealer)) return false;
        if (virtualDeck.Count == 0) return false;
        drawnCard = virtualDeck[Random.Range(0, virtualDeck.Count - 1)];
        physicalDeck.Remove(drawnCard);
        return true;
    }

    //Populate physical deck with a clone of the master deck.
    protected bool FillDeck()
    {
        physicalDeck.Clear();
        for (int i = 0; i < GameInstance.instance.masterDeck.Count; i++)
        {
            physicalDeck.Add(GameInstance.instance.masterDeck[i]);
        }
        return true;
    }

    //Modify master deck draw weights.
    protected bool ModifyCardDrawWeight(Card Selection, bool IsDealer, uint NewWeight)
    {
        return GameInstance.instance.ModifyCardDrawWeight(Selection, IsDealer, NewWeight);
    }
}