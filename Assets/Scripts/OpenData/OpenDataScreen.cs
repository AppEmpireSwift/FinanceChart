using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data;
using DG.Tweening;

namespace OpenData
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class OpenDataScreen : MonoBehaviour
    {
        [SerializeField] private Button _backButton;

        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;

        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private TMP_Text _timeText;

        [SerializeField] private PhotosController _photosController;

        [Header("DOTween Animation Settings")] [SerializeField]
        private float _animationDuration = 0.5f;

        [SerializeField] private Ease _animationEase = Ease.OutBack;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private Data.Data _currentData;

        private Vector3 _initialScale;
        private CanvasGroup _canvasGroup;

        public event Action BackClicked;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _initialScale = transform.localScale;

            _canvasGroup.alpha = 0;
            transform.localScale = _initialScale * 0.8f;
        }

        private void Start()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        public void Enable(Data.Data data)
        {
            _screenVisabilityHandler.EnableScreen();
            SetData(data);
            AnimateScreenIn();
        }

        public void Disable()
        {
            AnimateScreenOut(() => { _screenVisabilityHandler.DisableScreen(); });
        }

        private void AnimateScreenIn()
        {
            _canvasGroup.alpha = 0;
            transform.localScale = _initialScale * 0.8f;

            transform.DOScale(_initialScale, _animationDuration)
                .SetEase(_animationEase);

            _canvasGroup.DOFade(1f, _animationDuration)
                .SetEase(Ease.Linear);
        }

        private void AnimateScreenOut(Action onComplete = null)
        {
            transform.DOScale(_initialScale * 0.8f, _animationDuration)
                .SetEase(_animationEase);

            _canvasGroup.DOFade(0f, _animationDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => { onComplete?.Invoke(); });
        }

        private void SetData(Data.Data data)
        {
            _currentData = data;

            _nameText.text = data.Name;

            int absoluteCost = Math.Abs(data.Cost);
            _costText.text = $"${absoluteCost}";

            _dateText.text = data.Date.ToString("MMM dd, yyyy");

            _timeText.text = data.Time;

            _photosController.SetPhotos(data.Photo);
        }

        private void OnBackButtonClicked()
        {
            BackClicked?.Invoke();
            Disable();
        }
    }
}