using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using AddData;
using Bitsplash.DatePicker;
using Data;
using OpenData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using SaveSystem;

namespace MainScreen
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class MainScreenController : MonoBehaviour
    {
        [SerializeField] private Button _dateButton;
        [SerializeField] private GameObject _datePlane;
        [SerializeField] private DatePickerSettings _datePickerSettings;

        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private TMP_InputField _budgetInput;
        [SerializeField] private TMP_Text _remainderText;
        [SerializeField] private TMP_Text _totalIncome;
        [SerializeField] private TMP_Text _totalExpenses;

        [SerializeField] private Button _expensesButton;
        [SerializeField] private Button _incomeButton;

        [SerializeField] private Color _selectedExpensesButtonColor;
        [SerializeField] private Color _defaultExpensesButtonColor;

        [SerializeField] private List<DataPlane> _dataPlanes;

        [SerializeField] private Button _addDataButton;

        [SerializeField] private GameObject _emptyPlane;
        [SerializeField] private TMP_Text _emptyBudgetText;
        [SerializeField] private TMP_Text _emptyEntriesText;

        [SerializeField] private AddDataScreen _addDataScreen;
        [SerializeField] private OpenDataScreen _openDataScreen;

        [Header("DoTween Animation Settings")] [SerializeField]
        private float _fadeInDuration = 0.3f;

        [SerializeField] private float _moveInDuration = 0.5f;
        [SerializeField] private float _scaleUpDuration = 0.4f;
        [SerializeField] private Ease _animationEase = Ease.OutBack;

        private ScreenVisabilityHandler _screenVisabilityHandler;
        private ExpenseType _currentExpenseType = ExpenseType.Expense;
        private List<Data.Data> _allData = new List<Data.Data>();
        private int _budget = 0;
        private DateTime _currentDate = DateTime.Now;
        private bool _isUpdatingBudget = false;
        private DataSaver _dataSaver;

        public event Action AddDataClicked;
        public event Action<Data.Data> DataRemoved;

        private void Awake()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
            _dataSaver = new DataSaver();

            DOTween.SetTweensCapacity(500, 50);
        }

        private void OnEnable()
        {
            _dateButton.onClick.AddListener(OnDateButtonClicked);
            _expensesButton.onClick.AddListener(OnExpensesButtonClicked);
            _incomeButton.onClick.AddListener(OnIncomeButtonClicked);
            _addDataButton.onClick.AddListener(OnAddDataButtonClicked);
            _budgetInput.onValueChanged.AddListener(OnBudgetChanged);
            _budgetInput.onEndEdit.AddListener(OnBudgetEditEnd);
            _addDataScreen.DataSaved += AddData;
            _addDataScreen.BackClicked += Enable;
            _openDataScreen.BackClicked += Enable;

            _datePickerSettings.Content.OnSelectionChanged.AddListener(OnDateSelected);

            foreach (var dataPlane in _dataPlanes)
            {
                dataPlane.DeleteClicked += OnDataRemoved;
                dataPlane.OpenClicked += OpenTask;
            }
        }

        private void OnDisable()
        {
            _dateButton.onClick.RemoveListener(OnDateButtonClicked);
            _expensesButton.onClick.RemoveListener(OnExpensesButtonClicked);
            _incomeButton.onClick.RemoveListener(OnIncomeButtonClicked);
            _addDataButton.onClick.RemoveListener(OnAddDataButtonClicked);
            _budgetInput.onValueChanged.RemoveListener(OnBudgetChanged);
            _budgetInput.onEndEdit.RemoveListener(OnBudgetEditEnd);
            _addDataScreen.DataSaved -= AddData;
            _addDataScreen.BackClicked -= Enable;
            _openDataScreen.BackClicked -= Enable;

            _datePickerSettings.Content.OnSelectionChanged.RemoveListener(OnDateSelected);

            foreach (var dataPlane in _dataPlanes)
            {
                dataPlane.DeleteClicked -= OnDataRemoved;
                dataPlane.OpenClicked -= OpenTask;
            }
        }

        private void Start()
        {
            LoadData();
            InitializeUI();
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
            AnimateScreenIn();
        }

        public void Disable()
        {
            AnimateScreenOut(() => _screenVisabilityHandler.DisableScreen());
        }

        public void CloseDatePlane()
        {
            _datePlane.transform.DOScale(Vector3.zero, _scaleUpDuration * 0.7f).SetEase(Ease.InBack)
                .OnComplete(() => _datePlane.SetActive(false));
        }

        public void SetBudget(int budget)
        {
            _isUpdatingBudget = true;
            _budget = budget;
            _budgetInput.text = budget.ToString("N0");
            _isUpdatingBudget = false;
            UpdateUIState();
            SaveData();
        }

        public void AddData(Data.Data data)
        {
            Enable();
            _allData.Add(data);
            UpdateDataDisplay();
            SaveData();
        }

        private void OpenTask(Data.Data data)
        {
            _openDataScreen.Enable(data);
            Disable();
        }

        private void InitializeUI()
        {
            UpdateDateText();

            _currentExpenseType = ExpenseType.Expense;
            UpdateExpenseTypeButtonsUI();

            _addDataButton.interactable = _budget > 0;
            ToggleEmptyPlane();
            ToggleEmptyPlaneText();

            UpdateDataDisplay();

            if (_budget <= 0)
            {
                _budgetInput.text = string.Empty;
                return;
            }

            _budgetInput.text = _budget.ToString("N0");
        }

        private void OnBudgetChanged(string value)
        {
            if (_isUpdatingBudget) return;

            _isUpdatingBudget = true;

            string cleanValue = value.Replace(",", "");

            if (int.TryParse(cleanValue, out int newBudget))
            {
                _budget = newBudget;

                if (!string.IsNullOrEmpty(value))
                {
                    _budgetInput.text = newBudget.ToString("N0");
                }
            }
            else if (string.IsNullOrEmpty(value))
            {
                _budget = 0;
            }

            _isUpdatingBudget = false;
            UpdateUIState();
            SaveData();
        }

        private void OnBudgetEditEnd(string value)
        {
            if (!_isUpdatingBudget)
            {
                _isUpdatingBudget = true;

                string cleanValue = value.Replace(",", "");

                if (string.IsNullOrEmpty(cleanValue))
                {
                    _budgetInput.text = "$0";
                    _budget = 0;
                }
                else if (int.TryParse(cleanValue, out int newBudget))
                {
                    _budget = newBudget;
                    _budgetInput.text = "$" + newBudget.ToString("N0");
                }

                _isUpdatingBudget = false;
                UpdateUIState();
                SaveData();
            }
        }

        private void OnDateButtonClicked()
        {
            _datePlane.transform.localScale = Vector3.zero;
            _datePlane.SetActive(true);
            _datePlane.transform.DOScale(Vector3.one, _scaleUpDuration).SetEase(_animationEase);

            UpdateDateText();
            UpdateDataDisplay();
            SaveData();
        }

        private void OnDateSelected()
        {
            var selection = _datePickerSettings.Content.Selection;

            _currentDate = selection.GetItem(0);

            CloseDatePlane();
            UpdateDateText();
            UpdateDataDisplay();
            SaveData();
        }

        private void UpdateDateText()
        {
            _dateText.text = _currentDate.ToString("MMMM yyyy");

            _dateText.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 2, 0.5f);
        }

        private void OnExpensesButtonClicked()
        {
            _currentExpenseType = ExpenseType.Expense;
            UpdateExpenseTypeButtonsUI();
            UpdateDataDisplay();
        }

        private void OnIncomeButtonClicked()
        {
            _currentExpenseType = ExpenseType.Income;
            UpdateExpenseTypeButtonsUI();
            UpdateDataDisplay();
        }

        private void UpdateExpenseTypeButtonsUI()
        {
            ColorBlock expensesColorBlock = _expensesButton.colors;
            ColorBlock incomeColorBlock = _incomeButton.colors;

            if (_currentExpenseType == ExpenseType.Expense)
            {
                expensesColorBlock.normalColor = _selectedExpensesButtonColor;
                incomeColorBlock.normalColor = _defaultExpensesButtonColor;
                _expensesButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 2, 0.5f);
            }
            else
            {
                expensesColorBlock.normalColor = _defaultExpensesButtonColor;
                incomeColorBlock.normalColor = _selectedExpensesButtonColor;
                _incomeButton.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 2, 0.5f);
            }

            _expensesButton.colors = expensesColorBlock;
            _incomeButton.colors = incomeColorBlock;
        }

        private void OnAddDataButtonClicked()
        {
            AddDataClicked?.Invoke();

            _addDataButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.3f, 2, 0.5f)
                .OnComplete(new TweenCallback(() =>
                {
                    _addDataScreen.Enable();
                    Disable();
                }));
        }

        private void OnDataRemoved(Data.Data data)
        {
            _allData.Remove(data);
            DataRemoved?.Invoke(data);
            UpdateDataDisplay();
            SaveData();
        }

        private void UpdateDataDisplay()
        {
            var filteredData = _allData.Where(d =>
                d.Date.Month == _currentDate.Month &&
                d.Date.Year == _currentDate.Year &&
                ((d.Cost < 0 && _currentExpenseType == ExpenseType.Expense) ||
                 (d.Cost > 0 && _currentExpenseType == ExpenseType.Income))
            ).ToList();

            foreach (var plane in _dataPlanes)
            {
                plane.Disable();
            }

            for (int i = 0; i < filteredData.Count && i < _dataPlanes.Count; i++)
            {
                _dataPlanes[i].Enable(filteredData[i]);
            }

            UpdateTotals();
            ToggleEmptyPlane();
        }

        private void UpdateTotals()
        {
            var currentMonthData = _allData.Where(d =>
                d.Date.Month == _currentDate.Month &&
                d.Date.Year == _currentDate.Year
            ).ToList();

            int totalExp = 0;
            int totalInc = 0;

            foreach (var data in currentMonthData)
            {
                if (data.Cost < 0)
                {
                    totalExp += Math.Abs(data.Cost);
                }
                else
                {
                    totalInc += data.Cost;
                }
            }

            DOTween.To(() => int.Parse(_totalExpenses.text.Replace("-$", "").Replace(",", "")),
                x => _totalExpenses.text = "-$" + x.ToString("N0"),
                totalExp, 1f).SetEase(Ease.OutQuad);

            DOTween.To(() => int.Parse(_totalIncome.text.Replace("+$", "").Replace(",", "")),
                x => _totalIncome.text = "+$" + x.ToString("N0"),
                totalInc, 1f).SetEase(Ease.OutQuad);

            int remainder = _budget + totalInc - totalExp;

            DOTween.To(() => int.Parse(_remainderText.text.Replace("$", "").Replace(",", "")),
                x => _remainderText.text = "$" + x.ToString("N0"),
                remainder, 1f).SetEase(Ease.OutQuad);
        }

        private void ToggleEmptyPlane()
        {
            bool allInactive = _dataPlanes.All(p => !p.IsActive);

            if (allInactive && !_emptyPlane.activeSelf)
            {
                _emptyPlane.SetActive(true);
                _emptyPlane.transform.localScale = Vector3.zero;
                _emptyPlane.transform.DOScale(Vector3.one, _scaleUpDuration).SetEase(_animationEase);

                ToggleEmptyPlaneText();
            }
            else if (!allInactive && _emptyPlane.activeSelf)
            {
                _emptyPlane.transform.DOScale(Vector3.zero, _scaleUpDuration * 0.7f).SetEase(Ease.InBack)
                    .OnComplete(new TweenCallback(() => _emptyPlane.SetActive(false)));
            }
        }

        private void ToggleEmptyPlaneText()
        {
            if (_budget > 0)
            {
                _addDataButton.interactable = true;

                if (_emptyBudgetText.gameObject.activeSelf)
                {
                    _emptyBudgetText.DOFade(0, _fadeInDuration * 0.5f)
                        .OnComplete(new TweenCallback(() => _emptyBudgetText.gameObject.SetActive(false)));

                    _emptyEntriesText.gameObject.SetActive(true);
                    _emptyEntriesText.alpha = 0;
                    _emptyEntriesText.DOFade(1, _fadeInDuration);
                }
            }
            else
            {
                _addDataButton.interactable = false;

                if (_emptyEntriesText.gameObject.activeSelf)
                {
                    _emptyEntriesText.DOFade(0, _fadeInDuration * 0.5f)
                        .OnComplete(new TweenCallback(() => _emptyEntriesText.gameObject.SetActive(false)));

                    _emptyBudgetText.gameObject.SetActive(true);
                    _emptyBudgetText.alpha = 0;
                    _emptyBudgetText.DOFade(1, _fadeInDuration);
                }
            }
        }

        private void UpdateUIState()
        {
            ToggleEmptyPlane();
            ToggleEmptyPlaneText();
            UpdateTotals();
        }

        private void SaveData()
        {
            try
            {
                PlayerData playerData = new PlayerData
                {
                    Budget = _budget,
                    CurrentDate = _currentDate,
                    Datas = _allData
                };

                _dataSaver.SaveData(_allData);

                PlayerPrefs.SetInt("Budget", _budget);
                PlayerPrefs.SetString("CurrentDate", _currentDate.ToString("o"));
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving data: {e.Message}");
            }
        }

        private void LoadData()
        {
            try
            {
                _allData = _dataSaver.LoadData() ?? new List<Data.Data>();

                if (PlayerPrefs.HasKey("Budget"))
                {
                    _budget = PlayerPrefs.GetInt("Budget");
                }

                if (PlayerPrefs.HasKey("CurrentDate"))
                {
                    try
                    {
                        _currentDate = DateTime.Parse(PlayerPrefs.GetString("CurrentDate"));
                    }
                    catch
                    {
                        _currentDate = DateTime.Now;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading data: {e.Message}");
                _allData = new List<Data.Data>();
                _budget = 0;
                _currentDate = DateTime.Now;
            }
        }

        private void AnimateScreenIn()
        {
            CanvasGroup[] canvasGroups = GetComponentsInChildren<CanvasGroup>(true);

            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = 0;
            }

            RectTransform headerRect = _dateText.transform.parent as RectTransform;
            if (headerRect != null)
            {
                headerRect.anchoredPosition = new Vector2(0, 50);
                headerRect.DOAnchorPosY(0, _moveInDuration).SetEase(_animationEase);

                CanvasGroup headerGroup = headerRect.GetComponent<CanvasGroup>();
                if (headerGroup != null)
                {
                    headerGroup.alpha = 0;
                    headerGroup.DOFade(1, _fadeInDuration);
                }
            }

            RectTransform statsRect = _remainderText.transform.parent as RectTransform;
            if (statsRect != null)
            {
                statsRect.anchoredPosition = new Vector2(0, -50);
                statsRect.DOAnchorPosY(0, _moveInDuration).SetEase(_animationEase).SetDelay(0.1f);

                CanvasGroup statsGroup = statsRect.GetComponent<CanvasGroup>();
                if (statsGroup != null)
                {
                    statsGroup.alpha = 0;
                    statsGroup.DOFade(1, _fadeInDuration).SetDelay(0.1f);
                }
            }

            for (int i = 0; i < canvasGroups.Length; i++)
            {
                if (canvasGroups[i].transform != headerRect && canvasGroups[i].transform != statsRect)
                {
                    canvasGroups[i].DOFade(1, _fadeInDuration).SetDelay(0.2f + (i * 0.05f));
                }
            }

            _addDataButton.transform.localScale = Vector3.zero;
            _addDataButton.transform.DOScale(1, _scaleUpDuration).SetEase(_animationEase).SetDelay(0.3f);
        }

        private void AnimateScreenOut(Action onComplete)
        {
            CanvasGroup[] canvasGroups = GetComponentsInChildren<CanvasGroup>(true);

            int animationCount = canvasGroups.Length;

            if (animationCount == 0)
            {
                onComplete?.Invoke();
                return;
            }

            var completionCounter = new CompletionTracker(animationCount, onComplete);

            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.DOFade(0, _fadeInDuration * 0.7f)
                    .OnComplete(() => completionCounter.IncrementAndCheckCompletion());
            }
        }

        private class CompletionTracker
        {
            private int _totalCount;
            private int _completedCount;
            private Action _onComplete;

            public CompletionTracker(int totalCount, Action onComplete)
            {
                _totalCount = totalCount;
                _completedCount = 0;
                _onComplete = onComplete;
            }

            public void IncrementAndCheckCompletion()
            {
                _completedCount++;
                if (_completedCount >= _totalCount)
                {
                    _onComplete?.Invoke();
                }
            }
        }
    }

    [Serializable]
    public class PlayerData
    {
        public int Budget;
        public DateTime CurrentDate;
        public List<Data.Data> Datas;
    }
}