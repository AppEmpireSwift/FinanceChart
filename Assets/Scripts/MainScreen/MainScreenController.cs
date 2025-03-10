using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainScreen
{
    [RequireComponent(typeof(ScreenVisabilityHandler))]
    public class MainScreenController : MonoBehaviour
    {
        [SerializeField] private Button _dateButton;
        [SerializeField] private TMP_Text _dateText;
        [SerializeField] private TMP_Text _budgetInput;
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

        private ScreenVisabilityHandler _screenVisabilityHandler;

        private void Awake()
        {
            _screenVisabilityHandler = GetComponent<ScreenVisabilityHandler>();
        }

        public void Enable()
        {
            _screenVisabilityHandler.EnableScreen();
        }

        public void Disable()
        {
            _screenVisabilityHandler.DisableScreen();
        }

        private void ToggleEmptyPlane()
        {
            _emptyPlane.gameObject.SetActive(_dataPlanes.All(p => !p.IsActive));
        }

        private void ToggleEmptyPlaneText()
        {
            if (!string.IsNullOrEmpty(_budgetInput.text))
            {
                _addDataButton.interactable = true;
                _emptyBudgetText.gameObject.SetActive(false);
                _emptyEntriesText.gameObject.SetActive(true);
            }
        }
    }
}