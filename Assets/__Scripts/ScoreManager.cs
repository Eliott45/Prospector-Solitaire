using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Перечесление со всеми событиями начисление очков
/// </summary>
public enum eScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

/// <summary>
/// Управляет подчетом очков
/// </summary>
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    // Поля для хранение информации о заработанных очках
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    private void Awake()
    {
        if(S == null)
        {
            S = this; // Подготовка скрытого объекта-одиночки
        } else
        {
            Debug.LogError("Error: ScoreManager.Awake(): S is already set!");
        }

        // Проверить рекорд в PlayerPrefs
        if(PlayerPrefs.HasKey ("HighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("HighScore");

        }
        // Добавить очки, заработанные в последнем раунде которое должны быть >0, если раунд завершился победой
        score += SCORE_FROM_PREV_ROUND;
        // И сбросить SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;
    }

    static public void EVENT(eScoreEvent evt)
    {
        try
        {  
            S.Event(evt);
        }
        catch (System.NullReferenceException nre)
        {
            Debug.LogError("ScoreManager.EVENT() called while S=null.\n" + nre);
        }
    }

    void Event(eScoreEvent evt)
    {
        switch(evt)
        {
            // В случаи победы, проигрыша и завершения хода выполняются одни и те же действия
            case eScoreEvent.draw: // Выбор свободной карты
            case eScoreEvent.gameWin: // Победа в раунде
            case eScoreEvent.gameLoss: // Проигрыш в раунде 
                chain = 0;  // Сбросить цепочку подсчета очков
                score += scoreRun; // Добавить scoreRun к общему числу очков
                scoreRun = 0; 
                break;
            case eScoreEvent.mine: // Удаление карты из основной расскадки
                chain++; // увеличить количество очков в цепочке
                scoreRun += chain; // Добавить очки за карту
                break;
            case eScoreEvent.mineGold: // Удаление золотой карты 
                chain++;
                scoreRun *= 2;
                if (scoreRun == 0) scoreRun++;
                break;
        }

        Debug.Log(HIGH_SCORE);

        // Обрабатывает победу и проигрыш в раунде
        switch(evt)
        {
            case eScoreEvent.gameWin:
                // В случае победы перенести очки в следующий раунд статические поля НЕ сбрасываются вызовом SceneManager.LoadScene()
                SCORE_FROM_PREV_ROUND = score;
                // print("You won this round! Round score: " + score);
                break;
            case eScoreEvent.gameLoss:
                // В случае проигрыша сравнить с рекордом
                if(HIGH_SCORE <= score)
                {
                    // print("You got the high score! High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("HighScore", score);
                } else
                {
                    // print("Your final score for the game was " + score);
                }
                break;
            default:
                // print($"score: {score} scoreRun: {scoreRun} chain: {chain}");
                break;
        }
    }

    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int CHAIN_RUN { get { return S.scoreRun; } }
}
