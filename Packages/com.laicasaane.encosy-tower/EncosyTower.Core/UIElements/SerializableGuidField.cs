using System;
using EncosyTower.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    [UxmlElement(libraryPath = "Encosy Tower")]
    public partial class SerializableGuidField : TextValueField<SerializableGuid>, IHasBindingPath
    {
        public static readonly string UssClassName = "serializable-guid-field";
        public static readonly string NewButtonUssClassName = $"{UssClassName}__new-v4-button";
        public static readonly string NewV7ButtonUssClassName = $"{UssClassName}__new-v7-button";

        internal static readonly BindingId s_guidFormatProperty = nameof(GuidFormat);

        private readonly Button _newV4Button;
        private readonly Button _newV7Button;

        /// <summary>
        /// Construct a SerializableGuidField.
        /// </summary>
        public SerializableGuidField() : this(string.Empty)
        {
        }

        /// <summary>
        /// Construct a SerializableGuidField.
        /// </summary>
        /// <param name="label"></param>
        public SerializableGuidField(string label) : this(label, string.Empty)
        {
        }

        /// <summary>
        /// Construct a SerializableGuidField.
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

            Add(_newV4Button = new Button() {
                text = "New v4",
                tooltip = "Creates a new Guid according to RFC 4122, following the Version 4 format.",
            }.WithClass(NewButtonUssClassName));

            Add(_newV7Button = new Button() {
                text = "New v7",
                tooltip = "Creates a new Guid according to RFC 9562, following the Version 7 format."
            }.WithClass(NewV7ButtonUssClassName));

            _newV4Button.clicked += OnNewV4;
            _newV7Button.clicked += OnNewV7;

            onIsReadOnlyChanged += OnIsReadOnlyChanged;
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

        private SerializableGuidInput GuidInput => (SerializableGuidInput)textInputBase;

        public override void ApplyInputDeviceDelta(Vector3 delta, DeltaSpeed speed, SerializableGuid startValue)
        {
        }

        protected override SerializableGuid StringToValue(string str)
        {
            return Guid.TryParse(str, out var result) ? result : Guid.Empty;
        }

        protected override string ValueToString(SerializableGuid value)
        {
            return value.ToString(GuidFormat);
        }

        private void OnNewV4()
        {
            if (isReadOnly)
            {
                return;
            }

            value = SerializableGuid.NewGuid();
        }

        private void OnNewV7()
        {
            if (isReadOnly)
            {
                return;
            }

            value = SerializableGuid.CreateVersion7();
        }

        private void OnIsReadOnlyChanged(bool value)
        {
            _newV4Button.WithDisplay(value ? DisplayStyle.None : DisplayStyle.Flex);
            _newV7Button.WithDisplay(value ? DisplayStyle.None : DisplayStyle.Flex);
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
                        if (Guid.TryParse(text, out var result) == false)
                        {
                            result = Guid.Empty;
                        }

                        text = result.ToString(value.NotEmptyOr("D"));
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
                return Guid.TryParse(str, out var result) ? result : Guid.Empty;
            }
        }
    }
}
