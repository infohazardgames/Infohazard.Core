// This file is part of the Infohazard.Core package.
// Copyright (c) 2022-present Vincent Miller (Infohazard Games).

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Infohazard.Core {
    /// <summary>
    /// Used to create health bars and other types of progress bars without using a Slider.
    /// </summary>
    /// <remarks>
    /// It supports images that fill the bar using either the “filled” image type or by manipulating the RectTransform anchors.
    /// </remarks>
    public class ProgressBar : MonoBehaviour {
        [SerializeField]
        [Tooltip("Image that will be used as the bar fill.")]
        private Image _fillImage;

        [SerializeField]
        [Range(0, 1)]
        [Tooltip("By what value to fill the bar. A value of zero means empty, one means full.")]
        private float _fillAmount = 0.5f;

        [SerializeField]
        [Tooltip("An optional text label to show the progress percentage on.")]
        private TMP_Text _percentText;

        private RectTransform _fillRect;

        /// <summary>
        /// By what value to fill the bar.
        /// </summary>
        /// <remarks>
        /// A value of zero means empty, one means full.
        /// </remarks>
        public float FillAmount {
            get => _fillAmount;
            set {
                if (_fillAmount == value) return;
                _fillAmount = Mathf.Clamp01(value);
                UpdateUI();
            }
        }

        private void UpdateUI() {
            if (_fillImage == null) return;
            if (_fillRect == null || _fillRect != _fillImage.transform) {
                _fillRect = _fillImage.GetComponent<RectTransform>();
            }

            if (!_fillRect) return;

            if (_fillImage.type == Image.Type.Filled) {
                _fillImage.fillAmount = _fillAmount;
                _fillRect.anchorMax = new Vector2(1, 1);
            } else {
                _fillImage.fillAmount = 1;
                _fillRect.anchorMax = new Vector2(_fillAmount, 1);
            }

            if (_percentText) _percentText.text = $"{Mathf.RoundToInt(_fillAmount * 100)}%";
        }

        private void OnValidate() {
            UpdateUI();
        }
    }
}
