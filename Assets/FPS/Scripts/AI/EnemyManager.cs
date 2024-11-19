using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    /// <summary>
    /// Enemy 리스트를 관리하는 클래스
    /// </summary>
    public class EnemyManager : MonoBehaviour
    {
        #region Vaiables
        public List<EnemyController> enemies {  get; private set; }
        public int NumberOfenemiesTotal { get; private set; }           //총 생산된 enemies 수
        public int NumberofenemiesRemaining => enemies.Count;
        #endregion

        private void Awake()
        {
            enemies = new List<EnemyController>();
        }

        //등록
        public void RegisterEemies(EnemyController newEnemy)
        {
            enemies.Add(newEnemy);
            NumberOfenemiesTotal++;
        }
        //제거
        public void RemoveEnemies(EnemyController removeEnemy)
        {
            enemies.Remove(removeEnemy);
        }
    }
}