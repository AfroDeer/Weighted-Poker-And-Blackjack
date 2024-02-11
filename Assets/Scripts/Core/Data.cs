//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EGameStep
{
    Standby,
    DealerTurn,
    PlayerTurn,
    Finished
}

[System.Serializable]
public enum ETransactionType
{
    InsertedCredits,
    EarnedCredits,
    SpentCredits,
    WithdrewCredits,
    AttemptedOverdraftCredits
}

[System.Serializable]
public struct Transaction
{
    public uint creditsAmount;
    public ETransactionType transactionType;
    public string timestamp;
    public uint resultingBalance;

    public Transaction(uint CreditsAmount, ETransactionType TransactionType, string Timestamp, uint ResultingBalance)
    {
        this.creditsAmount = CreditsAmount;
        this.transactionType = TransactionType;
        this.timestamp = Timestamp;
        this.resultingBalance = ResultingBalance;
    }
}

[System.Serializable]
public struct Account
{
    public uint credits;
    public uint creditsSpent;
    public uint creditsEarned;
    public List<Transaction> transactionHistory;

    public Account(uint Credits, uint CreditsSpent, uint CreditsEarned, List<Transaction> TransactionHistory)
    {
        this.credits = Credits;
        this.creditsSpent = CreditsSpent;
        this.creditsEarned = CreditsEarned;
        this.transactionHistory = TransactionHistory;
    }
}

[System.Serializable]
public enum ESuit
{
    Clubs,
    Spades,
    Hearts,
    Diamonds
}

[System.Serializable]
public enum ECardValue
{
    Ace,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King
}

[System.Serializable]
public struct Card
{
    public ECardValue cardValue;
    public ESuit suit;
    public uint playerWeight;
    public uint dealerWeight;
    public Sprite image;

    public Card(ECardValue CardValue, ESuit Suit, uint PlayerWeight, uint DealerWeight, Sprite Image)
    {
        this.cardValue = CardValue;
        this.suit = Suit;
        this.playerWeight = PlayerWeight;
        this.dealerWeight = DealerWeight;
        this.image = Image;
    }
}

[System.Serializable]
public struct PlayingCard
{
    public Card cardData;
    public bool isRevealed;

    public PlayingCard(Card CardData, bool IsRevealed)
    {
        this.cardData = CardData;
        this.isRevealed = IsRevealed;
    }
}