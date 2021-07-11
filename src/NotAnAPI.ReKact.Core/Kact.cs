﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NotAnAPI.ReKact.Core.Enums;
using NotAnAPI.ReKact.Core.EventArgs;

namespace NotAnAPI.ReKact.Core
{
    /// <summary>
    /// Class Kact.
    /// </summary>
    /// <autogeneratedoc />
    public class Kact
    {
        /// <summary>
        /// The kact string
        /// </summary>
        private readonly string _kact;

        /// <summary>
        /// Gets the sensor data.
        /// </summary>
        /// <value>The sensor data.</value>
        /// <autogeneratedoc />
        public string SensorData { get; private set; }

        /// <summary>
        /// Gets the keys which is a list of events stored and separated by ; in kact.
        /// </summary>
        /// <value>The keys.</value>
        public List<KeyAct> Keys { get; }

        /// <summary>
        /// Returns true if Kact is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        /// <autogeneratedoc />
        public bool IsValid => Keys?.Count > 0;

        /// <summary>
        /// Gets the total submissions detected in the Kact, submissions are called whatever that cause the sensor_data submission, e.g [ENTER].
        /// </summary>
        /// <value>The total submissions.</value>
        public int Submissions { get; private set; }

        #region Events

        /// <summary>
        /// Delegate KeyDownHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotAnAPI.ReKact.Core.EventArgs.KeyDownEventArgs"/> instance containing the event data.</param>
        /// <autogeneratedoc />
        public delegate void KeyDownHandler(object sender, KeyDownEventArgs e);

        /// <summary>
        /// Occurs on ReKact KeyDown event.
        /// </summary>
        public event KeyDownHandler OnKeyDown;

        /// <summary>
        /// Delegate KeyPressHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotAnAPI.ReKact.Core.EventArgs.KeyPressEventArgs"/> instance containing the event data.</param>
        /// <autogeneratedoc />
        public delegate void KeyPressHandler(object sender, KeyPressEventArgs e);

        /// <summary>
        /// Occurs on ReKact KeyPress event.
        /// </summary>
        public event KeyPressHandler OnKeyPress;

        /// <summary>
        /// Delegate KeyUpHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotAnAPI.ReKact.Core.EventArgs.KeyUpEventArgs"/> instance containing the event data.</param>
        /// <autogeneratedoc />
        public delegate void KeyUpHandler(object sender, KeyUpEventArgs e);

        /// <summary>
        /// Occurs on ReKact KeyUp event.
        /// </summary>
        public event KeyUpHandler OnKeyUp;

        /// <summary>
        /// Delegate WaitHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="WaitEventArgs"/> instance containing the event data.</param>
        /// <autogeneratedoc />
        public delegate void WaitHandler(object sender, WaitEventArgs e);

        /// <summary>
        /// Occurs on ReKact wait.
        /// </summary>
        public event WaitHandler OnWait;

        #endregion

        /// <summary>
        /// <para>The must have total charReKacts!</para>
        /// When a key that produces a charReKact value is pressed down there will also be KeyPress event right after that and it's determined by total KeyPress events.
        /// </summary>
        private int _mustHaveTotalCharReKacts;

        /// <summary>
        /// The total charReKacts that are wrote by ReKact.
        /// </summary>
        private int _totalCharReKacts;

        /// <summary>
        /// <para>Initializes a new instance of the <see cref="Kact"/> class.</para>
        /// The Constructor initialization also validates if there is a valid kact string in given string. if not, <see cref="NotAnAPI.ReKact.Core.Kact.IsValid"/> value will be false.
        /// </summary>
        /// <param name="kact">The text containing kact string, or only the kact string.</param>
        public Kact(string kact)
        {
            if (kact == null) return;

            if (kact.Contains("-1,2,-94,-108,", StringComparison.Ordinal) &&
                kact.Contains("-1,2,-94,-110,", StringComparison.Ordinal))
            {
                SensorData = kact;
                kact = Globals.Regexes.ExtractKactFromSensor.Match(kact).Groups["Kact"].Value;
            }

            kact = kact.Trim('\r', '\n', ' ', '\t');

            if (Validate(kact) is not {IsValid: true} validationResult) return;

            this.Keys = validationResult.Keys;

            this._kact = kact;
            
            this._mustHaveTotalCharReKacts = Keys.Count(act => act.Type == ActTypes.KeyPress);

            this.SetStats();
        }

        /// <summary>
        /// Validates the specified kact string.
        /// </summary>
        /// <param name="kact">The kact string.</param>
        /// <returns>(bool IsValid, System.Collections.Generic.List&lt;<see cref="NotAnAPI.ReKact.Core.KeyAct"/>&gt; Keys).</returns>
        public static (bool IsValid, List<KeyAct> Keys) Validate(string kact)
        {
            if (!Globals.Regexes.ValidateKactString.IsMatch(kact)) return (IsValid: false, Keys: null);
            string[] splitKact = SplitKacts(kact);
            List<KeyAct> keys = new();
            foreach (string kAct in splitKact)
            {
                string[] splitKeyAct = SplitAct(kAct);
                if (splitKeyAct.Length is <= 6 or >= 9) return (IsValid: false, Keys: null);
                keys.Add(new KeyAct(splitKeyAct));
            }

            return (IsValid: true, Keys: keys);
        }

        /// <summary>
        /// Sets the stats.<para />
        /// Stats are:
        /// <list type="bullet">
        /// <item><seealso cref="Submissions"/></item>
        /// </list>
        /// </summary>
        private void SetStats()
        {
            if (this.Keys.Count < 1)
            {
                this.Submissions = 0;
            }

            this.Submissions = Keys.Count(x => x.IsSubmission);
        }

        /// <summary>
        /// Splits the kacts by <c>;</c> separator.
        /// </summary>
        /// <param name="kacts">The kact string.</param>
        /// <returns>string[].</returns>
        private static string[] SplitKacts(string kacts)
        {
            return kacts.Split(';', StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Splits the Act string by <c>,</c> separator.
        /// </summary>
        /// <param name="keyAct">The Act string.</param>
        /// <returns>string[].</returns>
        private static string[] SplitAct(string keyAct)
        {
            return keyAct.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the summary of the Kact.
        /// </summary>
        /// <returns><see cref="string"/> summary.</returns>
        public string GetSummary()
        {
            return $@"===========================
Total Acts: {this.Keys.Count}
Total Key Downs: {this.Keys.Count(act => act.Type == ActTypes.KeyDown)}
Total Key Presses: {this.Keys.Count(act => act.Type == ActTypes.KeyPress)}
Total Key Ups: {this.Keys.Count(act => act.Type == ActTypes.KeyUp)}
Submissions: {this.Submissions}
===========================";
        }

        /// <summary>
        /// The key press queue
        /// </summary>
        /// <autogeneratedoc />
        private List<KeyAct> _keyPressQueue;
        /// <summary>
        /// The key up queue
        /// </summary>
        /// <autogeneratedoc />
        private List<KeyAct> _keyUpQueue;
        /// <summary>
        /// The unknown key codes
        /// </summary>
        /// <autogeneratedoc />
        private List<KeyAct> _unknownKeyCodes;

        /// <summary>
        /// Plays the specified skip wait.
        /// </summary>
        /// <param name="skipWait">The skip wait.</param>
        /// <autogeneratedoc />
        public void Play(bool skipWait = false)
        {
            _keyPressQueue = new();
            _keyUpQueue = new();
            _unknownKeyCodes = new();
            for (int i = 0; i < Keys.Count; i++)
            {
                int wait = skipWait ? 0 : i >= 1 ? (Keys[i].Time - Keys[i - 1].Time) : Keys[i].Time;
                if (wait > 0)
                {
                    OnWait?.Invoke(this, new WaitEventArgs(wait));
                    Thread.Sleep(wait);
                }

                FireEvent(Keys[i]);
            }
        }

        /// <summary>
        /// Fires the event for the specific Act.
        /// </summary>
        /// <param name="keyAct">The Act.</param>
        private void FireEvent(KeyAct keyAct)
        {
            if (keyAct.IsUnknown) _unknownKeyCodes.Add(keyAct);
            switch (keyAct.Type)
            {
                case ActTypes.KeyDown:
                    if (_totalCharReKacts < _mustHaveTotalCharReKacts && keyAct.KeyCode == -2)
                    {
                        keyAct.Char = keyAct.GetKeyByKeyCode(true);
                        _totalCharReKacts++;
                    }
                    else if (keyAct.KeyCode == -2)
                    {
                        keyAct.Char = keyAct.GetKeyByKeyCode();
                    }
                    
                    if (keyAct.IsCharReKact) _keyPressQueue.Add(keyAct);
                    _keyUpQueue.Add(keyAct);
                    OnKeyDown?.Invoke(this, new KeyDownEventArgs(keyAct, _keyPressQueue, _keyUpQueue));
                    return;
                case ActTypes.KeyPress:
                    if (_keyPressQueue.Count > 0)
                    {
                        KeyAct keyThatNotYetUp = null;

                        void SetKeyThatNotYetUp()
                        {
                            for (int iKeyPressQueue = 0; iKeyPressQueue < _keyPressQueue.Count; iKeyPressQueue++)
                            {
                                for (int iKeyUpQueue = 0; iKeyUpQueue < _keyUpQueue.Count; iKeyUpQueue++)
                                {
                                    if (keyAct.KeyCode.Equals(_keyPressQueue[iKeyPressQueue].KeyCode) && _keyUpQueue[iKeyUpQueue].Hash.Equals(_keyPressQueue[iKeyPressQueue].Hash) && _keyUpQueue[iKeyUpQueue].IsCharReKact)
                                    {
                                        keyThatNotYetUp = _keyPressQueue[iKeyPressQueue];
                                        _keyPressQueue.RemoveAt(iKeyPressQueue);
                                        return;
                                    }
                                }
                            }
                        }
                        SetKeyThatNotYetUp();

                        keyAct.Char = keyThatNotYetUp?.Char ?? keyAct.Char;
                    }

                    OnKeyPress?.Invoke(this, new KeyPressEventArgs(keyAct, _keyPressQueue, _keyUpQueue));
                    return;
                case ActTypes.KeyUp:
                    if (_keyUpQueue.Count > 0)
                    {
                        KeyAct keyThatAlreadyPressed = null;

                        void SetKeyThatAlreadyPressed()
                        {
                            if (_keyPressQueue.Count == 0 && _keyUpQueue.Count > 0)
                            {
                                for (int iKeyUpQueue = 0; iKeyUpQueue < _keyUpQueue.Count; iKeyUpQueue++)
                                {
                                    if (_keyUpQueue[iKeyUpQueue].KeyCode == keyAct.KeyCode)
                                    {
                                        keyThatAlreadyPressed = _keyUpQueue[iKeyUpQueue];
                                        _keyUpQueue.RemoveAt(iKeyUpQueue);
                                        return;   
                                    }
                                }
                            }
                            for (int iKeyUpQueue = 0; iKeyUpQueue < _keyUpQueue.Count; iKeyUpQueue++)
                            {
                                for (int iKeyPressQueue = 0; iKeyPressQueue < _keyPressQueue.Count; iKeyPressQueue++)
                                {
                                    if (keyAct.KeyCode.Equals(_keyUpQueue[iKeyUpQueue].KeyCode) && _keyPressQueue[iKeyPressQueue].Hash.Equals(_keyUpQueue[iKeyUpQueue].Hash) == false)
                                    {
                                        keyThatAlreadyPressed = _keyUpQueue[iKeyUpQueue];
                                        _keyUpQueue.RemoveAt(iKeyUpQueue);
                                        return;
                                    }
                                }
                            }
                        }

                        if (keyAct.KeyCode < 0)
                        {
                            SetKeyThatAlreadyPressed();
                        }
                        else
                        {
                            keyThatAlreadyPressed = _keyUpQueue.FirstOrDefault();
                            if (keyThatAlreadyPressed != null) _keyUpQueue.RemoveAt(0);
                        }

                        keyAct.Char = keyThatAlreadyPressed?.Char ?? keyAct.Char;
                    }
                    OnKeyUp?.Invoke(this, new KeyUpEventArgs(keyAct, _keyPressQueue, _keyUpQueue));
                    return;
            }
        }

        /// <summary>
        /// Gets the leftover key events. <para />
        /// Used only to show Warnings
        /// </summary>
        /// <returns>(<see cref="int"/> KeyPress, <see cref="int"/> KeyUp).</returns>
        public (int KeyPress, int KeyUp) GetLeftoverKeyEvents()
        {
            return (_keyPressQueue.Count(act => act.KeyCode == 2), _keyUpQueue.Count);
        }

        /// <summary>
        /// Gets the unknowns. <para />
        /// Used only to show Warnings
        /// </summary>
        /// <returns>System.Collections.ObjectModel.ReadOnlyCollection&lt;<see cref="NotAnAPI.ReKact.Core.KeyAct"/>&gt;.</returns>
        public ReadOnlyCollection<KeyAct> GetUnknowns()
        {
            return _unknownKeyCodes.AsReadOnly();
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns><see cref="string"/> kact.</returns>
        public override string ToString()
        {
            return _kact ?? string.Empty;
        }
    }
}