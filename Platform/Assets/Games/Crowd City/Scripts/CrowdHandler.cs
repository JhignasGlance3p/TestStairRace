using nostra.character;
using nostra.origami.common;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace nostra.origami.crowdcity
{
    [Serializable]
    public class CrowdCharacter
    {
        public GameObject gfxGO;
        public GameObject characterGO;
        public NavMeshAgent agent;
        public NostraCharacter nostraCharacter;
    }
    public class CrowdHandler : MonoBehaviour
    {
        [SerializeField] Camera frontCamera, backCamera, mainCamera;
        [SerializeField] Transform WorldCanvas;
        [SerializeField] bool isPlayer;
        [SerializeField] CrowdController crowdController;
        // [SerializeField] Billboard canvasBB;
        [SerializeField] SpriteRenderer MinimapIcon;
        [SerializeField] Minimap minimap;
        [Header("Formation Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float radiusFactor;
        [Range(0f, 1f)]
        [SerializeField] private float angleFactor;
        [SerializeField] private Transform followPoint;
        [Header("Crowd Info")]
        public string crowdGangName = "AI";
        [SerializeField] private TextMeshProUGUI crowdCountTxt, crowdNameTxt;

        List<CrowdCharacter> charactersInCrowd { get; set; } = new() { };
        Crowd myCrowdSettings;
        GameObject crown;
        string activeKiller;
        bool isWatch;
        CrowdCityManager gameManager;
        Transform MainCharacter;
        bool canMove = false;
        SpriteRenderer minimapSprite;

        public CrowdCityManager Manager => gameManager;
        public Crowd CrowdStatus => myCrowdSettings;
        public int CrowdCount { get; set; }
        public bool IsPlayerCrowd => isPlayer;

        void Update()
        {
            if (myCrowdSettings != null && canMove == true)
            {
                CrowdFormation();
            }
        }

        public void Initialise(CrowdCityManager crowdCityManager)
        {
            gameManager = crowdCityManager;
            // if (canvasBB != null && mainCamera != null)
            // {
            //     canvasBB.SetTarget(mainCamera.transform);
            // }
            MainCharacter = followPoint;
            crown = gameManager.Pooler.SpawnFromPool("Crown", MainCharacter, position: new Vector3(0f, 2f, 0f), scale: Vector3.one * 0.4f);
            if (minimap != null)
            {
                minimap.OrderLayer++;
                if (MinimapIcon != null)
                    MinimapIcon.sortingOrder = minimap.OrderLayer;
            }
            crowdController.Init();
            UpdateMainCharacter(followPoint);
        }
        public void SetCrowdSettings(Crowd _crowd = null)
        {
            MainCharacter = followPoint;
            myCrowdSettings = _crowd;
            isPlayer = myCrowdSettings.isPlayer;
            this.transform.position = myCrowdSettings.initialPosition;
            crowdController.transform.position = myCrowdSettings.currentPosition;
            crowdController.transform.localEulerAngles = myCrowdSettings.currentRotation;
            WorldCanvas.transform.GetChild(1).GetComponent<Image>().color = myCrowdSettings.crowdColor;
            WorldCanvas.transform.GetChild(2).GetComponent<Image>().color = myCrowdSettings.crowdColor;
            crowdNameTxt.text = myCrowdSettings.crowdGangName;
            MinimapIcon.color = myCrowdSettings.crowdColor;
        }
        public void OnRender()
        {
        }
        public void OnFocussed()
        {
            WorldCanvas.gameObject.SetActive(true);
            crowdController.SetAutoPlay(true, true);
            canMove = true;
            UpdateCrowd(myCrowdSettings.crowdCount);
            CrowdFormation(snap: true);
        }
        public void OnStart()
        {
            WorldCanvas.gameObject.SetActive(true);
            UpdateCrowd(myCrowdSettings.crowdCount);
            CrowdFormation(snap: true);
            if (isPlayer)
            {
                crowdController.SetAutoPlay(false, true);
            }
            else
            {
                crowdController.SetAutoPlay(true, true);
            }
            canMove = true;
        }
        public void OnPause()
        {
            WorldCanvas.gameObject.SetActive(true);
            crowdController.SetAutoPlay(false, false);
        }
        public void OnRestart()
        {
            Reset();
        }
        public void OnHidden()
        {
            WorldCanvas.gameObject.SetActive(false);
            foreach (CrowdCharacter crowd in charactersInCrowd)
            {
                gameManager.PutCharacterBack(crowd.characterGO);
                gameManager.Pooler.AddToPool(crowd.gfxGO.GetComponent<PooledObjectResetter>());
            }
            myCrowdSettings = null;
            CrowdCount = 0;
        }
        public void OnGameOver()
        {
            MainCharacter = followPoint;
            crowdController.OnGameOver();
            crowdController.SetAutoPlay(false, false);
            foreach (CrowdCharacter crowd in charactersInCrowd)
            {
                gameManager.PutCharacterBack(crowd.characterGO);
                gameManager.Pooler.AddToPool(crowd.gfxGO.GetComponent<PooledObjectResetter>());
            }
        }
        public void OnStop()
        {
            crowdController.SetAutoPlay(false, false);
            canMove = false;
        }
        public void OnWatch(Crowd currentStatus)
        {
            canMove = true;

            myCrowdSettings = currentStatus;
            isPlayer = myCrowdSettings.isPlayer;
            this.transform.position = myCrowdSettings.initialPosition;
            crowdController.transform.position = myCrowdSettings.currentPosition;
            crowdController.transform.localEulerAngles = myCrowdSettings.currentRotation;
            WorldCanvas.transform.GetChild(1).GetComponent<Image>().color = myCrowdSettings.crowdColor;
            WorldCanvas.transform.GetChild(2).GetComponent<Image>().color = myCrowdSettings.crowdColor;
            crowdNameTxt.text = myCrowdSettings.crowdGangName;
            MinimapIcon.color = myCrowdSettings.crowdColor;
            WorldCanvas.gameObject.SetActive(true);
            crowdController.SetAutoPlay(false, false);
            UpdateStatus(currentStatus);
        }

        public void UpdateCrowd(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject character = gameManager.Pooler.SpawnFromPool("Character", position: GetPositionInCrowd(CrowdCount + 1));
                character.transform.SetParent(this.transform);
                character.transform.localScale = Vector3.one;
                character.transform.rotation = Quaternion.Euler(Vector3.zero);
                character.name = $"{myCrowdSettings.crowdTag} ({charactersInCrowd.Count + 1})";

                GameObject characterGfx = gameManager.GetCharacter(isPlayer ? CharacterType.PLAYER : CharacterType.AI, myCrowdSettings.randomCustomiseId);
                characterGfx.name = $"{myCrowdSettings.crowdTag}{"_new"}";
                characterGfx.tag = "CharacterGfx";
                characterGfx.transform.SetParent(character.transform);
                characterGfx.transform.localPosition = Vector3.zero;
                characterGfx.transform.localScale = Vector3.one * 1.2f;
                characterGfx.SetActive(true);

                Character characterComp = character.GetComponent<Character>();
                if (isPlayer)
                {
                    if (characterComp == null)
                        characterComp = character.AddComponent<Character>();

                    characterComp.SetCharacter(this);
                }
                else if (!isPlayer && characterComp != null)
                    Destroy(characterComp);

                NavMeshAgent agent = character.GetComponent<NavMeshAgent>();
                if (agent == null)
                    agent = character.AddComponent<NavMeshAgent>();
                agent.acceleration = isPlayer ? 8 : crowdController.Agent.acceleration;
                agent.speed = isPlayer ? 9 : crowdController.Agent.speed;
                agent.angularSpeed = isPlayer ? 120 : crowdController.Agent.angularSpeed;
                agent.obstacleAvoidanceType = crowdController.Agent.obstacleAvoidanceType;

                CrowdCharacter characterToAdd = new()
                {
                    gfxGO = character,
                    characterGO = characterGfx,
                    agent = agent,
                    nostraCharacter = characterGfx.GetComponent<NostraCharacter>(),
                };
                charactersInCrowd.Add(characterToAdd);
                CrowdCount++;
            }
            if (gameManager.GameOver == false && CrowdCount >= 30)
                gameManager.ResetPowerup();
            crowdCountTxt.text = $"{CrowdCount}";
        }
        public void RemoveCrowd(GameObject characterToRemove = null, int removeCount = 0, string killer = "", bool isReset = false)
        {
            activeKiller = killer;
            if (characterToRemove != null)
            {
                if (charactersInCrowd.Count > 0)
                {
                    CrowdCharacter character = charactersInCrowd.Find(m_Charater => m_Charater.gfxGO == characterToRemove);
                    if (character != null)
                    {
                        gameManager.PutCharacterBack(character.characterGO);
                        gameManager.Pooler.AddToPool(character.gfxGO.GetComponent<PooledObjectResetter>());
                        charactersInCrowd.Remove(character);
                    }
                }
                CrowdCount--;
            }
            else
            {
                removeCount = removeCount > charactersInCrowd.Count ? charactersInCrowd.Count : removeCount;
                for (int i = 0; i < removeCount; i++)
                {
                    CrowdCharacter character = charactersInCrowd[^1];
                    gameManager.PutCharacterBack(character.characterGO);
                    gameManager.Pooler.AddToPool(character.gfxGO.GetComponent<PooledObjectResetter>());
                    charactersInCrowd.Remove(character);
                    CrowdCount--;
                }
            }
            if (CrowdCount <= 0)
            {
                if (isReset == false)
                {
                    crowdController.SetAutoPlay(false, false);
                    if (isPlayer)
                    {
                        gameManager.LoseGame(activeKiller);
                    }
                    else
                    {
                        myCrowdSettings.crowdCount = UnityEngine.Random.Range(3, 7);
                        myCrowdSettings.initialPosition = GetRandomAIPoints();
                        myCrowdSettings.currentPosition = myCrowdSettings.initialPosition;
                        myCrowdSettings.currentRotation = Vector3.zero;
                        SetCrowdSettings(myCrowdSettings);
                        MyUtils.Execute(0.1f, () =>
                        {
                            UpdateCrowd(myCrowdSettings.crowdCount);
                            CrowdFormation(snap: true);
                            crowdController.SetAutoPlay(true, true);
                        });
                    }
                }
            }
            if (!isPlayer && string.IsNullOrEmpty(activeKiller) == false)
            {
                gameManager.UpdateKill(removeCount);
            }
            CrowdCount = Mathf.Clamp(CrowdCount, 0, CrowdCount);
            crowdCountTxt.text = $"{CrowdCount}";
        }
        public void UpdateStatus(Crowd currentStatus)
        {
            myCrowdSettings = currentStatus;
            crowdController.transform.position = currentStatus.currentPosition;
            crowdController.transform.localEulerAngles = currentStatus.currentRotation;
            if (CrowdCount < currentStatus.currentCount)
            {
                UpdateCrowd(currentStatus.currentCount - CrowdCount);
                foreach (CrowdCharacter character in charactersInCrowd)
                {
                    // character.nostraCharacter.PlayNostraCharAnim(0, 1f);
                }
            }
            else if (CrowdCount > currentStatus.currentCount)
            {
                RemoveCrowd(null, (CrowdCount - currentStatus.currentCount));
            }

            for (int i = 0; i < charactersInCrowd.Count; i++)
            {
                Vector3 globalPosition = GetPositionInCrowd(i);
                Vector3 finalPosition = charactersInCrowd[i].gfxGO.transform.position;
                finalPosition.x = globalPosition.x;
                finalPosition.z = globalPosition.z;
                charactersInCrowd[i].gfxGO.transform.position = finalPosition;
                charactersInCrowd[i].gfxGO.transform.localEulerAngles = currentStatus.currentRotation;
                // charactersInCrowd[i].nostraCharacter.PlayNostraCharAnim(0, 1f);
            }
        }
        public void CrowdFormation(bool snap = false)
        {
            if (crowdController.Agent != null && crowdController.Agent.isOnNavMesh == true)
            {
                for (int i = 0; i < charactersInCrowd.Count; i++)
                {
                    // charactersInCrowd[i].nostraCharacter.PlayNostraCharAnim(0, crowdController.Agent.remainingDistance);
                }
            }

            for (int i = 0; i < charactersInCrowd.Count; i++)
            {
                Vector3 globalPosition = GetPositionInCrowd(i);
                Vector3 finalPosition = crowdController.transform.position;
                finalPosition.x = globalPosition.x;
                finalPosition.z = globalPosition.z;

                if (snap)
                {
                    charactersInCrowd[i].gfxGO.transform.position = finalPosition;
                    if (charactersInCrowd[i].agent.isOnNavMesh)
                        charactersInCrowd[i].agent.SetDestination(finalPosition);
                }
                else
                {
                    if (charactersInCrowd[i].agent.enabled)
                    {
                        // if (gameManager.GameOver == false)
                        // {
                        float distance = MyUtils.GetDistanceXZ(charactersInCrowd[i].agent.transform.position,
                                                    finalPosition);
                        if (distance < 0.005f)
                        {
                            // charactersInCrowd[i].nostraCharacter.PlayNostraCharAnim(0, 0f);
                        }
                        else
                        {
                            // charactersInCrowd[i].nostraCharacter.PlayNostraCharAnim(0, 1f);
                        }
                        charactersInCrowd[i].agent.transform.position = finalPosition;
                        charactersInCrowd[i].agent.transform.localEulerAngles = crowdController.transform.localEulerAngles;
                        // }
                    }
                }
            }
            if (myCrowdSettings != null)
            {
                myCrowdSettings.currentCount = CrowdCount;
                myCrowdSettings.currentPosition = crowdController.transform.position;
                myCrowdSettings.currentRotation = crowdController.transform.localEulerAngles;
            }
        }
        public Vector3 GetRandomAIPoints()
        {
            return gameManager.GetRandomAIPoints();
        }

        private Vector3 GetPositionInCrowd(int i)
        {
            float goldenAngle = 137.5f * angleFactor;
            float x = radiusFactor * Mathf.Sqrt(i + 1) * Mathf.Cos(Mathf.Deg2Rad * goldenAngle * (i + 1));
            float z = radiusFactor * Mathf.Sqrt(i + 1) * Mathf.Sin(Mathf.Deg2Rad * goldenAngle * (i + 1));

            Vector3 playerLocalPosition = new(x, 0, z);
            Vector3 globalPosition = followPoint.TransformPoint(playerLocalPosition);

            return globalPosition;
        }
        private float GetSquadRadius()
        {
            return radiusFactor * Mathf.Sqrt(transform.childCount);
        }
        private void Reset()
        {
            MainCharacter = followPoint;
            crowdController.SetAutoPlay(false, false);
            RemoveCrowd(null, removeCount: charactersInCrowd.Count, killer: string.Empty, isReset: true);
            canMove = false;
            crowdController.transform.localPosition = Vector3.zero;
            crowdController.transform.localEulerAngles = Vector3.zero;
        }
        private void UpdateMainCharacter(Transform _mainCharacter)
        {
            MainCharacter = _mainCharacter;
            Vector3 firstCharPosition = GetPositionInCrowd(0);
            if (crown != null)
            {
                crown.transform.SetParent(MainCharacter);
                firstCharPosition.y = 1.9f;
                crown.transform.position = firstCharPosition;
                crown.transform.rotation = Quaternion.identity;
                crown.transform.localScale = Vector3.one * 0.3f;
            }
            if (WorldCanvas != null)
            {
                WorldCanvas.transform.SetParent(MainCharacter);
                firstCharPosition.y = 3.5f;
                WorldCanvas.transform.position = firstCharPosition;
            }
            if (MinimapIcon != null) MinimapIcon.transform.SetParent(MainCharacter);
            if (minimap != null && isPlayer)
            {
                minimap.Player = MainCharacter;
            }
            if (isPlayer && MainCharacter == followPoint)
            {
                frontCamera.transform.SetParent(MainCharacter.transform);
                backCamera.transform.SetParent(MainCharacter.transform);
                SmoothCameraFollow cameraFollow = mainCamera.gameObject.GetComponent<SmoothCameraFollow>();
                Cam cam = mainCamera.gameObject.GetComponent<Cam>();
                cameraFollow.playerTarget = MainCharacter;
                cameraFollow.enabled = true;
                cam.enabled = true;

                cameraFollow = frontCamera.gameObject.GetComponent<SmoothCameraFollow>();
                cam = frontCamera.gameObject.GetComponent<Cam>();
                cameraFollow.playerTarget = MainCharacter;
                cameraFollow.enabled = true;
                cam.enabled = true;

                cameraFollow = backCamera.gameObject.GetComponent<SmoothCameraFollow>();
                cam = backCamera.gameObject.GetComponent<Cam>();
                cameraFollow.playerTarget = MainCharacter;
                cameraFollow.enabled = true;
                cam.enabled = true;
            }
        }
    }
}