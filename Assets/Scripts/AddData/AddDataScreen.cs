using System;
using AddTask;
using Bitsplash.DatePicker;
using Data;
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

        private ExpenseType _chosenType;
        private ScreenVisabilityHandler _screenVisabilityHandler;
        private DateTime _selectedDate = DateTime.Now;
        private string _selectedHour = "12";
        private string _selectedMinute = "00";
        private string _selectedAmPm = "AM";
        private bool _isDatePanelOpen = false;
        private bool _isTimePanelOpen = false;
        private bool _isUpdatingCostText = false;

        public event Action BackClicked;
        public event Action<Data.Data> DataSaved;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
        }

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _expensesButton.onClick.AddListener(OnExpensesButtonClicked);
            _incomeButton.onClick.AddListener(OnIncomeButtonClicked);

            _nameInput.onValueChanged.AddListener(OnNameInputChanged);
            _costInput.onValueChanged.AddListener(OnCostInputChanged);

            _dateButton.onClick.AddListener(OnDateButtonClicked);
            _saveDateButton.onClick.AddListener(OnSaveDateButtonClicked);

            _timeButton.onClick.AddListener(OnTimeButtonClicked);
            _saveTimeButton.onClick.AddListener(OnSaveTimeButtonClicked);

            _timeSelector.HourInputed += OnHourSelected;
            _timeSelector.MinuteInputed += OnMinuteSelected;
            _timeSelector.AmPmInputed += OnAmPmSelected;

            _saveButton.onClick.AddListener(OnSaveButtonClicked);
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _expensesButton.onClick.RemoveListener(OnExpensesButtonClicked);
            _incomeButton.onClick.RemoveListener(OnIncomeButtonClicked);

            _nameInput.onValueChanged.RemoveListener(OnNameInputChanged);
            _costInput.onValueChanged.RemoveListener(OnCostInputChanged);

            _dateButton.onClick.RemoveListener(OnDateButtonClicked);
            _saveDateButton.onClick.RemoveListener(OnSaveDateButtonClicked);

            _timeButton.onClick.RemoveListener(OnTimeButtonClicked);
            _saveTimeButton.onClick.RemoveListener(OnSaveTimeButtonClicked);

            _timeSelector.HourInputed -= OnHourSelected;
            _timeSelector.MinuteInputed -= OnMinuteSelected;
            _timeSelector.AmPmInputed -= OnAmPmSelected;

            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
        }

        private void Start()
        {
            InitializeUI();
            ResetInputs();
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
        }

        public void Disable()
        {
            ResetInputs();
            _screenVisabilityHandler.DisableScreen();
        }

        private void InitializeUI()
        {
            _chosenType = ExpenseType.Expense;
            UpdateExpenseTypeButtonsUI();

            UpdateDateText();
            UpdateTimeText();

            _datePlane.SetActive(false);
            _timePlane.SetActive(false);

            UpdateSaveButtonState();
        }

        private void OnBackButtonClicked()
        {
            BackClicked?.Invoke();
        }

        private void OnExpensesButtonClicked()
        {
            _chosenType = ExpenseType.Expense;
            UpdateExpenseTypeButtonsUI();
        }

        private void OnIncomeButtonClicked()
        {
            _chosenType = ExpenseType.Income;
            UpdateExpenseTypeButtonsUI();
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
            UpdateInputFieldColor(_nameInput, !string.IsNullOrEmpty(value));
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

            UpdateInputFieldColor(_costInput, !string.IsNullOrEmpty(cleanValue));
            UpdateSaveButtonState();

            _isUpdatingCostText = false;
        }

        private void UpdateInputFieldColor(TMP_InputField inputField, bool isFilled)
        {
            if (inputField == _costInput)
            {
                _costImage.color = isFilled ? _filledInputColor : _defaultInputColor;
            }
            else if(inputField == _nameInput)
            {
                _nameImage.color = isFilled ? _filledInputColor : _defaultInputColor;
            }
        }

        private void OnDateButtonClicked()
        {
            _isDatePanelOpen = !_isDatePanelOpen;
            _datePlane.SetActive(_isDatePanelOpen);

            if (_isDatePanelOpen && _isTimePanelOpen)
            {
                _isTimePanelOpen = false;
                _timePlane.SetActive(false);
            }
        }

        private void OnSaveDateButtonClicked()
        {
            var selection = _datePickerSettings.Content.Selection;

            _selectedDate = selection.GetItem(0);

            _isDatePanelOpen = false;
            _datePlane.SetActive(false);
            UpdateDateText();
        }

        private void UpdateDateText()
        {
            _dateText.text = _selectedDate.ToString("MMM dd, yyyy");
        }

        private void OnTimeButtonClicked()
        {
            _isTimePanelOpen = !_isTimePanelOpen;
            _timePlane.SetActive(_isTimePanelOpen);

            if (_isTimePanelOpen)
            {
                _timeSelector.Enable();

                if (_isDatePanelOpen)
                {
                    _isDatePanelOpen = false;
                    _datePlane.SetActive(false);
                }
            }
            else
            {
                _timeSelector.Disable();
            }
        }

        private void OnSaveTimeButtonClicked()
        {
            _isTimePanelOpen = false;
            _timePlane.SetActive(false);
            _timeSelector.Disable();
            UpdateTimeText();
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
                SaveData();
            }
        }

        private bool CanSaveData()
        {
            return !string.IsNullOrEmpty(_nameInput.text) &&
                   !string.IsNullOrEmpty(_costInput.text);
        }

        private void UpdateSaveButtonState()
        {
            _saveButton.interactable = CanSaveData();
        }

        private void SaveData()
        {
            if (!int.TryParse(_costInput.text, out int cost))
            {
                Debug.LogError("Не удалось преобразовать стоимость в число");
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

            _datePlane.SetActive(false);
            _timePlane.SetActive(false);
            _isDatePanelOpen = false;
            _isTimePanelOpen = false;
        }
    }
}