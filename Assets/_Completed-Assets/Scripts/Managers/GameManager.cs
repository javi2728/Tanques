using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Complete
{
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;
        public float m_StartDelay = 3f;
        public float m_EndDelay = 3f;
        public CameraControl m_CameraControl;
        public Text m_MessageText;
        public GameObject m_TankPrefab;
        public TankManager[] m_Tanks;

        [Header("Match Timer (4 minutes)")]
        public float m_MaxMatchTime = 240f;     // 4:00
        public Text m_TimerText;               // opcional (si quieres verlo en pantalla)

        [Header("Enemies (Optional)")]
        public EnemySpawner m_EnemySpawner;    // arrastra el EnemySpawner de la escena aquí (opcional)

        private int m_RoundNumber;
        private WaitForSeconds m_StartWait;
        private WaitForSeconds m_EndWait;
        private TankManager m_RoundWinner;
        private TankManager m_GameWinner;

        private float m_MatchTime;
        private bool m_CountMatchTime;
        private bool m_TimeUp;

        private void Start()
        {
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            m_MatchTime = 0f;
            m_CountMatchTime = false;
            m_TimeUp = false;

            SpawnAllTanks();
            SetCameraTargets();

            UpdateTimerUI();

            StartCoroutine(GameLoop());
        }

        private void Update()
        {
            if (!m_CountMatchTime || m_TimeUp)
            {
                UpdateTimerUI();
                return;
            }

            m_MatchTime += Time.deltaTime;

            if (m_MatchTime >= m_MaxMatchTime)
            {
                m_MatchTime = m_MaxMatchTime;
                m_TimeUp = true;
                m_CountMatchTime = false;
            }

            UpdateTimerUI();
        }

        private void SpawnAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                m_Tanks[i].m_Instance =
                    Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }

        private void SetCameraTargets()
        {
            Transform[] targets = new Transform[m_Tanks.Length];

            for (int i = 0; i < targets.Length; i++)
                targets[i] = m_Tanks[i].m_Instance.transform;

            m_CameraControl.m_Targets = targets;
        }

        private IEnumerator GameLoop()
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());

            if (m_TimeUp)
            {
                SceneManager.LoadScene(0);
            }
            else if (m_GameWinner != null)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                StartCoroutine(GameLoop());
            }
        }

        private IEnumerator RoundStarting()
        {
            ResetAllTanks();
            DisableTankControl();

            // enemigos por ronda (si lo asignaste)
            if (m_EnemySpawner != null)
            {
                m_EnemySpawner.ClearEnemies();
                m_EnemySpawner.SpawnEnemies();
            }

            m_CameraControl.SetStartPositionAndSize();

            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            yield return m_StartWait;
        }

        private IEnumerator RoundPlaying()
        {
            EnableTankControl();
            m_MessageText.text = string.Empty;

            if (!m_TimeUp)
                m_CountMatchTime = true;

            while (!OneTankLeft() && !m_TimeUp)
                yield return null;
        }

        private IEnumerator RoundEnding()
        {
            DisableTankControl();

            if (m_TimeUp)
            {
                m_RoundWinner = null;
                m_GameWinner = null;

                m_MessageText.text = "TIME UP!\n\nBOTH PLAYERS LOSE!\n\nTiempo: " + FormatTime(m_MatchTime);
                yield return m_EndWait;
                yield break;
            }

            m_RoundWinner = null;
            m_RoundWinner = GetRoundWinner();

            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            m_GameWinner = GetGameWinner();

            // no contar durante el delay final
            m_CountMatchTime = false;

            m_MessageText.text = EndMessage();

            yield return m_EndWait;
        }

        private bool OneTankLeft()
        {
            int numTanksLeft = 0;

            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            return numTanksLeft <= 1;
        }

        private TankManager GetRoundWinner()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }
            return null;
        }

        private TankManager GetGameWinner()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
            {
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }
            return null;
        }

        private string EndMessage()
        {
            string message = "DRAW!";

            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            message += "\n\n\n\n";

            for (int i = 0; i < m_Tanks.Length; i++)
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";

            message += "\nTiempo: " + FormatTime(m_MatchTime);

            if (m_GameWinner != null)
            {
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";
                message += "\n\nPuntaje: " + m_GameWinner.m_Wins;
                message += "\nTiempo: " + FormatTime(m_MatchTime);
            }

            return message;
        }

        private void ResetAllTanks()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                m_Tanks[i].Reset();
        }

        private void EnableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                m_Tanks[i].EnableControl();
        }

        private void DisableTankControl()
        {
            for (int i = 0; i < m_Tanks.Length; i++)
                m_Tanks[i].DisableControl();
        }

        private void UpdateTimerUI()
        {
            if (m_TimerText == null) return;

            float remaining = Mathf.Max(0f, m_MaxMatchTime - m_MatchTime);
            m_TimerText.text = FormatTime(remaining);
        }

        private static string FormatTime(float seconds)
        {
            int mins = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{mins:00}:{secs:00}";
        }
    }
}
