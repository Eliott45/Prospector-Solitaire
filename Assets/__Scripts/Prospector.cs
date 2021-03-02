using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Prospector : MonoBehaviour
{
    static public Prospector S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 2f; // Задержка между раундами 2 секунды
    public Text gameOverText, roundResultText, highScoreText;
    public GameObject panel;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;

    void Awake()
    {
        S = this; // Подготовка объекта одиночки Prospector
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        // Настроить объект HighScore
        GameObject go = GameObject.Find("HighScore");
        if(go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        highScoreText.text = hScore;

        // Настроить надписи, отображаемые в конце раунда
        go = GameObject.Find("GameOver");
        if(go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }

        go = GameObject.Find("RoundResult");

        if(go!=null)
        {
            roundResultText = go.GetComponent<Text>();
        }

        // Скрыть надписи
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
        panel.gameObject.SetActive(show);
    }

    /// <summary>
    /// С вероятностью 10% сделать карту золотой.
    /// </summary>
    void MakeGoldCards()
    {
        float chance;
        for (int i = 0; i < tableau.Count; i++) // Перебрать основную стопку карт
        {
            chance = Random.Range(0, 100); // Шанс золотой карты
            if(chance < 10)
            {
                var _tGo = tableau[i].back.GetComponent<SpriteRenderer>();
                _tGo.sprite = deck.cardBackGold;
                _tGo = tableau[i].GetComponent<SpriteRenderer>();
                _tGo.sprite = deck.cardFrontGold;
            }
        }
    }

    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

        deck = GetComponent<Deck>(); // Получить компонент Deck
        deck.InitDeck(deckXML.text); // Передать ему DeckXML
        Deck.Shuffle(ref deck.cards); // Перемешать колоду, передав ее по ссылке

        /*
        Card c;
        for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        {
            c = deck.cards[cNum];
            c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        }
        */

        layout = GetComponent<Layout>(); // Получить компонент Layout
        layout.ReadLayout(layoutXML.text); // Передать ему содержимое LayoutXML
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    /// <summary>
    /// Функция снимает одну карту с вершины drawPile и возращает ее
    /// </summary>
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0]; // Снять 0-ю карту CardProspector
        drawPile.RemoveAt(0); // Удалить из list<> drawpile
        return (cd);
    }

    /// <summary>
    /// Размещает карты в начальной раскладке - "шахте"
    /// </summary>
    void LayoutGame()
    {
        // Создать пустой игровой объект, который будет служить центром раскладки 
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor"); // Создать пустой игровой объект
            layoutAnchor = tGO.transform; // Получить его компонент transform
            layoutAnchor.transform.position = layoutCenter; // Поместить в центр
        }

        CardProspector cp;

        // Разложить карты
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw(); // Выбрать первую карту (сверху) из стопки draw pile;
            cp.faceUp = tSD.faceUp; // Установить ее признак faceUp в соответствии с определением в SlotDef
            cp.transform.parent = layoutAnchor; // Назначить layoutAnchor ее родителем
            // ^ Эта операция заменит предыдущего родителя: deck.deckAnchor, который после запуска игры отображается в иерархии с именем _Deck.
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID); // Установить localPosition из SlotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            // Карты CardProspector в основной раскладке имеют состояние CardState.tableau
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName); // Назначить слой сортировки

            tableau.Add(cp); // Добавить карту в список
        }



        // Настроить списки карт, мешающих перевернуть данную 
        foreach (CardProspector tCP in tableau)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        // Выбрать начальную целевую карту
        MoveToTarget(Draw());

        // Разложить стопку свободных карт
        UpdateDrawPile();

        MakeGoldCards();
    }

    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach(CardProspector tCP in tableau)
        {
            // Поиск по всем картам в списке tableau
            if(tCP.layoutID == layoutID)
            {
                // Если номер слота карты совпадает с искомым, вернуть ее 
                return (tCP);
            }
        }
        return null;
    }

    /// <summary>
    /// Поворачивает карты в основной раскладке лицевой стороной вверх или вниз 
    /// </summary>
    void SetTableauFaces()
    {
        foreach(CardProspector cd in tableau)
        {
            bool faceUp = true; // Предположить что карта должна быть повернута лицевой стороной вверх
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // Если любая из карт, перекрывающих текущую, присутствует в основной раскладке
                if(cover.state == eCardState.tableau)
                {
                    faceUp = false; // Повернуть лицевой стороной вниз
                }
            }
            cd.faceUp = faceUp; // Покернуть карту так или иначе
        }
        
    }

    /// <summary>
    /// Перемещает текущую целевую карту в стопку сброшенных карт
    /// </summary>
    void MoveToDiscard(CardProspector cd)
    {
        // Установить состояние карты как discard (сброшена)
        cd.state = eCardState.discard;
        discardPile.Add(cd); // Добавить ее в список
        cd.transform.parent = layoutAnchor;

        // Переместить эту карту в позицию стопки сброшенных карт
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID+0.5f);
        cd.faceUp = true; // Повернуть лицевой стороной вверх
        // Настроить сортировку по глубине
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    /// <summary>
    /// Делает карту cd новой целевой картой
    /// </summary
    void MoveToTarget(CardProspector cd)
    {
        // Если целевая карта существует, переместить ее в стопку сброшенных карт
        if (target != null) MoveToDiscard(target);
        target = cd; // cd - Новая целевая карта
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        // Переместить на место для целевой карты
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true; // Повернуть лицевой стороной вверх
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    /// <summary>
    /// Расскладывает стопку свободных карт, чтобы был видно, сколько карт осталось
    /// </summary>
    void UpdateDrawPile()
    {
        CardProspector cd;
        // Выпошлнить обход всех свободных карт в drawPile
        for (int i = 0; i <drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            // Расположить с учетом смещения layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i*dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f*i );
            cd.faceUp = false; // Повернуть лицевой стороной вниз
            cd.state = eCardState.drawpile;
            // Настроить сортировку по глубине
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardProspector cd)
    {
        // Реакция определяется состоянием карты
        switch(cd.state)
        {
            case eCardState.target:
                // Щелчок на целевой карте игнорируется
                break;
            case eCardState.drawpile:
                // Щелчок на любой карте в стопке свободных карт приводит к смене целевой карты
                MoveToDiscard(target); // Переместить целевую карту в discardPile
                MoveToTarget(Draw()); // Переместить верхнюю свободную карту на место целевой
                UpdateDrawPile(); // Повторно разложить стопку свободных карт
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                // Для карты в основной расккладке проверятеся возможностоь ее перемещение на место целевой
                bool validMatch = true;
                if(!cd.faceUp)
                {
                    // Карта, повернутая лицевой стороной вниз, не может перемещаться
                    validMatch = false;
                }
                if(!AdjacentRank(cd, target))
                {
                    // Если правило старшинства не соблюдается, карта не может перемещаться
                    validMatch = false;
                }
                if (!validMatch) return;

                tableau.Remove(cd); // Удалить из списка tableau
                MoveToTarget(cd); // Сделать эту карту целевой
                SetTableauFaces(); // Повернуть карты в основной раскладке лицевой стороной вниз или вверх
                if (cd.GetComponent<SpriteRenderer>().sprite == deck.cardFrontGold)
                {
                    ScoreManager.EVENT(eScoreEvent.mineGold);
                    FloatingScoreHandler(eScoreEvent.mineGold);
                } else
                {
                    ScoreManager.EVENT(eScoreEvent.mine);
                    FloatingScoreHandler(eScoreEvent.mine);
                }
                
                break;
        }

        CheckForGameOver();
    }

    /// <summary>
    /// Проверяет завершение игры
    /// </summary>
    void CheckForGameOver() 
    {
        // Если оснавная раскладка опустел, игра завершена
        if(tableau.Count == 0)
        {
            // Вызвать GameOver() с признаком победы
            GameOver(true);
            return;
        }

        // Если еще есть сводобные карты, игра не завершилась
        if (drawPile.Count > 0)
        {
            return;
        }

        // Проверить наличие допустимых ходов
        foreach (CardProspector cd in tableau)
        {
            if(AdjacentRank(cd, target))
            {
                // Если есть допустимый ход, игра не завершилась
                return;
            }
        }

        // Если допустимых ходов нет
        GameOver(false);
    }

    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if(won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round!\nRound Score: " + score;
            ShowResultsUI(true);
            // print("Game Over. You won! ^_^");
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        } else
        {
            gameOverText.text = "Game Over";
            if(ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            } else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            // print("Game Over. You Lost. :(");
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }

        // SceneManager.LoadScene("__Prospector_Scene_0");
        Invoke("ReloadLevel", reloadDelay);
    }

    /// <summary>
    /// Перезагрузить сцену и сбросить игру в исходное состояние
    /// </summary>
    void ReloadLevel()
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    /// <summary>
    /// Возвращает true, если две карты соответствуют правилу старшинства (с учатом циклического переноса старшинства между тузом и королем)
    /// </summary>
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // Если любая из карт повернута лицевой стороной вниз, правило старшинства не соблюдается.
        if (!c0.faceUp || !c1.faceUp) return false;

        // Если достоинство карт отлицаются на 1, правило старшинства соблюдается
        if(Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }

        // Если одна карта - туз, а другая - король, правило старшиества собюдается
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);

        return false;
    }

    /// <summary>
    /// Обрабатывает движение FloatingScore
    /// </summary>
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch(evt)
        {
            // В случае победы, проигрыша и завершения хода выполняются одни и те же действия
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                // Добавить fsRun в ScoreBoard
                if(fsRun != null)
                {
                    // Создать точки для кривой Безье
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // Также скорректировать fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 29, 36, 4 });
                    fsRun = null; // Очистить fsRun, чтобы создать заново
                }
                break;
            case eScoreEvent.mine: // Удалениие карты из основной раскладки
                // Создать FloatingScore для отображения этого количества очков 
                FloatingScore fs;
                // Переместить из позиции указателя мыши mousePosition в fdPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if(fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                } else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
            case eScoreEvent.mineGold:
                p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                if(ScoreManager.CHAIN_RUN == 1)
                {
                    fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN_RUN, fsPts);
                } else
                {
                    fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN_RUN / 2, fsPts);
                }

                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }

                break;
        }
    }
}
