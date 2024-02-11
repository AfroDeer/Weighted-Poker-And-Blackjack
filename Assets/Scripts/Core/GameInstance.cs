//Nicholas Johnson - 2024

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

//Global "Game Instance" for preserving data across scenes and handling root logic.
public class GameInstance : MonoBehaviour
{
    //Create global instance
    public static GameInstance instance
    {
        get;
        set;
    }

    //Path Names
    private string logFilePath;
    string transactionLogName;

    //Constants
    private const uint transactionLogBuffer = 100;

    //Flags
    private bool hasStartedGame = false;

    //Data
    private Account account = new Account(10000, 0, 0, new List<Transaction>()); //Temporary account info of active play session. TODO: Create savestate system for this!
    [SerializeField]
    public List<Card> masterDeck = new List<Card>(); //Serialized data initialized via the editor, acts as a reference to a standard deck of cards. This can be optimized heavily with a hashmap, but this is easier to work with in-editor.

    //System
    protected void Awake()
    {
        //Create path names.
        logFilePath = Application.dataPath + "/Logs";
        transactionLogName = logFilePath + "/TransactionHistory.txt";
        //Create directories.
        Directory.CreateDirectory(logFilePath);
        //Generate log if it doesn't exist, if a file exists then begin a new session log.
        if (!File.Exists(transactionLogName))
        {
            File.WriteAllText(transactionLogName, "[Transaction Log]\n\n");
        }
        else
        {
            string startLine = "(" + System.DateTime.UtcNow.ToLocalTime().ToString("MM-dd-yyyy / HH:mm:ss:ff") + ")  Start Of Session";
            File.AppendAllText(transactionLogName, startLine + "\n");
        }

        //Populate instance with this class and mark for preservation.
        instance = this;
        DontDestroyOnLoad(transform.gameObject);

        //Sort master deck by suit followed by value. This is done in case editor values aren't initialized in a sorted fashion. Messy algorithim, optimization shouldn't matter since this only occurs on initialization.
        List<Card> clubsTempDeck = new List<Card>();
        List<Card> spadesTempDeck = new List<Card>();
        List<Card> heartsTempDeck = new List<Card>();
        List<Card> diamondsTempDeck = new List<Card>();
        for (int i = 0; i < masterDeck.Count; i++)
        {
            switch (masterDeck[i].suit)
            {
                case ESuit.Clubs:
                    clubsTempDeck.Add(masterDeck[i]);
                    break;
                case ESuit.Spades:
                    spadesTempDeck.Add(masterDeck[i]);
                    break;
                case ESuit.Hearts:
                    heartsTempDeck.Add(masterDeck[i]);
                    break;
                case ESuit.Diamonds:
                    diamondsTempDeck.Add(masterDeck[i]);
                    break;
                default:
                    continue;
            }
        }
        clubsTempDeck.Sort((cardA, cardB) => cardA.cardValue.CompareTo(cardB.cardValue));
        spadesTempDeck.Sort((cardA, cardB) => cardA.cardValue.CompareTo(cardB.cardValue));
        heartsTempDeck.Sort((cardA, cardB) => cardA.cardValue.CompareTo(cardB.cardValue));
        diamondsTempDeck.Sort((cardA, cardB) => cardA.cardValue.CompareTo(cardB.cardValue));
        masterDeck.Clear();
        foreach (Card tempCard in clubsTempDeck)
        {
            masterDeck.Add(tempCard);
        }
        foreach (Card tempCard in spadesTempDeck)
        {
            masterDeck.Add(tempCard);
        }
        foreach (Card tempCard in heartsTempDeck)
        {
            masterDeck.Add(tempCard);
        }
        foreach (Card tempCard in diamondsTempDeck)
        {
            masterDeck.Add(tempCard);
        }
    }
    protected void OnApplicationQuit()
    {
        WriteTransactionLog(true);
    }
    protected void Update()
    {
        if (!hasStartedGame && Input.anyKey) LoadLevel("MainMenu");
    }

    //Cache information for bulk logging later. Controlled by "transactionLogBuffer".
    private void LogTransaction(uint Amount, ETransactionType TransactionType)
    {
        //Cache logging info.
        Transaction tempTransaction = new Transaction(Amount, TransactionType, System.DateTime.UtcNow.ToLocalTime().ToString("MM-dd-yyyy / HH:mm:ss:ff"), account.credits);
        account.transactionHistory.Add(tempTransaction);

        if (account.transactionHistory.Count < transactionLogBuffer) return; //Buffer check.

        WriteTransactionLog(false);
    }

    //Write cached information to a .txt file then dump cache to free memory. Logging is handled in bulk to consolidate write calls and reduce stuttering.
    private void WriteTransactionLog(bool EndLog)
    {
        //Iterate through cache and log to the file.
        for (int i = 0; i < account.transactionHistory.Count; i++)
        {
            string cachedLine = "(" + account.transactionHistory[i].timestamp + ")  ";
            switch (account.transactionHistory[i].transactionType)
            {
                case ETransactionType.InsertedCredits:
                    cachedLine += "Transaction Type: Credits Inserted | Transaction Amount: " + account.transactionHistory[i].creditsAmount + " | Resulting Balance: " + account.transactionHistory[i].resultingBalance;
                    break;
                case ETransactionType.EarnedCredits:
                    cachedLine += "Transaction Type: Credits Earned | Transaction Amount: " + account.transactionHistory[i].creditsAmount + " | Resulting Balance: " + account.transactionHistory[i].resultingBalance;
                    break;
                case ETransactionType.SpentCredits:
                    cachedLine += "Transaction Type: Credits Spent | Transaction Amount: -" + account.transactionHistory[i].creditsAmount + " | Resulting Balance: " + account.transactionHistory[i].resultingBalance;
                    break;
                case ETransactionType.WithdrewCredits:
                    cachedLine += "Transaction Type: Credits Withdrew | Transaction Amount: -" + account.transactionHistory[i].creditsAmount + " | Resulting Balance: " + account.transactionHistory[i].resultingBalance;
                    break;
                case ETransactionType.AttemptedOverdraftCredits:
                    cachedLine += "Transaction Type: Attempted Overdraft | Transaction Amount: " + account.transactionHistory[i].creditsAmount + " | Resulting Balance: " + account.transactionHistory[i].resultingBalance;
                    break;
                default:
                    cachedLine += "ERROR: Transaction failed to log!";
                    break;
            }
            File.AppendAllText(transactionLogName, cachedLine + "\n");
        }

        if (EndLog)
        {
            string endLine = "(" + System.DateTime.UtcNow.ToLocalTime().ToString("MM-dd-yyyy / HH:mm:ss:ff") + ")  End Of Session | Credits Spent: " + account.creditsEarned + " | Credits Earned: " + account.creditsSpent;
            File.AppendAllText(transactionLogName, endLine + "\n");
        }

        account.transactionHistory.Clear(); //Dump cache.
    }

    //Create an async callback for when next scene has been loaded.
    IEnumerator ASyncLoadScene(string LevelName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(LevelName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    //Start asynchronous load and toggle state/transition flags.
    public void LoadLevel(string LevelName)
    {
        if (LevelName.Length == 0) return;

        if (LevelName == "MainMenu") hasStartedGame = true;
        StartCoroutine(ASyncLoadScene(LevelName));
    }

    //Quit application.
    public void QuitGame()
    {
        Application.Quit();
    }

    //Reveal logs in file explorer.
    public void OpenLogsFolder()
    {
        string path = logFilePath;
        path = path.Replace(@"/", @"\");   //Format for windows explorer.
        System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
    }

    //Return balance amount for gamestates.
    public uint GetCreditBalance()
    {
        return account.credits;
    }

    //Add credits to account balance and log accordingly.
    public bool AddCredits(uint Value, bool IsExternalTransaction)
    {
        if (Value == 0) return false;

        account.credits += Value;
        if (IsExternalTransaction) LogTransaction(Value, ETransactionType.InsertedCredits);
        else
        {
            account.creditsEarned += Value;
            LogTransaction(Value, ETransactionType.EarnedCredits);
        }

        return true;
    }

    //Remove credits from account balance and log accordingly.
    public bool RemoveCredits(uint Value, bool IsExternalTransaction)
    {
        if (Value == 0) return false;

        if (Value > account.credits)
        {
            LogTransaction(Value, ETransactionType.AttemptedOverdraftCredits);
        }
        else
        {
            account.credits -= Value;
            if (IsExternalTransaction) LogTransaction(Value, ETransactionType.WithdrewCredits);
            else
            {
                account.creditsSpent += Value;
                LogTransaction(Value, ETransactionType.SpentCredits);
            }
        }

        return true;
    }

    //Modify master deck draw weights.
    public bool ModifyCardDrawWeight(Card Selection, bool IsDealer, uint NewWeight)
    {
        int index = masterDeck.FindIndex(x => x.suit == Selection.suit && x.cardValue == Selection.cardValue);
        if (index < 0 || index >= masterDeck.Count) return false;

        Card tempCard = masterDeck[index];
        if (IsDealer) tempCard.dealerWeight = NewWeight;
        else tempCard.playerWeight = NewWeight;
        masterDeck[index] = tempCard;
        return true;
    }
}