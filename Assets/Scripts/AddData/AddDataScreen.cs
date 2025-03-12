using System;
using AddTask;
using Bitsplash.DatePicker;
using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AddData
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class AddDataScreen : MonoBehaviour
    {
        [SerializeField] private Color _selectedExpensesButtonColor;
        [SerializeField] private Color _defaultExpensesButtonColor;

        [SerializeField] private Color _defaultInputColor;
        [SerializeField] private Color _filledInputColor;

        [SerializeField] private Button _backButton;

        [SerializeField] private Button _expensesButton;
        [SerializeField] private Button _incomeButton;

        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_InputField _costInput;
        [SerializeField] private Image _nameImage;
        [SerializeField] private Image _costImage;

        [SerializeField] private Button _dateButton;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private GameObject _datePlane;
        [SerializeField] private Button _saveDateButton;
        [SerializeField] private DatePickerSettings _datePickerSettings;

        [SerializeField] private Button _timeButton;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private GameObject _timePlane;
        [SerializeField] private Button _saveTimeButton;
        [SerializeField] private TimeSelector _timeSelector;

        [SerializeField] private PhotosController _photosController;

        [SerializeField] private Button _saveButton;

        [Header("Animation Settings")] [SerializeField]
        private float _fadeInDuration = 0.3f;

        [SerializeField] private float _fadeOutDuration = 0.2f;
        [SerializeField] private float _buttonScaleDuration = 0.1f;
        [SerializeField] private float _buttonClickScale = 0.95f;
        [SerializeField] private float _panelAnimationDuration = 0.25f;
        [SerializeField] private Ease _fadeInEase = Ease.OutQuad;
        [SerializeField] private Ease _fadeOutEase = Ease.InQuad;
        [SerializeField] private Ease _buttonAnimationEase = Ease.OutBack;
        [SerializeField] private Ease _panelAnimationEase = Ease.OutCubic;

        private ExpenseType _chosenType;
        private ScreenVisabilityHandler _screenVisabilityHandler;
        private DateTime _selectedDate = DateTime.Now;
        private string _selectedHour = "12";
        private string _selectedMinute = "00";
        private string _selectedAmPm = "AM";
        private bool _isDatePanelOpen = false;
        private bool _isTimePanelOpen = false;
        private bool _isUpdatingCostText = false;
        private bool _isAnimating = false;

        private Sequence _screenFadeSequence;
        private Sequence _datePanelSequence;
        private Sequence _timePanelSequence;

        private Vector3 _dateButtonOriginalScale;
        private Vector3 _timeButtonOriginalScale;
        private Vector3 _backButtonOriginalScale;
        private Vector3 _saveButtonOriginalScale;
        private Vector3 _expensesButtonOriginalScale;
        private Vector3 _incomeButtonOriginalScale;
        private Vector3 _saveDateButtonOriginalScale;
        private Vector3 _saveTimeButtonOriginalScale;

        private CanvasGroup _datePlaneCanvasGroup;
        private CanvasGroup _timePlaneCanvasGroup;

        public event Action BackClicked;
        public event Action<Data.Data> DataSaved;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();

            _dateButtonOriginalScale = _dateButton.transform.localScale;
            _timeButtonOriginalScale = _timeButton.transform.localScale;
            _backButtonOriginalScale = _backButton.transform.localScale;
            _saveButtonOriginalScale = _saveButton.transform.localScale;
            _expensesButtonOriginalScale = _expensesButton.transform.localScale;
            _incomeButtonOriginalScale = _incomeButton.transform.localScale;
            _saveDateButtonOriginalScale = _saveDateButton.transform.localScale;
            _saveTimeButtonOriginalScale = _saveTimeButton.transform.localScale;

            _datePlaneCanvasGroup = _datePlane.GetComponent<CanvasGroup>();
            if (_datePlaneCanvasGroup == null)
            {
                _datePlaneCanvasGroup = _datePlane.AddComponent<CanvasGroup>();
            }

            _timePlaneCanvasGroup = _timePlane.GetComponent<CanvasGroup>();
            if (_timePlaneCanvasGroup == null)
            {
                _timePlaneCanvasGroup = _timePlane.AddComponent<CanvasGroup>();
            }
        }

        private void OnEnable()
        {
            SetupButtonWithAnimation(_backButton, OnBackButtonClicked);
            SetupButtonWithAnimation(_expensesButton, OnExpensesButtonClicked);
            SetupButtonWithAnimation(_incomeButton, OnIncomeButtonClicked);
            SetupButtonWithAnimation(_dateButton, OnDateButtonClicked);
            SetupButtonWithAnimation(_saveDateButton, OnSaveDateButtonClicked);
            SetupButtonWithAnimation(_timeButton, OnTimeButtonClicked);
            SetupButtonWithAnimation(_saveTimeButton, OnSaveTimeButtonClicked);
            SetupButtonWithAnimation(_saveButton, OnSaveButtonClicked);

            _nameInput.onValueChanged.AddListener(OnNameInputChanged);
            _costInput.onValueChanged.AddListener(OnCostInputChanged);

            _timeSelector.HourInputed += OnHourSelected;
            _timeSelector.MinuteInputed += OnMinuteSelected;
            _timeSelector.AmPmInputed += OnAmPmSelected;
        }

        private void OnDisable()
        {
            RemoveButtonAnimation(_backButton, OnBackButtonClicked);
            RemoveButtonAnimation(_expensesButton, OnExpensesButtonClicked);
            RemoveButtonAnimation(_incomeButton, OnIncomeButtonClicked);
            RemoveButtonAnimation(_dateButton, OnDateButtonClicked);
            RemoveButtonAnimation(_saveDateButton, OnSaveDateButtonClicked);
            RemoveButtonAnimation(_timeButton, OnTimeButtonClicked);
            RemoveButtonAnimation(_saveTimeButton, OnSaveTimeButtonClicked);
            RemoveButtonAnimation(_saveButton, OnSaveButtonClicked);

            _nameInput.onValueChanged.RemoveListener(OnNameInputChanged);
            _costInput.onValueChanged.RemoveListener(OnCostInputChanged);

            _timeSelector.HourInputed -= OnHourSelected;
            _timeSelector.MinuteInputed -= OnMinuteSelected;
            _timeSelector.AmPmInputed -= OnAmPmSelected;

            KillAllAnimations();
        }

        private void KillAllAnimations()
        {
            _screenFadeSequence?.Kill();
            _datePanelSequence?.Kill();
            _timePanelSequence?.Kill();

            _isDatePanelOpen = false;
            _isTimePanelOpen = false;
            _isAnimating = false;

            _datePlane.SetActive(false);
            _timePlane.SetActive(false);

            if (_datePlaneCanvasGroup != null)
            {
                _datePlaneCanvasGroup.alpha = 0;
                _datePlaneCanvasGroup.interactable = false;
                _datePlaneCanvasGroup.blocksRaycasts = false;
            }

            if (_timePlaneCanvasGroup != null)
            {
                _timePlaneCanvasGroup.alpha = 0;
                _timePlaneCanvasGroup.interactable = false;
                _timePlaneCanvasGroup.blocksRaycasts = false;
            }
        }

        private void Start()
        {
            InitializeUI();
            ResetInputs();
            _screenVisabilityHandler.DisableScreen();
        }

        private void SetupButtonWithAnimation(Button button, UnityEngine.Events.UnityAction onClick)
        {
            UnityEngine.Events.UnityAction originalCallback = onClick;

            button.onClick.RemoveListener(onClick);

            button.onClick.AddListener(() =>
            {
                if (_isAnimating && !(button == _saveDateButton || button == _saveTimeButton))
                {
                    return;
                }

                button.transform.DOScale(_buttonClickScale * button.transform.localScale, _buttonScaleDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        button.transform.DOScale(button.transform.localScale / _buttonClickScale, _buttonScaleDuration)
                            .SetEase(_buttonAnimationEase)
                            .OnComplete(() => { originalCallback?.Invoke(); });
                    });
            });
        }

        private void RemoveButtonAnimation(Button button, UnityEngine.Events.UnityAction onClick)
        {
            button.onClick.RemoveAllListeners();
        }

        public void Enable()
        {
            ResetInputs();
            _screenFadeSequence?.Kill();
            _isAnimating = true;

            _screenFadeSequence = DOTween.Sequence();

            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 1;

            _screenFadeSequence.Append(canvasGroup.DOFade(1, _fadeInDuration).SetEase(_fadeInEase));
            _screenFadeSequence.Join(_backButton.transform.DOScale(_backButtonOriginalScale, _fadeInDuration)
                .From(Vector3.zero).SetEase(_fadeInEase));
            _screenFadeSequence.Join(_saveButton.transform.DOScale(_saveButtonOriginalScale, _fadeInDuration)
                .From(Vector3.zero).SetEase(_fadeInEase));

            _screenFadeSequence.Join(_nameImage.transform.DOScale(1, _fadeInDuration).From(0.8f).SetEase(_fadeInEase));
            _screenFadeSequence.Join(_costImage.transform.DOScale(1, _fadeInDuration).From(0.8f).SetEase(_fadeInEase)
                .SetDelay(0.05f));

            _screenFadeSequence.Join(_expensesButton.transform.DOScale(_expensesButtonOriginalScale, _fadeInDuration)
                .From(Vector3.zero).SetEase(_fadeInEase).SetDelay(0.1f));
            _screenFadeSequence.Join(_incomeButton.transform.DOScale(_incomeButtonOriginalScale, _fadeInDuration)
                .From(Vector3.zero).SetEase(_fadeInEase).SetDelay(0.15f));
            _screenFadeSequence.Join(_dateButton.transform.DOScale(_dateButtonOriginalScale, _fadeInDuration)
                .From(Vector3.zero).SetEase(_fadeInEase).SetDelay(0.2f));
            _screenFadeSequence.Join(_timeButton.transform.DOScale(_timeButtonOriginalScale, _fadeInDuration)
                .From(Vector3.zero).SetEase(_fadeInEase).SetDelay(0.25f));

            _screenFadeSequence.Join(_photosController.transform.DOScale(1, _fadeInDuration).From(0.8f)
                .SetEase(_fadeInEase).SetDelay(0.3f));

            _screenFadeSequence.OnComplete(() =>
            {
                _screenVisabilityHandler.EnableScreen();
                _isAnimating = false;
            });
        }

        public void Disable()
        {
            _screenFadeSequence?.Kill();
            _isAnimating = true;

            _screenFadeSequence = DOTween.Sequence();

            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _screenFadeSequence.Append(canvasGroup.DOFade(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_backButton.transform.DOScale(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_saveButton.transform.DOScale(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_nameImage.transform.DOScale(0.8f, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_costImage.transform.DOScale(0.8f, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_expensesButton.transform.DOScale(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_incomeButton.transform.DOScale(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_dateButton.transform.DOScale(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_timeButton.transform.DOScale(0, _fadeOutDuration).SetEase(_fadeOutEase));
            _screenFadeSequence.Join(_photosController.transform.DOScale(0.8f, _fadeOutDuration).SetEase(_fadeOutEase));

            _screenFadeSequence.OnComplete(() =>
            {
                ResetInputs();
                _screenVisabilityHandler.DisableScreen();
                _isAnimating = false;
            });
        }

        private void InitializeUI()
        {
            _chosenType = ExpenseType.Expense;
            UpdateExpenseTypeButtonsUI();

            UpdateDateText();
            UpdateTimeText();

            _datePlane.SetActive(false);
            _timePlane.SetActive(false);

            if (_datePlaneCanvasGroup != null)
            {
                _datePlaneCanvasGroup.interactable = false;
                _datePlaneCanvasGroup.blocksRaycasts = false;
            }

            if (_timePlaneCanvasGroup != null)
            {
                _timePlaneCanvasGroup.interactable = false;
                _timePlaneCanvasGroup.blocksRaycasts = false;
            }

            UpdateSaveButtonState();
        }

        private void OnBackButtonClicked()
        {
            BackClicked?.Invoke();
        }

        private void OnExpensesButtonClicked()
        {
            _chosenType = ExpenseType.Expense;

            _expensesButton.image.DOColor(_selectedExpensesButtonColor, 0.2f).SetEase(Ease.OutQuad);
            _incomeButton.image.DOColor(_defaultExpensesButtonColor, 0.2f).SetEase(Ease.OutQuad);
        }

        private void OnIncomeButtonClicked()
        {
            _chosenType = ExpenseType.Income;

            _expensesButton.image.DOColor(_defaultExpensesButtonColor, 0.2f).SetEase(Ease.OutQuad);
            _incomeButton.image.DOColor(_selectedExpensesButtonColor, 0.2f).SetEase(Ease.OutQuad);
        }

        private void UpdateExpenseTypeButtonsUI()
        {
            if (_chosenType == ExpenseType.Expense)
            {
                _expensesButton.image.color = _selectedExpensesButtonColor;
                _incomeButton.image.color = _defaultExpensesButtonColor;
            }
            else
            {
                _expensesButton.image.color = _defaultExpensesButtonColor;
                _incomeButton.image.color = _selectedExpensesButtonColor;
            }
        }

        private void OnNameInputChanged(string value)
        {
            _nameImage.DOColor(
                !string.IsNullOrEmpty(value) ? _filledInputColor : _defaultInputColor,
                0.3f
            ).SetEase(Ease.OutQuad);

            UpdateSaveButtonState();
        }

        private void OnCostInputChanged(string value)
        {
            if (_isUpdatingCostText)
                return;

            _isUpdatingCostText = true;

            string cleanValue = value.Replace("$", "");

            if (!string.IsNullOrEmpty(cleanValue))
            {
                _costInput.text = "$" + cleanValue;
            }
            else
            {
                _costInput.text = "";
            }

            _costImage.DOColor(
                !string.IsNullOrEmpty(cleanValue) ? _filledInputColor : _defaultInputColor,
                0.3f
            ).SetEase(Ease.OutQuad);

            UpdateSaveButtonState();

            _isUpdatingCostText = false;
        }

        private void UpdateInputFieldColor(TMP_InputField inputField, bool isFilled)
        {
            if (inputField == _costInput)
            {
                _costImage.color = isFilled ? _filledInputColor : _defaultInputColor;
            }
            else if (inputField == _nameInput)
            {
                _nameImage.color = isFilled ? _filledInputColor : _defaultInputColor;
            }
        }

        private void OnDateButtonClicked()
        {
            if (_isAnimating)
                return;

            _isAnimating = true;
            _isDatePanelOpen = !_isDatePanelOpen;

            _datePanelSequence?.Kill();

            _datePanelSequence = DOTween.Sequence();

            if (_isDatePanelOpen)
            {
                _datePlane.SetActive(true);

                _datePlaneCanvasGroup.alpha = 0;
                _datePlaneCanvasGroup.interactable = true;
                _datePlaneCanvasGroup.blocksRaycasts = true;

                _datePanelSequence.Append(_datePlaneCanvasGroup.DOFade(1, _panelAnimationDuration)
                    .SetEase(_panelAnimationEase));
                _datePanelSequence.Join(_datePlane.transform.DOScale(1, _panelAnimationDuration).From(0.9f)
                    .SetEase(_panelAnimationEase));

                if (_isTimePanelOpen)
                {
                    _isTimePanelOpen = false;
                    CloseTimePanelWithAnimation();
                }

                _datePanelSequence.OnComplete(() => { _isAnimating = false; });
            }
            else
            {
                CloseDatePanelWithAnimation();
            }
        }

        private void CloseDatePanelWithAnimation()
        {
            _datePlane.transform.DOScale(Vector3.zero, _buttonScaleDuration * 0.7f).SetEase(Ease.InBack)
                .OnComplete(() => _datePlane.SetActive(false));
        }

        private void OnSaveDateButtonClicked()
        {
            var selection = _datePickerSettings.Content.Selection;
            _selectedDate = selection.GetItem(0);

            _saveDateButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 10, 1)
                .OnComplete(() =>
                {
                    _isDatePanelOpen = false;
                    CloseDatePanelWithAnimation();

                    _dateText.transform.DOScale(1.1f, 0.2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            UpdateDateText();
                            _dateText.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
                        });
                });
        }

        private void UpdateDateText()
        {
            _dateText.text = _selectedDate.ToString("MMM dd, yyyy");
        }

        private void OnTimeButtonClicked()
        {
            if (_isAnimating)
                return;

            _isAnimating = true;
            _isTimePanelOpen = !_isTimePanelOpen;

            _timePanelSequence?.Kill();

            _timePanelSequence = DOTween.Sequence();

            if (_isTimePanelOpen)
            {
                _timePlane.SetActive(true);

                _timePlaneCanvasGroup.alpha = 0;
                _timePlaneCanvasGroup.interactable = true;
                _timePlaneCanvasGroup.blocksRaycasts = true;

                _timePanelSequence.Append(_timePlaneCanvasGroup.DOFade(1, _panelAnimationDuration)
                    .SetEase(_panelAnimationEase));
                _timePanelSequence.Join(_timePlane.transform.DOScale(1, _panelAnimationDuration).From(0.9f)
                    .SetEase(_panelAnimationEase));

                _timeSelector.Enable();

                if (_isDatePanelOpen)
                {
                    _isDatePanelOpen = false;
                    CloseDatePanelWithAnimation();
                }

                _timePanelSequence.OnComplete(() => { _isAnimating = false; });
            }
            else
            {
                CloseTimePanelWithAnimation();
                _timeSelector.Disable();
            }
        }

        private void CloseTimePanelWithAnimation()
        {
            _timePlane.transform.DOScale(Vector3.zero, _buttonScaleDuration * 0.7f).SetEase(Ease.InBack)
                .OnComplete(() => _timePlane.SetActive(false));
        }

        private void OnSaveTimeButtonClicked()
        {
            _saveTimeButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 10, 1)
                .OnComplete(() =>
                {
                    CloseTimePanelWithAnimation();
                    _timeSelector.Disable();

                    _timeText.transform.DOScale(1.1f, 0.2f)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            UpdateTimeText();
                            _timeText.transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack);
                        });
                });
        }

        private void OnHourSelected(string hour)
        {
            _selectedHour = hour;
        }

        private void OnMinuteSelected(string minute)
        {
            _selectedMinute = minute;
        }

        private void OnAmPmSelected(string amPm)
        {
            _selectedAmPm = amPm;
        }

        private void UpdateTimeText()
        {
            _timeText.text = $"{_selectedHour}:{_selectedMinute} {_selectedAmPm}";
        }

        private void OnSaveButtonClicked()
        {
            if (CanSaveData())
            {
                _saveButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 10, 1)
                    .OnComplete(() => { SaveData(); });
            }
        }

        private bool CanSaveData()
        {
            return !string.IsNullOrEmpty(_nameInput.text) &&
                   !string.IsNullOrEmpty(_costInput.text);
        }

        private void UpdateSaveButtonState()
        {
            bool canSave = CanSaveData();

            if (_saveButton.interactable != canSave)
            {
                _saveButton.interactable = canSave;

                if (canSave)
                {
                    _saveButton.transform.DOScale(_saveButtonOriginalScale, 0.3f)
                        .SetEase(Ease.OutBack);
                }
                else
                {
                    _saveButton.transform.DOScale(_saveButtonOriginalScale * 0.95f, 0.3f)
                        .SetEase(Ease.OutQuad);
                }
            }
        }

        private void SaveData()
        {
            string costText = _costInput.text.Replace("$", "").Trim();

            if (!int.TryParse(costText, out int cost))
            {
                return;
            }

            if (_chosenType == ExpenseType.Expense)
            {
                cost = -Math.Abs(cost);
            }
            else
            {
                cost = Math.Abs(cost);
            }

            DateTime date = _selectedDate;

            string time = $"{_selectedHour}:{_selectedMinute} {_selectedAmPm}";

            byte[] photo = _photosController.GetPhoto();

            Data.Data newData = new Data.Data(
                cost,
                date,
                _nameInput.text,
                time,
                photo
            );

            DataSaved?.Invoke(newData);

            ResetInputs();
            Disable();
        }

        private void ResetInputs()
        {
            _nameInput.text = string.Empty;
            _costInput.text = string.Empty;

            _chosenType = ExpenseType.Expense;
            UpdateExpenseTypeButtonsUI();

            _selectedDate = DateTime.Now;
            UpdateDateText();

            _selectedHour = "12";
            _selectedMinute = "00";
            _selectedAmPm = "AM";
            UpdateTimeText();

            _photosController.ResetPhotos();

            KillAllAnimations();
        }
    }
}