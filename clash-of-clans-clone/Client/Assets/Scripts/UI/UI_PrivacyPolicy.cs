namespace DevelopersHub.ClashOfWhatecer
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class UI_PrivacyPolicy : MonoBehaviour
    {

        [SerializeField] private GameObject _elements = null;
        [SerializeField] private Button _closeButton = null;

        private bool _active = false; public bool isActive { get { return _active; } }
        private static UI_PrivacyPolicy _instance = null; public static UI_PrivacyPolicy instance { get { return _instance; } }
        
        private void Awake()
        {
            _instance = this;
            _elements.SetActive(false);
        }

        private void Start()
        {
            _closeButton.onClick.AddListener(Close);
        }

        public void Open()
        {
            transform.SetAsLastSibling();
            _active = true;
            _elements.SetActive(true);
        }

        public void Close()
        {
            SoundManager.instance.PlaySound(SoundManager.instance.buttonClickSound);
            _active = false;
            _elements.SetActive(false);
        }

    }
}