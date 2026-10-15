using UnityEngine;
using UnityEngine.UI;

namespace CosmicBatter
{
    public class GameUI : MonoBehaviour
    {
        public GameManager game;
        public GameObject startPanel, resultPanel, hud;
        public Text scoreText, timerText, resultText;
        public Text speedText, heavyText, slowText;
        public Button startButton, restartButton;
        void Awake() { startButton.onClick.AddListener(game.BeginRound); restartButton.onClick.AddListener(game.BeginRound); }
        public void ShowStart() { startPanel.SetActive(true); resultPanel.SetActive(false); hud.SetActive(false); }
        public void ShowGame() { startPanel.SetActive(false); resultPanel.SetActive(false); hud.SetActive(true); Refresh(); }
        public void ShowResult()
        {
            resultPanel.SetActive(true); hud.SetActive(false);
            resultText.text = "TIME'S UP!\n\n" + game.Score + " POINTS\n\nRANK  " + game.Rank;
        }
        public void Refresh()
        {
            scoreText.text="SCORE  "+game.Score;
            timerText.text=Mathf.CeilToInt(game.Remaining).ToString("00")+" s";
            ShowEffect(speedText,"SPEED +30%",game.player.SpeedRemaining);
            ShowEffect(heavyText,"TRIPLE FIRE",game.player.HeavyRemaining);
            ShowEffect(slowText,"SLOWED -40%",game.player.SlowRemaining);
        }
        static void ShowEffect(Text label,string title,float remaining)
        {
            if (!label) return;
            label.gameObject.SetActive(remaining>0);
            label.text=title+"  "+remaining.ToString("0.0")+"s";
        }
    }
}
