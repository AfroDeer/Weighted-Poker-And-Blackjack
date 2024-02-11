//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Blackjack : CasinoGameState
{
    //References
    [SerializeField]
    public GameObject playingCardRef;
    [SerializeField]
    public Button hitButton;
    protected List<GameObject> playerCardProps = new List<GameObject>();
    protected List<GameObject> dealerCardProps = new List<GameObject>();

    //Data
    protected List<PlayingCard> playerHand = new List<PlayingCard>();
    protected List<PlayingCard> dealerHand = new List<PlayingCard>();
    protected int playerMinValue = 0;
    protected int playerMaxValue = 0;
    protected int dealerMinValue = 0;
    protected int dealerMaxValue = 0;

    //System
    protected override void Start()
    {
        base.Start();
        SetGameStep(EGameStep.Standby);
    }

    //Override game step logic.
    override protected void SetGameStep(EGameStep Step)
    {
        base.SetGameStep(Step);
        switch (Step)
        {
            case EGameStep.Standby:
                playerHand = new List<PlayingCard>();
                dealerHand = new List<PlayingCard>();
                playerCardProps = new List<GameObject>();
                dealerCardProps = new List<GameObject>();
                playerMinValue = 0;
                playerMaxValue = 0;
                dealerMinValue = 0;
                dealerMaxValue = 0;
                FillDeck();
                if (playCreditsText) playCreditsText.text = "INSERT CREDITS TO PLAY";
                if (playText) playText.text = "DEAL";
                if (playButton) playButton.interactable = true;
                if (hitButton) hitButton.interactable = false;
                break;
            case EGameStep.DealerTurn:
                if (playerCardProps != null)
                {
                    for (int i = 0; i < playerCardProps.Count; i++)
                    {
                        Destroy(playerCardProps[i].gameObject);
                    }
                }
                if (dealerCardProps != null)
                {
                    for (int i = 0; i < dealerCardProps.Count; i++)
                    {
                        Destroy(dealerCardProps[i].gameObject);
                    }
                }
                playerCardProps = new List<GameObject>();
                dealerCardProps = new List<GameObject>();
                if (playCreditsText) playCreditsText.text = "";
                if (resultAmountText) resultAmountText.text = "";
                if (resultTypeText) resultTypeText.text = "";
                if (playText) playText.text = "";
                if (playButton) playButton.interactable = false;
                if (hitButton) hitButton.interactable = false;
                StartCoroutine(DealInitialHands());
                break;
            case EGameStep.PlayerTurn:
                if (playCreditsText) playCreditsText.text = "INSERT CREDITS TO PLAY";
                if (playText) playText.text = "STAND";
                if (playButton) playButton.interactable = true;
                if (hitButton) hitButton.interactable = true;
                break;
            case EGameStep.Finished:
                StartCoroutine(FinishGame());
                break;
            default:
                break;
        }
    }

    //Send event when play button is clicked.
    public void PlayButtonClicked()
    {
        switch (gameStep)
        {
            case EGameStep.Standby:
            case EGameStep.Finished:
                if (CommitBet()) SetGameStep(EGameStep.DealerTurn);
                break;
            case EGameStep.PlayerTurn:
                if (playText) playText.text = "";
                if (playButton) playButton.interactable = false;
                SetGameStep(EGameStep.Finished);
                break;
            case EGameStep.DealerTurn:
            default:
                Debug.LogError("BUGSPLAT: The Deal/Draw button was clicked during a non-valid Game Step!");
                break;
        }
    }

    //Send event when hit button is clicked.
    public void HitButtonClicked()
    {
        BlackjackDrawCard(false);
        if (CheckBust(false)) SetGameStep(EGameStep.Finished);
    }

    //Draw two cards for player and dealer.
    IEnumerator DealInitialHands()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i % 2 == 0) BlackjackDrawCard(true);
            else BlackjackDrawCard(false);
            yield return new WaitForSeconds(0.4f / gameSpeed);
        }
        SetGameStep(EGameStep.PlayerTurn);
    }

    //Dealer draws until they have a decent hand then resolve game.
    IEnumerator FinishGame()
    {
        bool endConditionMet = false;
        PlayingCard tempCardData = new PlayingCard(dealerHand[0].cardData, true);
        dealerHand[0] = tempCardData;
        UpdateCardDisplay(dealerCardProps[0], tempCardData);
        yield return new WaitForSeconds(0.4f / gameSpeed);
        if (CheckBust(false))
        {
            if (resultTypeText) resultTypeText.text = "PLAYER BUST";
            if (resultAmountText) resultAmountText.text = "BETTER LUCK NEXT TIME!";
            endConditionMet = true;
        }
        else
        {
            while (dealerMaxValue < 18 || (dealerMinValue < 14 && dealerMaxValue < 14))
            {
                BlackjackDrawCard(true);
                yield return new WaitForSeconds(0.4f / gameSpeed);
            }
            if (CheckBust(true))
            {
                if (resultTypeText) resultTypeText.text = "DEALER BUST";
                uint payout = (currentBet * betModifier * 2);
                float displayPayout = (float)payout / 100.0f;
                GameInstance.instance.AddCredits(payout, false);
                if (resultAmountText) resultAmountText.text = "WON $" + $"{displayPayout:n2}";
                endConditionMet = true;
            }
        }
        if (!endConditionMet)
        {
            if (CheckHandAgainstDealer())
            {
                if (resultTypeText) resultTypeText.text = "PLAYER WON";
                uint payout = (currentBet * betModifier * 2);
                float displayPayout = (float)payout / 100.0f;
                GameInstance.instance.AddCredits(payout, false);
                if (resultAmountText) resultAmountText.text = "WON $" + $"{displayPayout:n2}";
            }
            else
            {
                if (resultTypeText) resultTypeText.text = "DEALER WON";
                if (resultAmountText) resultAmountText.text = "BETTER LUCK NEXT TIME!";
            }
            endConditionMet = true;
        }
        //print("DEALER: " + dealerMinValue + " - " + dealerMaxValue + " | PLAYER: " + playerMinValue + " - " + playerMaxValue);
        playerHand = new List<PlayingCard>();
        dealerHand = new List<PlayingCard>();
        playerMinValue = 0;
        playerMaxValue = 0;
        dealerMinValue = 0;
        dealerMaxValue = 0;
        FillDeck();
        if (playCreditsText) playCreditsText.text = "INSERT CREDITS TO PLAY";
        if (playText) playText.text = "DEAL";
        if (playButton) playButton.interactable = true;
        if (hitButton) hitButton.interactable = false;
    }

    //Draw card data and populate scene with a prop.
    protected void BlackjackDrawCard(bool IsDealer)
    {
        if (DrawCard(IsDealer))
        {
            PlayingCard tempCardData = new PlayingCard(drawnCard, false);
            GameObject tempCardProp = Instantiate(playingCardRef, new Vector3(0, 0, 0), Quaternion.identity);
            tempCardProp.transform.SetParent(GameObject.Find("Panel").transform, false);
            if (IsDealer)
            {
                if (dealerHand.Count >= 1) tempCardData.isRevealed = true;
                tempCardProp.transform.localPosition = new Vector3(400 - (dealerHand.Count * 125), 500, 0);
                dealerHand.Add(tempCardData);
                dealerCardProps.Add(tempCardProp);
                switch (tempCardData.cardData.cardValue)
                {
                    case ECardValue.Ace:
                        dealerMinValue++;
                        dealerMaxValue += 11;
                        break;
                    case ECardValue.Ten:
                    case ECardValue.Jack:
                    case ECardValue.Queen:
                    case ECardValue.King:
                        dealerMinValue += 10;
                        dealerMaxValue += 10;
                        break;
                    default:
                        int cardVal = (int)tempCardData.cardData.cardValue + 1;
                        dealerMinValue += cardVal;
                        dealerMaxValue += cardVal;
                        break;
                }
            }
            else
            {
                tempCardData.isRevealed = true;
                tempCardProp.transform.localPosition = new Vector3(-400 + (playerHand.Count * 125), 50, 0);
                playerHand.Add(tempCardData);
                playerCardProps.Add(tempCardProp);
                switch (tempCardData.cardData.cardValue)
                {
                    case ECardValue.Ace:
                        playerMinValue++;
                        playerMaxValue += 11;
                        break;
                    case ECardValue.Ten:
                    case ECardValue.Jack:
                    case ECardValue.Queen:
                    case ECardValue.King:
                        playerMinValue += 10;
                        playerMaxValue += 10;
                        break;
                    default:
                        int cardVal = (int)tempCardData.cardData.cardValue + 1;
                        playerMinValue += cardVal;
                        playerMaxValue += cardVal;
                        break;
                }
            }
            UpdateCardDisplay(tempCardProp, tempCardData);
        }
        else
        {
            Debug.LogError("BUGSPLAT: Deck was empty and could not perform a draw. Returning to main menu to avoid a soft lock!");
            LoadLevel("MainMenu");
        }
    }

    //Check if the hand has bust.
    protected bool CheckBust(bool IsDealer)
    {
        if (IsDealer)
        {
            if (dealerMinValue > 21 && dealerMaxValue > 21) return true;
        }
        else
        {
            if (playerMinValue > 21 && playerMaxValue > 21) return true;
        }
        return false;
    }

    //Check if player won by card value.
    protected bool CheckHandAgainstDealer()
    {
        int playerValue = playerMinValue;
        if (playerMaxValue <= 21) playerValue = playerMaxValue;
        int dealerValue = dealerMinValue;
        if (dealerMaxValue <= 21) playerValue = dealerMaxValue;
        return playerValue < dealerValue;
    }
}