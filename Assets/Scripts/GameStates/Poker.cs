//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class Poker : CasinoGameState
{
    //References
    [SerializeField]
    public GameObject[] riverProps = new GameObject[5];
    [SerializeField]
    public TextMeshProUGUI[] riverText = new TextMeshProUGUI[5];
    [SerializeField]
    public TextMeshProUGUI currentBetTable;
    [SerializeField]
    public TextMeshProUGUI nextBetTable;

    //Data
    protected PlayingCard[] river = new PlayingCard[5];
    protected Card[] sortedRiver = new Card[5];
    protected bool[] heldCards = new bool[5];

    //Constants
    protected const uint payoutMod_RoyalFlush = 800;
    protected const uint payoutMod_StraightFlush = 50;
    protected const uint payoutMod_FourKind = 25;
    protected const uint payoutMod_FullHouse = 9;
    protected const uint payoutMod_Flush = 6;
    protected const uint payoutMod_Straight = 4;
    protected const uint payoutMod_ThreeKind = 3;
    protected const uint payoutMod_TwoPair = 2;
    protected const uint payoutMod_JacksOrBetter = 1;

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
                FillDeck();
                river = new PlayingCard[5];
                heldCards = new bool[5];
                UpdateRiverDisplay();
                UpdateBetTables();
                if (playCreditsText) playCreditsText.text = "INSERT CREDITS TO PLAY";
                if (playText) playText.text = "DEAL";
                if (playButton) playButton.interactable = true;
                break;
            case EGameStep.DealerTurn:
                if (playCreditsText) playCreditsText.text = "";
                if (resultAmountText) resultAmountText.text = "";
                if (playText) playText.text = "";
                if (playButton) playButton.interactable = false;
                for (int i = 0; i < riverText.Length; i++)
                {
                    riverText[i].text = "";
                }
                UpdateRiverDisplay();
                StartCoroutine(DealerFillRiver());
                break;
            case EGameStep.PlayerTurn:
                CheckPayout();
                if (playCreditsText) playCreditsText.text = "";
                if (playText) playText.text = "DRAW";
                if (playButton) playButton.interactable = true;
                break;
            case EGameStep.Finished:
                uint payout = CheckPayout();
                if (payout > 0)
                {
                    float displayPayout = (float)payout / 100.0f;
                    if (resultAmountText) resultAmountText.text = "WON $" + $"{displayPayout:n2}";
                    GameInstance.instance.AddCredits(payout, false);
                    UpdateCreditDisplay();
                }
                else if (resultAmountText) resultAmountText.text = "BETTER LUCK NEXT TIME!";
                FillDeck();
                river = new PlayingCard[5];
                heldCards = new bool[5];
                if (playCreditsText) playCreditsText.text = "INSERT CREDITS TO PLAY";
                if (playText) playText.text = "DEAL";
                if (playButton) playButton.interactable = true;
                break;
            default:
                break;
        }
    }

    //Override AdjustBet for visual purposes.
    protected override void UpdateBetDisplay()
    {
        base.UpdateBetDisplay();
        UpdateBetTables();
    }

    //Check payout conditions.
    uint CheckPayout()
    {
        //Cache river and sort.
        sortedRiver = new Card[5];
        for (int i = 0; i < river.Length; i++)
        {
            sortedRiver[i] = river[i].cardData;
        }
        System.Array.Sort(sortedRiver, (x, y) => { return x.cardValue.CompareTo(y.cardValue); });

        bool foundFlush = CheckFlush();
        bool foundStraight = CheckStraight();
        if (foundFlush && foundStraight)
        {
            if (CheckRoyalFlush())
            {
                if (resultTypeText) resultTypeText.text = "ROYAL FLUSH!!!";
                return (currentBet * betModifier) * payoutMod_RoyalFlush; //Royal Flush
            }
            else
            {
                if (resultTypeText) resultTypeText.text = "STRAIGHT FLUSH!";
                return (currentBet * betModifier) * payoutMod_StraightFlush; //Straight Flush
            }
        }

        uint[] cardCount = GetCardCount();
        uint maxKinds = 0;
        uint maxPairs = 0;
        for (int i = 0; i < cardCount.Length; i++)
        {
            if (cardCount[i] > maxKinds) maxKinds = cardCount[i];
            if (cardCount[i] >= 2) maxPairs++;
        }

        if (maxKinds >= 4)
        {
            if (resultTypeText) resultTypeText.text = "FOUR OF A KIND";
            return (currentBet * betModifier) * payoutMod_FourKind; //Four Of A Kind
        }

        if (maxKinds >= 3 && maxPairs >= 2)
        {
            if (resultTypeText) resultTypeText.text = "FULL HOUSE";
            return (currentBet * betModifier) * payoutMod_FourKind; //Full House
        }

        if (foundFlush)
        {
            if (resultTypeText) resultTypeText.text = "FLUSH";
            return (currentBet * betModifier) * payoutMod_Flush; //Flush
        }

        if (foundStraight)
        {
            if (resultTypeText) resultTypeText.text = "STRAIGHT";
            return (currentBet * betModifier) * payoutMod_Straight; //Straight
        }

        if (maxKinds >= 3)
        {
            if (resultTypeText) resultTypeText.text = "THREE OF A KIND";
            return (currentBet * betModifier) * payoutMod_ThreeKind; //Three Of A Kind
        }

        if (maxKinds >= 2)
        {
            if (maxPairs >= 2)
            {
                if (resultTypeText) resultTypeText.text = "TWO PAIRS";
                return (currentBet * betModifier) * payoutMod_TwoPair; //Two Pairs
            }

            bool jacksOrBetter = false;
            if (cardCount[0] >= 2) jacksOrBetter = true;
            for (int i = 10; i < cardCount.Length; i++)
            {
                if (cardCount[i] >= 2)
                {
                    jacksOrBetter = true;
                    break;
                }
            }
            if (jacksOrBetter)
            {
                if (resultTypeText) resultTypeText.text = "JACKS OR BETTER";
                return (currentBet * betModifier) * payoutMod_JacksOrBetter; //Jacks Or Better
            }
        }
        if (resultTypeText) resultTypeText.text = "";
        return 0; //No winnings.
    }
    bool CheckRoyalFlush()
    {
        for (int i = 0; i < sortedRiver.Length; i++)
        {
            if (sortedRiver[i].cardValue < ECardValue.Ten && sortedRiver[i].cardValue != ECardValue.Ace) return false;
        }
        return true;
    }
    bool CheckFlush()
    {
        ESuit suitCheck = river[0].cardData.suit;
        for (int i = 0; i < sortedRiver.Length; i++)
        {
            if (sortedRiver[i].suit != suitCheck) return false;
        }
        return true;
    }
    bool CheckStraight()
    {
        ECardValue lastCardValue = sortedRiver[0].cardValue;
        bool hasAceAndKing = (sortedRiver[0].cardValue == ECardValue.Ace && sortedRiver[sortedRiver.Length - 1].cardValue == ECardValue.King); //Check if there is an Ace and King to determine whether to wrap around array to check for a straight.
        uint inARow = 1;
        for (int i = 1; i < sortedRiver.Length; i++)
        {
            if (sortedRiver[i].cardValue == (lastCardValue + 1))
            {
                lastCardValue = sortedRiver[i].cardValue;
                inARow++;
                if (inARow >= 5) return true; //Found straight in a single pass.
            }
        }
        if (hasAceAndKing)
        {
            inARow++;
            lastCardValue = sortedRiver[sortedRiver.Length - 1].cardValue;
            for (int i = sortedRiver.Length - 1; i > 0; i--)
            {
                if (sortedRiver[i].cardValue == (lastCardValue - 1))
                {
                    lastCardValue = sortedRiver[i].cardValue;
                    inARow++;
                    if (inARow >= 5) return true; //Found straight in the second pass.
                }
            }
        }
        print(inARow);
        return false;
    }
    uint[] GetCardCount()
    {
        uint[] cardCount = new uint[13];
        for (int i = 0; i < sortedRiver.Length; i++)
        {
            cardCount[(int)sortedRiver[i].cardValue]++;
        }
        return cardCount;
    }

    //Dealer fills river with cards.
    IEnumerator DealerFillRiver()
    {
        for (int i = 0; i < river.Length; i++)
        {
            if (DrawCard(true))
            {
                river[i].cardData = drawnCard;
                river[i].isRevealed = true;
                UpdateCardDisplay(riverProps[i], river[i]);
                yield return new WaitForSeconds(0.25f / gameSpeed);
            }
            else
            {
                Debug.LogError("BUGSPLAT: Deck was empty and could not perform a draw. Returning to main menu to avoid a soft lock!");
                LoadLevel("MainMenu");
            }
        }
        SetGameStep(EGameStep.PlayerTurn);
    }

    //Player mulls non-held cards.
    IEnumerator PlayerMullRiver()
    {
        for (int i = 0; i < river.Length; i++)
        {
            if (heldCards.Length < river.Length) break;
            if (heldCards[i]) continue;
            if (DrawCard(false))
            {
                river[i].cardData = drawnCard;
                river[i].isRevealed = true;
                UpdateCardDisplay(riverProps[i], river[i]);
                yield return new WaitForSeconds(0.25f / gameSpeed);
            }
            else
            {
                Debug.LogError("BUGSPLAT: Deck was empty and could not perform a draw. Returning to main menu to avoid a soft lock!");
                LoadLevel("MainMenu");
            }
        }
        SetGameStep(EGameStep.Finished);
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
                StartCoroutine(PlayerMullRiver());
                break;
            case EGameStep.DealerTurn:
            default:
                Debug.LogError("BUGSPLAT: The Deal/Draw button was clicked during a non-valid Game Step!");
                break;
        }
    }

    //Send event when a card is held.
    public void HoldCardToggle(int cardIndex)
    {
        if (gameStep != EGameStep.PlayerTurn) return;
        if (heldCards[cardIndex])
        {
            heldCards[cardIndex] = false;
            if (riverText.Length >= cardIndex) riverText[cardIndex].text = "";
        }
        else
        {
            heldCards[cardIndex] = true;
            if (riverText.Length >= cardIndex) riverText[cardIndex].text = "HELD";
        }
    }

    //Update all card visuals for the river.
    protected void UpdateRiverDisplay()
    {
        for (int i = 0; i < river.Length; i++)
        {
            if (i >= riverProps.Length) break;
            UpdateCardDisplay(riverProps[i], river[i]);
        }
    }

    //Update bet tables to reflect current reward potential.
    protected void UpdateBetTables()
    {
        for (int i = 0; i < 2; i++)
        {
            float baseReward = (currentBet * betModifier);
            if (i > 0) baseReward = ((currentBet + 1) * betModifier);
            baseReward = baseReward / 100;
            string newTableInfo = $"{(baseReward * (float)payoutMod_RoyalFlush):n2}" + "\n"; 
            newTableInfo += $"{(baseReward * (float)payoutMod_StraightFlush):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_FourKind):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_FullHouse):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_Flush):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_Straight):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_ThreeKind):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_TwoPair):n2}" + "\n";
            newTableInfo += $"{(baseReward * (float)payoutMod_JacksOrBetter):n2}";
            if (i == 0 && currentBetTable) currentBetTable.text = newTableInfo;
            else if (nextBetTable) nextBetTable.text = newTableInfo;
        }
    }
}