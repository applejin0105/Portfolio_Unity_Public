using System.Collections.Generic;
using Components.Effects.UI;
using Components.Effects.UI.Types;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scenes.Main.Controller
{
    public class ClickableDecoController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite[] sprites;

        [SerializeField] private UIGlitchFadePuzzleEffect thisEffect;
        [SerializeField] private Image thisImage;
        private LinkedListNode<Sprite> _currentNode;

        private LinkedList<Sprite> _spritesList;

        private void Awake()
        {
            _spritesList = new LinkedList<Sprite>();
        }

        private void Start()
        {
            foreach (var sprite in sprites)
                _spritesList.AddLast(sprite);
            _currentNode = _spritesList.First;
            thisImage.sprite = _currentNode.Value;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            thisEffect.Play();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            thisEffect.Stop();
            thisEffect.StopSound();
            _currentNode = _currentNode.Next ?? _spritesList.First;
            thisImage.sprite = _currentNode.Value;
        }
    }
}