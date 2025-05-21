using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
namespace nostra.PKPL.JailBreakGame
{
    public class AudioManager : MonoBehaviour
    {
        public AudioSource audioSource;
        public AudioSource gameMusicSource;
        public AudioSource gameOverMusicSource;

        public AudioClip keySpawned, keyPicked, keyUsed, escaped, dead, countdown, countdownStart;
        public void PlayGameMusic()
        {
            gameMusicSource.Play();
            DOTween.To(() => gameMusicSource.volume, x => gameMusicSource.volume = x, 1, 1);
            DOTween.To(() => gameOverMusicSource.volume, x => gameOverMusicSource.volume = x, 0, 1).OnComplete(() => gameOverMusicSource.Stop());
        }

        public void PlayGameOverMusic()
        {
            gameOverMusicSource.Play();
            DOTween.To(() => gameMusicSource.volume, x => gameMusicSource.volume = x, 0, 1).OnComplete(() => gameMusicSource.Stop());
            DOTween.To(() => gameOverMusicSource.volume, x => gameOverMusicSource.volume = x, 1, 1);
        }

        public void PlayClip(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}