using Unity.VisualScripting;
using UnityEngine;

namespace Unity.FPS.Game
{
    /// <summary>
    /// 오디오 플레이 관련 기능 구현
    /// </summary>
    public class AudioUtility : MonoBehaviour
    {
        //지정된 위치에 게임오브젝트 생성 후 생성한 게임오브젝트 속에 오디오 소스 컴포넌트를 추가해서 지정된 클립을 플레이한다
        //클립 사운드 플레이가 끝나면 자동으로 킬한다 - TimeSelfDestruct 컴포넌트 이용
        public static void CreateSfx(AudioClip clip, Vector3 position, float spartialBlend, float rolloffDistanceMin = 1f)
        {
            var impackSfx = new GameObject().AddComponent<AudioSource>();
            impackSfx.name = "ImpackSfx";
            impackSfx.clip = clip;
            impackSfx.transform.position = position;
            impackSfx.playOnAwake = false;
            impackSfx.spatialBlend = spartialBlend;
            impackSfx.minDistance = rolloffDistanceMin;
            impackSfx.Play();

            TimeSelfDestruct timeSelf = impackSfx.AddComponent<TimeSelfDestruct>();
            timeSelf.m_Time = clip.length;
        }
    }
}