using System;
using EncosyTower.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    [UxmlElement]
    public partial class SerializableGuidField : TextValueField<SerializableGuid>, IHasBindingPath
    {
        public static readonly string UssClassName = "serializable-guid-field";
        public static readonly string NewButtonUssClassName = $"{UssClassName}__new-button";
        public static readonly string Version7ButtonUssClassName = $"{UssClassName}__version-7-button";

        internal static readonly BindingId s_guidFormatProperty = nameof(GuidFormat);

        private readonly Button _newButton;
        private readonly Button _version7Button;

        /// <summary>
        /// Creates a new SerializableGuidField.
        /// </summary>
        public SerializableGuidField() : this(string.Empty)
        {
        }

        /// <summary>
        /// Creates a new SerializableGuidField.
        /// </summary>
        /// <param name="label"></param>
        public SerializableGuidField(string label) : this(label, string.Empty)
        {
        }

        /// <summary>
        /// Creates a new SerializableGuidField.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="guidFormat">
        /// The value of format can be either null, an empty string (""), "N", "D", "B", "P", or "X".
        /// </param>
        public SerializableGuidField(string label, string guidFormat)
            : base(label, 68, new SerializableGuidInput())
        {
            AddToClassList(UssClassName);

            pickingMode = PickingMode.Ignore;
            GuidFormat = guidFormat;

            Add(_newButton = new Button() { text = "New" }.WithClass(NewButtonUssClassName));
            Add(_version7Button = new Button() { text = "Version 7" }.WithClass(Version7ButtonUssClassName));

            _newButton.clicked += OnCreateNew;
            _version7Button.clicked += OnCreateVersion7;
        }

        /// <summary>
        /// The value of format can be either null, an empty string (""), "N", "D", "B", "P", or "X".
        /// </summary>
        [UxmlAttribute("guid-format")]
        public string GuidFormat
        {
            get
            {
                return GuidInput.formatString;
            }

            set
            {
                var previous = GuidInput.GuidFormat;
                GuidInput.GuidFormat = value;

                if (previous != value)
                {
                    NotifyPropertyChanged(s_guidFormatProperty);
                }
            }
        }

#pragma warning disable IDE1006 // Naming Styles
        [UxmlAttribute("readonly")]
        public new bool isReadOnly
        {
            get
            {
                return base.isReadOnly;
            }

            set
            {
                _newButton.WithDisplay(value ? DisplayStyle.None : DisplayStyle.Flex);
                _version7Button.WithDisplay(value ? DisplayStyle.None : DisplayStyle.Flex);

                base.isReadOnly = value;
            }
        }
#pragma warning restore IDE1006 // Naming Styles

        private SerializableGuidInput GuidInput => (SerializableGuidInput)textInputBase;

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, SerializableGuid startValue)
        {
        }

        protected override SerializableGuid StringToValue(string str)
        {
            return new Guid(str);
        }

        protected override string ValueToString(SerializableGuid value)
        {
            return value.ToString(GuidFormat);
        }

        private void OnCreateNew()
        {
            if (isReadOnly)
            {
                return;
            }

            value = SerializableGuid.NewGuid();
        }

        private void OnCreateVersion7()
        {
            if (isReadOnly)
            {
                return;
            }

            value = SerializableGuid.CreateVersion7();
        }

        private class SerializableGuidInput : TextValueInput
        {
            public SerializableGuidInput()
            {
                formatString = string.Empty;
            }

            public string GuidFormat
            {
                get
                {
                    return formatString;
                }

                set
                {
                    if ((formatString = value) is null or "" or "N" or "D" or "B" or "P" or "X")
                    {
                        text = new Guid(text).ToString(value.NotEmptyOr("D"));
                        return;
                    }

                    throw new FormatException(
                        $"The value of format is not null, an empty string (\"\"), \"N\", \"D\", \"B\", \"P\", or \"X\""
                    );
                }
            }

            protected override string allowedCharacters => "abcdefx0123456789-(){}";

            public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, SerializableGuid startValue)
            {
            }

            protected override string ValueToString(SerializableGuid v)
            {
                return v.ToString(formatString);
            }

            protected override SerializableGuid StringToValue(string str)
            {
                return new Guid(str);
            }
        }
    }
}
