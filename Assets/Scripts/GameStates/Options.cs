//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Options : GameState
{
    //References
    [SerializeField]
    public TMP_Dropdown cardDropdown;
    [SerializeField]
    public TMP_InputField cardPlayerWeightField;
    [SerializeField]
    public TMP_InputField cardDealerWeightField;
    [SerializeField]
    public TextMeshProUGUI selectedCardText;

    //Data
    protected int selectedCardIndex = 0;

    //System
    public override void Awake()
    {
        base.Awake();
        List<string> dropdownOptions = new List<string>();
        for (int i = 0; i < GameInstance.instance.masterDeck.Count; i++)
        {
            string cardVal = System.Enum.GetName(typeof(ECardValue), GameInstance.instance.masterDeck[i].cardValue);
            string cardSuit = System.Enum.GetName(typeof(ESuit), GameInstance.instance.masterDeck[i].suit);
            string tempOption = cardVal + " Of " + cardSuit;
            dropdownOptions.Add(tempOption);
        }
        cardDropdown.ClearOptions();
        cardDropdown.AddOptions(dropdownOptions);
    }
    protected void Start()
    {
        selectedCardIndex = 0;
        UpdateSelectedCardText();
    }

    //Get the card's weighted % chance to be drawn from a deck.
    double GetChanceToDraw(Card CardRef, bool IsDealer)
    {
        uint totalWeight = 0;
        for (int i = 0; i < GameInstance.instance.masterDeck.Count; i++)
        {
            if (IsDealer) totalWeight += GameInstance.instance.masterDeck[i].dealerWeight;
            else totalWeight += GameInstance.instance.masterDeck[i].playerWeight;
        }
        double chanceToDraw = 0;
        if (IsDealer) chanceToDraw = (double)CardRef.dealerWeight / (double)totalWeight;
        else chanceToDraw = (double)CardRef.playerWeight / (double)totalWeight;
        return chanceToDraw * 100;
    }

    //Update UI when changing card or weight.
    protected void UpdateSelectedCardText()
    {
        Card masterCardRef = GameInstance.instance.masterDeck[selectedCardIndex];
        string cardVal = System.Enum.GetName(typeof(ECardValue), GameInstance.instance.masterDeck[selectedCardIndex].cardValue);
        string cardSuit = System.Enum.GetName(typeof(ESuit), GameInstance.instance.masterDeck[selectedCardIndex].suit);
        string cardText = "[SELECTED CARD]\n" + cardVal + " Of " + cardSuit + "(" + selectedCardIndex + ")\n\n";
        cardText += "[PLAYER WEIGHT]\n" + masterCardRef.playerWeight + " | " + $"{GetChanceToDraw(masterCardRef, false):n2}" +"%\n\n";
        cardText += "[DEALER WEIGHT]\n" + masterCardRef.dealerWeight + " | " + $"{GetChanceToDraw(masterCardRef, true):n2}" + "%";
        selectedCardText.text = cardText;
        cardPlayerWeightField.text = masterCardRef.playerWeight.ToString();
        cardDealerWeightField.text = masterCardRef.dealerWeight.ToString();
    }

    //Bind event for selecting a card via UI.
    public void SelectCard(int Index)
    {
        if (Index < 0 || Index > GameInstance.instance.masterDeck.Count) return;
        selectedCardIndex = Index;
        UpdateSelectedCardText();
    }

    //Bind event for adjusting the player weight of a card via UI.
    public void AdjustPlayerWeight(string Weight)
    {
        int realWeight = 1;
        int.TryParse(Weight, out realWeight);
        ModifyCardDrawWeight(GameInstance.instance.masterDeck[selectedCardIndex], false, (uint)realWeight);
        UpdateSelectedCardText();
    }

    //Bind event for adjusting the dealer weight of a card via UI.
    public void AdjustDealerWeight(string Weight)
    {
        int realWeight = 1;
        int.TryParse(Weight, out realWeight);
        ModifyCardDrawWeight(GameInstance.instance.masterDeck[selectedCardIndex], true, (uint)realWeight);
        UpdateSelectedCardText();
    }
}