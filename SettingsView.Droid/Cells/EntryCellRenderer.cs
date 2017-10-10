﻿using System;
using AiEntryCell = AiForms.Renderers.EntryCell;
using Android.Content;
using Xamarin.Forms;
using Android.Text;
using Android.Widget;
using AView = Android.Views.View;
using Android.Views.InputMethods;
using Java.Lang;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using Android.OS;
using Android.Content.Res;

[assembly: ExportRenderer(typeof(AiEntryCell), typeof(AiForms.Renderers.Droid.EntryCellRenderer))]
namespace AiForms.Renderers.Droid
{
    public class EntryCellRenderer:CellBaseRenderer<EntryCellView>{}

    public class EntryCellView:CellBaseView,ITextWatcher,
        TextView.IOnFocusChangeListener,TextView.IOnEditorActionListener
    {
        AiEntryCell _EntryCell => Cell as AiEntryCell;

        AiEditText _EditText;

        public EntryCellView(Context context,Cell cell):base(context,cell)
        {
            _EditText = new AiEditText(context);

            _EditText.Focusable = true;
            _EditText.ImeOptions = ImeAction.Done;
            _EditText.SetOnEditorActionListener(this);
            //_EditText.AddTextChangedListener(this);
            _EditText.OnFocusChangeListener = this;
            _EditText.SetSingleLine(true);
            _EditText.Gravity = GravityFlags.Right | GravityFlags.CenterVertical;

            _EditText.Background.Alpha = 0;  //下線は非表示

            _EditText.ClearFocusAction = DoneEdit;
            Click += EntryCellView_Click;

            var lparams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent) {

            };
            using (lparams)
            {
                ContentStack.AddView(_EditText, lparams);
            }
        }

        public override void UpdateCell()
        {
            UpdateValueText();
            UpdateValueTextColor();
            UpdateValueTextFontSize();
            UpdateKeyboard();
            UpdatePlaceholder();
            UpdateAccentColor();
            base.UpdateCell();
        }

        public override void CellPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.CellPropertyChanged(sender, e);
            if (e.PropertyName == AiEntryCell.ValueTextProperty.PropertyName)
            {
                UpdateValueText();
            }
            else if (e.PropertyName == AiEntryCell.ValueTextFontSizeProperty.PropertyName)
            {
                UpdateWithForceLayout(UpdateValueTextFontSize);
            }
            else if (e.PropertyName == AiEntryCell.ValueTextColorProperty.PropertyName)
            {
                UpdateWithForceLayout(UpdateValueTextColor);
            }
            else if (e.PropertyName == AiEntryCell.KeyboardProperty.PropertyName)
            {
                UpdateKeyboard();
            }
            else if (e.PropertyName == AiEntryCell.PlaceholderProperty.PropertyName)
            {
                UpdatePlaceholder();
            }
            else if(e.PropertyName == AiEntryCell.AccentColorProperty.PropertyName){
                UpdateAccentColor();
            }
        }

        public override void ParentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.ParentPropertyChanged(sender, e);
            if (e.PropertyName == SettingsView.CellValueTextColorProperty.PropertyName)
            {
                UpdateValueTextColor();
            }
            else if (e.PropertyName == SettingsView.CellValueTextFontSizeProperty.PropertyName)
            {
                UpdateWithForceLayout(UpdateValueTextFontSize);
            }
            else if( e.PropertyName == SettingsView.CellAccentColorProperty.PropertyName){
                UpdateAccentColor();
            }
        }


        protected override void Dispose(bool disposing)
        {
            if(disposing){
                Click -= EntryCellView_Click;
                _EditText.RemoveFromParent();
                _EditText.SetOnEditorActionListener(null);
                _EditText.RemoveTextChangedListener(this);
                _EditText.OnFocusChangeListener = null;
                _EditText.ClearFocusAction = null;
                _EditText.Dispose();
                _EditText = null;
            }
            base.Dispose(disposing);
        }

        void EntryCellView_Click(object sender, EventArgs e)
        {
            _EditText.RequestFocus();
            ShowKeyboard(_EditText);
        }

        void UpdateValueText()
        {
            _EditText.RemoveTextChangedListener(this);
            if (_EditText.Text != _EntryCell.ValueText)
            {
                _EditText.Text = _EntryCell.ValueText;
            }
            _EditText.AddTextChangedListener(this);
        }

        void UpdateValueTextFontSize()
        {
            if (_EntryCell.ValueTextFontSize > 0)
            {
                _EditText.SetTextSize(Android.Util.ComplexUnitType.Sp, (float)_EntryCell.ValueTextFontSize);
            }
            else if (CellParent != null)
            {
                _EditText.SetTextSize(Android.Util.ComplexUnitType.Sp, (float)CellParent.CellValueTextFontSize);
            }
        }

        void UpdateValueTextColor()
        {
            if (_EntryCell.ValueTextColor != Xamarin.Forms.Color.Default)
            {
                _EditText.SetTextColor(_EntryCell.ValueTextColor.ToAndroid());
            }
            else if (CellParent != null && CellParent.CellValueTextColor != Xamarin.Forms.Color.Default)
            {
                _EditText.SetTextColor(CellParent.CellValueTextColor.ToAndroid());
            }
        }

        void UpdateKeyboard()
        {
            _EditText.InputType = _EntryCell.Keyboard.ToInputType();
        }

        void UpdatePlaceholder()
        {
            _EditText.Hint = _EntryCell.Placeholder;
            _EditText.SetHintTextColor(Android.Graphics.Color.Rgb(210, 210, 210));
        }

        void UpdateAccentColor()
        {
            if (_EntryCell.AccentColor != Xamarin.Forms.Color.Default)
            {
                ChangeTextViewBack(_EntryCell.AccentColor.ToAndroid());
            }
            else if (CellParent != null && CellParent.CellAccentColor != Xamarin.Forms.Color.Default)
            {
                ChangeTextViewBack(CellParent.CellAccentColor.ToAndroid());
            }
        }

        void ChangeTextViewBack(Android.Graphics.Color accent)
        {
            var colorlist = new ColorStateList(new int[][]
            {
                new int[]{global::Android.Resource.Attribute.StateFocused},
                new int[]{-global::Android.Resource.Attribute.StateFocused},
            },
                new int[] {
                    Android.Graphics.Color.Argb(255,accent.R,accent.G,accent.B),
                    Android.Graphics.Color.Argb(255, 200, 200, 200)
            });
            _EditText.Background.SetTintList(colorlist);
        }


        bool TextView.IOnEditorActionListener.OnEditorAction(TextView v, ImeAction actionId, Android.Views.KeyEvent e)
        {
            if (actionId == ImeAction.Done ||
                    (actionId == ImeAction.ImeNull && e.KeyCode == Keycode.Enter))
            {
                HideKeyboard(v);
                DoneEdit();
            }

            return true;
        }

        void DoneEdit()
        {
            var entryCell = (IEntryCellController)Cell;
            entryCell.SendCompleted();
            _EditText.ClearFocus();
            ClearFocus();
        }

        void HideKeyboard(Android.Views.View inputView)
        {
            using (var inputMethodManager = (InputMethodManager)Forms.Context.GetSystemService(Context.InputMethodService))
            {
                IBinder windowToken = inputView.WindowToken;
                if (windowToken != null)
                    inputMethodManager.HideSoftInputFromWindow(windowToken, HideSoftInputFlags.None);
            }
        }
        void ShowKeyboard(Android.Views.View inputView)
        {
            using (var inputMethodManager = (InputMethodManager)Forms.Context.GetSystemService(Context.InputMethodService))
            {

                inputMethodManager.ShowSoftInput(inputView, ShowFlags.Forced);
                inputMethodManager.ToggleSoftInput(ShowFlags.Forced, HideSoftInputFlags.ImplicitOnly);

            }
        }

        void ITextWatcher.AfterTextChanged(IEditable s)
        {
        }

        void ITextWatcher.BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
        }

        void ITextWatcher.OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            _EntryCell.ValueText = s?.ToString();
        }

        void IOnFocusChangeListener.OnFocusChange(Android.Views.View v, bool hasFocus)
        {
            if (hasFocus)
            {
                //フォーカス時のみ下線表示
                _EditText.Background.Alpha = 100;
            }
            else
            {
                //非フォーカス時は非表示
                _EditText.Background.Alpha = 0;
            }
        }
    }

    internal class AiEditText:EditText
    {
        public Action ClearFocusAction { get; set; }
        public AiEditText(Context context):base(context)
        {
        }

        public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
        {
            if(keyCode == Keycode.Back && e.Action == KeyEventActions.Up){
                ClearFocus();
                ClearFocusAction?.Invoke();
            }
            return base.OnKeyPreIme(keyCode, e);

        }
    }
}