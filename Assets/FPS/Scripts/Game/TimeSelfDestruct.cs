using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// TimeSelfDestruct 부착한 게임 오브젝트는 생성 후 지정된 시간에 킬
    /// </summary>
    public class TimeSelfDestruct : MonoBehaviour
    {
        #region Variables
        public float m_Time = 5;

        private float spawnTime;
        #endregion
        private void Awake()
        {
            spawnTime = Time.time;
        }
        void Update()
        {

            if ((spawnTime + m_Time) <= Time.time)
            {
                Destroy(gameObject);
            }
        }
    }
}