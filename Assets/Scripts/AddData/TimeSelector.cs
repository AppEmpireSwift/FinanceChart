using System;
using DanielLochner.Assets.SimpleScrollSnap;
using TMPro;
using UnityEngine;

namespace AddTask
{
    public class TimeSelector : MonoBehaviour
    {
        [SerializeField] private Color _selectedColor;
        [SerializeField] private Color _unselectedColor;

        [SerializeField] private SimpleScrollSnap _hourScrollSnap;
        [SerializeField] private SimpleScrollSnap _minuteScrollSnap;
        [SerializeField] private SimpleScrollSnap _amPmScrollSnap;
        [SerializeField] private TMP_Text[] _hourText;
        [SerializeField] private TMP_Text[] _minuteText;
        [SerializeField] private TMP_Text[] _amPmText;

        private string _hour;
        private string _minute;
        private string _amPm;

        public event Action<string> HourInputed;
        public event Action<string> MinuteInputed;
        public event Action<string> AmPmInputed;

        private void OnEnable()
        {
            _hourScrollSnap.OnPanelCentered.AddListener(SetHour);
            _minuteScrollSnap.OnPanelCentered.AddListener(SetMinute);
            _amPmScrollSnap.OnPanelCentered.AddListener(SetAmPm);
        }

        private void OnDisable()
        {
            _hourScrollSnap.OnPanelCentered.RemoveListener(SetHour);
            _minuteScrollSnap.OnPanelCentered.RemoveListener(SetMinute);
            _amPmScrollSnap.OnPanelCentered.RemoveListener(SetAmPm);
        }

        private void Start()
        {
            InitializeTimeFields();
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            Reset();
            gameObject.SetActive(false);
        }

        private void SetHour(int start, int end)
        {
            _hour = _hourText[start].text;
            SetColorForSelected(_hourText, start);
            HourInputed?.Invoke(_hour);
        }

        private void SetMinute(int start, int end)
        {
            _minute = _minuteText[start].text;
            SetColorForSelected(_minuteText, start);
            MinuteInputed?.Invoke(_minute);
        }

        private void SetAmPm(int start, int end)
        {
            _amPm = _amPmText[start].text;
            SetColorForSelected(_amPmText, start);
            AmPmInputed?.Invoke(_amPm);
        }

        private void InitializeTimeFields()
        {
            PopulateHours();
            PopulateMinutes();
            PopulateAmPm();
            SetColorForSelected(_hourText, 0);
            SetColorForSelected(_minuteText, 0);
            SetColorForSelected(_amPmText, 0);
        }

        private void PopulateHours()
        {
            for (int i = 0; i < _hourText.Length; i++)
            {
                _hourText[i].text = i % 12 == 0 ? "12" : (i % 12).ToString("00");
            }
        }

        private void PopulateMinutes()
        {
            for (int i = 0; i < _minuteText.Length; i++)
            {
                _minuteText[i].text = i < 60 ? i.ToString("00") : "";
            }
        }

        private void PopulateAmPm()
        {
            if (_amPmText.Length >= 2)
            {
                _amPmText[0].text = "AM";
                _amPmText[1].text = "PM";
                
                for (int i = 2; i < _amPmText.Length; i++)
                {
                    _amPmText[i].text = "";
                }
            }
        }

        private void SetColorForSelected(TMP_Text[] texts, int selectedIndex)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].color = i == selectedIndex ? _selectedColor : _unselectedColor;
            }
        }

        private void Reset()
        {
            _hourScrollSnap.GoToPanel(0);
            _minuteScrollSnap.GoToPanel(0);
            _amPmScrollSnap.GoToPanel(0);

            _hour = string.Empty;
            _minute = string.Empty;
            _amPm = string.Empty;
        }
        
        public string GetTimeString()
        {
            if (string.IsNullOrEmpty(_hour) || string.IsNullOrEmpty(_minute) || string.IsNullOrEmpty(_amPm))
                return string.Empty;
                
            return $"{_hour}:{_minute} {_amPm}";
        }
    }
}