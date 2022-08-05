// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Infohazard.Core {
    public class ProgressBar : MonoBehaviour {
        [SerializeField] private Image _fillImage;
        [SerializeField] [Range(0, 1)] private float _fillAmount = 0.5f;
        [SerializeField] private TMP_Text _percentText;

        private RectTransform _fillRect;

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