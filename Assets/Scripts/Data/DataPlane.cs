using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Data
{
    public class DataPlane : MonoBehaviour
    {
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _deleteButton;

        public event Action<Data> OpenClicked;
        public event Action<Data> DeleteClicked;

        public bool IsActive { get; private set; }
        public Data Data { get; private set; }

        private void OnEnable()
        {
            _openButton.onClick.AddListener(OnOpenClicked);
            _deleteButton.onClick.AddListener(OnDeleteClicked);
        }

        private void OnDisable()
        {
            _openButton.onClick.RemoveListener(OnOpenClicked);
            _deleteButton.onClick.RemoveListener(OnDeleteClicked);
        }

        public void Enable(Data data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            IsActive = true;
            gameObject.SetActive(IsActive);

            Data = data;
            _nameText.text = Data.Name;
            _costText.text = Data.Cost.ToString("N0");
        }

        public void Disable()
        {
            IsActive = false;
            gameObject.SetActive(IsActive);
        }

        public void OnReset()
        {
            _nameText.text = string.Empty;
            _costText.text = string.Empty;
            Data = null;
        }

        private void OnOpenClicked()
        {
            OpenClicked?.Invoke(Data);
        }

        private void OnDeleteClicked()
        {
            Disable();
            DeleteClicked?.Invoke(Data);
        }
    }
}