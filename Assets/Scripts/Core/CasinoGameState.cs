//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CasinoGameState : GameState
{
    //References
    [SerializeField]
    public Sprite cardBackImage;
    [SerializeField]
    public TextMeshProUGUI creditDisplay;
    [SerializeField]
    public TextMeshProUGUI betDisplay;
    [SerializeField]
    public Button increaseBetButton;
    [SerializeField]
    public Button decreaseBetButton;
    [SerializeField]
    public Button maxBetButton;
    [SerializeField]
    public TextMeshProUGUI gameSpeedText;
    [SerializeField]
    public Button playButton;
    [SerializeField]
    public TextMeshProUGUI playText;
    [SerializeField]
    public TextMeshProUGUI resultTypeText;
    [SerializeField]
    public TextMeshProUGUI resultAmountText;
    [SerializeField]
    public TextMeshProUGUI playCreditsText;

    //Data
    protected uint gameSpeed = 1;
    protected uint currentBet = 1;
    protected EGameStep gameStep = EGameStep.Standby;

    //Constants
    protected const uint betModifier = 50;
    protected const uint maxBet = 5;

    //System
    protected virtual void Start()
    {
        gameSpeed = 1;
        currentBet = 1;
        gameStep = EGameStep.Standby;
        UpdateCreditDisplay();
        UpdateBetDisplay();
    }

    //Change game step to act as a logic gate and update UI.
    protected virtual void SetGameStep(EGameStep Step)
    {
        gameStep = Step;
        switch (Step)
        {
            case EGameStep.Standby:
            case EGameStep.Finished:
                if (increaseBetButton) increaseBetButton.interactable = true;
                if (decreaseBetButton) decreaseBetButton.interactable = true;
                if (maxBetButton) maxBetButton.interactable = true;
                break;
            case EGameStep.DealerTurn:
            case EGameStep.PlayerTurn:
                if (increaseBetButton) increaseBetButton.interactable = false;
                if (decreaseBetButton) decreaseBetButton.interactable = false;
                if (maxBetButton) maxBetButton.interactable = false;
                break;
            default:
                Debug.LogError("BUGSPLAT: Game State went out of index of the Game Step logic! Returning to main menu to prevent a softlock!");
                LoadLevel("MainMenu");
                break;
        }
    }

    //Change game speed via a UI event and update visuals.
    public void ChangeGameSpeed()
    {
        if (gameSpeed >= 3) gameSpeed = 1;
        else gameSpeed++;

        switch (gameSpeed)
        {
            case 1:
                gameSpeedText.text = ">";
                break;
            case 2:
                gameSpeedText.text = ">>";
                break;
            case 3:
                gameSpeedText.text = ">>>";
                break;
            default:
                gameSpeedText.text = "ERROR";
                break;
        }
    }

    //Update card prop with appropriate graphics.
    protected virtual void UpdateCardDisplay(GameObject cardProp, PlayingCard cardInfo)
    {
        if (!cardProp) return;
        Image img = cardProp.GetComponent<Image>();
        if (!img) return;
        if (cardInfo.isRevealed) img.overrideSprite = cardInfo.cardData.image;
        else img.overrideSprite = cardBackImage;
    }

    //Update credit display UI.
    protected virtual void UpdateCreditDisplay()
    {
        if (!creditDisplay) return;
        float creditAmount = (float)GetCreditBalance() / 100.0f;
        creditDisplay.text = "CREDIT $" + $"{creditAmount:n2}";
    }

    //Update bet display UI.
    protected virtual void UpdateBetDisplay()
    {
        if (!betDisplay) return;
        float betAmount = (float)(currentBet * betModifier) / 100.0f;
        betDisplay.text = "BET $" + $"{betAmount:n2}";
    }

    //Check credit balance on game instance.
    protected uint GetCreditBalance()
    {
        return GameInstance.instance.GetCreditBalance();
    }

    //Insert or withdraw credits.
    protected bool InsertCredit(uint Amount)
    {
        bool wasSuccessful = GameInstance.instance.AddCredits(Amount * betModifier, true);
        UpdateCreditDisplay();
        return wasSuccessful;
    }
    protected bool WithdrawCredit(uint Amount)
    {
        uint creditAmount = Amount * betModifier;
        if (GetCreditBalance() < creditAmount) return false;
        bool wasSuccessful = GameInstance.instance.RemoveCredits(creditAmount, true);
        UpdateCreditDisplay();
        return wasSuccessful;
    }

    //Commits the desired bet amount and processes a transaction to the account.
    protected bool CommitBet()
    {
        if (currentBet <= 0 || currentBet > maxBet) return false;
        uint wagerAmount = currentBet * betModifier;
        if (GetCreditBalance() < wagerAmount) return false;
        bool wasSuccessful = GameInstance.instance.RemoveCredits(currentBet * betModifier, false);
        UpdateCreditDisplay();
        return wasSuccessful;
    }

    //Sets desired bet amount.
    protected bool AdjustBet(uint BetAmount)
    {
        if (BetAmount > maxBet || BetAmount <= 0) return false;
        currentBet = BetAmount;
        UpdateBetDisplay();
        return true;
    }

    //Receive event when bet buttons are clicked.
    public void BetButtonClicked(bool wantsToRaise)
    {
        if (wantsToRaise) AdjustBet(currentBet + 1);
        else AdjustBet(currentBet - 1);
    }

    //Receive event when max bet button is clicked.
    public void MaxBetButtonClicked()
    {
        AdjustBet(maxBet);
    }

    //Receive event when insert credits button is clicked.
    public void Insert20CreditsButtonClicked()
    {
        GameInstance.instance.AddCredits(2000, true);
        UpdateCreditDisplay();
    }

    //Receive event when withdraw credits button is clicked.
    public void WithdrawCreditsButtonClicked()
    {
        GameInstance.instance.RemoveCredits(GetCreditBalance(), true);
        UpdateCreditDisplay();
    }
}