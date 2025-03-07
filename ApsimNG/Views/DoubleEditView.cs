﻿// -----------------------------------------------------------------------
// <copyright file="DoubleEditView.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------

namespace UserInterface.Views
{
    using System;
    using Gtk;

    /// <summary>An interface for a GTK.Entry control</summary>
    public interface IDoubleEditView
    {
        /// <summary>Gets or sets the value displayed</summary>
        double Value { get; set; }

        /// <summary>Gets or sets a value indicating whether the control should be editable.</summary>
        bool IsEditable { get; set; }

        bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the maximum value allowed
        /// </summary>
        double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum value allowed
        /// </summary>
        double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the number of decimal places to show
        /// </summary>
        int DecPlaces { get; set; }
    }

    /// <summary>A drop down view.</summary>
    public class DoubleEditView : ViewBase, IDoubleEditView
    {
        /// <summary>
        /// The control to manage/wrap
        /// </summary>
        private Entry textEntry;

        /// <summary>
        /// Internal representation of the value
        /// </summary>
        private double value;

        /// <summary>Constructor for DoubleEditView</summary>
        /// <param name="owner">The owning view</param>
        public DoubleEditView(ViewBase owner) : base(owner)
        {
            textEntry = new Entry();
            SetupDoubleEdit();
        }

        /// <summary>
        /// Constructor the the DoubleEditView
        /// </summary>
        /// <param name="owner">The owning view</param>
        /// <param name="textbox">The textbox to manage</param>
        public DoubleEditView(ViewBase owner, Entry textbox) : base(owner)
        {
            textEntry = textbox;
            SetupDoubleEdit();
        }

        public DoubleEditView(ViewBase owner, Entry textbox, double maximum, double minimum, int decplaces) : base(owner)
        {
            textEntry = textbox;
            MaxValue = maximum;
            MinValue = minimum;
            DecPlaces = decplaces;

            SetupDoubleEdit();
        }

        /// <summary>
        /// Gets or sets the floating point value from this control
        /// </summary>
        public double Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = Math.Max(MinValue, Math.Min(value, MaxValue));
                ShowValue();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value allowed
        /// </summary>
        public double MaxValue { get; set; } = double.MaxValue;

        /// <summary>
        /// Gets or sets the minimum value allowed
        /// </summary>
        public double MinValue { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the number of decimal places to show
        /// </summary>
        public int DecPlaces { get; set; } = 2;

        /// <summary>
        /// Gets or sets a value indicating whether the control is editable
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return textEntry.IsEditable;
            }

            set
            {
                textEntry.IsEditable = value;
            }
        }

        public bool Visible
        {
            get
            {
                return textEntry.Visible;
            }

            set
            {
                textEntry.Visible = value;
            }
        }

        /// <summary>
        /// Attach events
        /// </summary>
        private void SetupDoubleEdit()
        {
            mainWidget = textEntry;
            textEntry.Changed += OnChanged;
            mainWidget.Destroyed += _mainWidget_Destroyed;
        }

        /// <summary>
        /// Cleanup the object
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void _mainWidget_Destroyed(object sender, EventArgs e)
        {
            textEntry.Changed -= OnChanged;
            mainWidget.Destroyed -= _mainWidget_Destroyed;
            owner = null;
        }

        /// <summary>
        /// Display the floating point value
        /// </summary>
        private void ShowValue()
        {
            if ((this.value == double.MaxValue) || (this.value == double.MinValue))
                textEntry.Text = string.Empty;
            else
                textEntry.Text = string.Format("{0,2:f" + DecPlaces.ToString() + "}", this.value);
        }

        /// <summary>
        /// The handler for editing changes
        /// </summary>
        /// <param name="sender">The sending object</param>
        /// <param name="e">The event arguments</param>
        private void OnChanged(object sender, EventArgs e)
        {
            double result;
            if (double.TryParse(textEntry.Text, out result))    // TODO: need to check the ranges here and adjust the viewed value
                value = result;
            else
            {
                textEntry.Text = "0.0";
            }
        }
    }
}
