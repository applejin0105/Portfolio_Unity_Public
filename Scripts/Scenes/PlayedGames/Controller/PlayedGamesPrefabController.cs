using System;
using System.Collections.Generic;
using Core.Managers;
using Scenes.PlayedGames.Data;
using Scenes.PlayedGames.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.PlayedGames.Controller
{
    public class PlayedGamesPrefabController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playedGameIDTitleTxt;
        [SerializeField] private TextMeshProUGUI playedGameIDTitleShadowTxt;

        [SerializeField] private TextMeshProUGUI playedGameTitleTxt;
        [SerializeField] private TextMeshProUGUI playedGameTitleShadowTxt;

        [SerializeField] private TextMeshProUGUI originalityScoreTxt;
        [SerializeField] private TextMeshProUGUI originalityScoreShadowTxt;

        [SerializeField] private TextMeshProUGUI developSoulScoreTxt;
        [SerializeField] private TextMeshProUGUI developSoulScoreShadowTxt;

        [SerializeField] private TextMeshProUGUI uniquenessScoreTxt;
        [SerializeField] private TextMeshProUGUI uniquenessScoreShadowTxt;

        [SerializeField] private TextMeshProUGUI atmosphereScoreTxt;
        [SerializeField] private TextMeshProUGUI atmosphereScoreShadowTxt;

        [SerializeField] private TextMeshProUGUI immersionScoreTxt;
        [SerializeField] private TextMeshProUGUI immersionScoreShadowTxt;

        [SerializeField] private TextMeshProUGUI memorableScoreTxt;
        [SerializeField] private TextMeshProUGUI memorableScoreShadowTxt;

        [SerializeField] private TextMeshProUGUI descriptionTxt;

        [SerializeField] private TextMeshProUGUI playTimeTxt;
        [SerializeField] private TextMeshProUGUI playTimeShadowTxt;

        [SerializeField] private Image playedGameImage;

        private readonly List<string> scores = new();

        private PlayedGamesData _data;

        private void Awake()
        {
            scores.Add("☆☆☆☆☆");
            scores.Add("★☆☆☆☆");
            scores.Add("★★☆☆☆");
            scores.Add("★★★☆☆");
            scores.Add("★★★★☆");
            scores.Add("★★★★★");
        }

        public void Setup(PlayedGamesData data, PlayedGamesPrefabManager manager)
        {
            if (data == null) return;

            _data = data;

            playedGameIDTitleTxt.text = data.id.ToString("D3");
            playedGameIDTitleShadowTxt.text = data.id.ToString("D3");

            playedGameTitleTxt.text = data.title;
            playedGameTitleShadowTxt.text = data.title;

            originalityScoreTxt.text = ReturnScore(data.score[0]);
            originalityScoreShadowTxt.text = ReturnScore(data.score[0]);

            developSoulScoreTxt.text = ReturnScore(data.score[1]);
            developSoulScoreShadowTxt.text = ReturnScore(data.score[1]);

            uniquenessScoreTxt.text = ReturnScore(data.score[2]);
            uniquenessScoreShadowTxt.text = ReturnScore(data.score[2]);

            atmosphereScoreTxt.text = ReturnScore(data.score[3]);
            atmosphereScoreShadowTxt.text = ReturnScore(data.score[3]);

            immersionScoreTxt.text = ReturnScore(data.score[4]);
            immersionScoreShadowTxt.text = ReturnScore(data.score[4]);

            memorableScoreTxt.text = ReturnScore(data.score[5]);
            memorableScoreShadowTxt.text = ReturnScore(data.score[5]);

            descriptionTxt.text = data.description;
            playTimeTxt.text = $"{data.playTime}h";

            LoadProjectImage(data.imageSource, manager);
        }

        private string ReturnScore(int score)
        {
            return scores[score];
        }

        private void LoadProjectImage(string imageSource, PlayedGamesPrefabManager manager)
        {
            if (string.IsNullOrWhiteSpace(imageSource) || playedGameImage == null) return;

            var isLocalMode = GameSettingManager.GetDeviceMode();

            Action<Texture2D> onImageLoaded = tex =>
            {
                if (_data == null || _data.imageSource != imageSource) return;

                if (tex != null)
                {
                    var newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.5f, 0.5f));
                    playedGameImage.sprite = newSprite;
                }
            };

            if (isLocalMode)
                manager.StartCoroutine(manager.LoadImageFromLocal(imageSource, onImageLoaded));
            else
                manager.StartCoroutine(manager.LoadImageFromServer(imageSource, onImageLoaded));
        }

        private enum Scores
        {
            Originality,
            DevelopSoul,
            Uniqueness,
            Atmosphere,
            Immersion,
            Memorable
        }
    }
}