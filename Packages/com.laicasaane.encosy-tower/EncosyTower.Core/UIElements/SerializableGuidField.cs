using System;
using System.Diagnostics;
using EncosyTower.Common;
using EncosyTower.SystemExtensions;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.UIElements
{
    public class SerializableGuidField : TextValueField<SerializableGuid>
    {
        public static readonly string UssClassName = "serializable-guid-field";
        public static readonly string LabelUssClassName = $"{UssClassName}__label";
        public static readonly string InputUssClassName = $"{UssClassName}__input";

        internal static readonly BindingId s_version7Property = nameof(Version7);
        internal static readonly BindingId s_guidFormatProperty = nameof(GuidFormat);

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
        public SerializableGuidField(string label) : this(label, false, string.Empty)
        {
        }

        /// <summary>
        /// Creates a new SerializableGuidField.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="version7">Whether should use GUID version 7.</param>
        /// <param name="guidFormat">
        /// The value of format can be either null, an empty string (""), "N", "D", "B", "P", or "X".
        /// </param>
        public SerializableGuidField(string label, bool version7, string guidFormat)
            : base(label, 68, new SerializableGuidInput())
        {
            AddToClassList(UssClassName);

            pickingMode = PickingMode.Ignore;

            SetValueWithoutNotify(version7 ? SerializableGuid.CreateVersion7() : SerializableGuid.NewGuid());

            Version7 = version7;
            GuidFormat = guidFormat;
        }

        /// <summary>
        /// Whether should use GUID version 7.
        /// </summary>
        [CreateProperty]
        public bool Version7
        {
            get
            {
                return GuidInput.Version7;
            }

            set
            {
                var previous = GuidInput.Version7;
                GuidInput.Version7 = value;

                if (previous != value)
                {
                    NotifyPropertyChanged(s_version7Property);
                }
            }
        }

        /// <summary>
        /// The value of format can be either null, an empty string (""), "N", "D", "B", "P", or "X".
        /// </summary>
        [CreateProperty]
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
            return new Guid(str);
        }

        protected override string ValueToString(SerializableGuid value)
        {
            return value.ToString(GuidFormat);
        }

        [Serializable]
        public new class UxmlSerializedData : TextInputBaseField<SerializableGuid>.UxmlSerializedData
        {
            [SerializeField] private bool _version7;

            [UxmlIgnore, HideInInspector]
            [SerializeField] private UxmlAttributeFlags _version7_UxmlAttributeFlags;

            [SerializeField] private string _guidFormat;

            [UxmlIgnore, HideInInspector]
            [SerializeField] private UxmlAttributeFlags _guidFormat_UxmlAttributeFlags;

            [Conditional("UNITY_EDITOR")]
            public new static void Register()
            {
                TextInputBaseField<SerializableGuid>.UxmlSerializedData.Register();

                UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[] {
                    new(nameof(_version7), "version-7", null),
                    new(nameof(_guidFormat), "guid-format", null),
                });
            }

            public override object CreateInstance()
            {
                return new SerializableGuidField();
            }

            public override void Deserialize(object obj)
            {
                base.Deserialize(obj);

                var e = (SerializableGuidField)obj;

                if (ShouldWriteAttributeValue(_version7_UxmlAttributeFlags))
                {
                    e.Version7 = _version7;
                }

                if (ShouldWriteAttributeValue(_guidFormat_UxmlAttributeFlags))
                {
                    e.GuidFormat = _guidFormat;
                }
            }
        }

        private class SerializableGuidInput : TextValueInput
        {
            private bool _version7;

            public SerializableGuidInput()
            {
                formatString = string.Empty;
            }

            public bool Version7
            {
                get
                {
                    return _version7;
                }

                set
                {
                    if (_version7 = value)
                    {
                        text = new Guid(text).ToVersion7().ToString();
                    }
                    else
                    {
                        text = Guid.NewGuid().ToString();
                    }
                }
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
                        text = new Guid(text).ToString(value);
                    }

                    throw new FormatException(
                        "The value of format is not null, an empty string (\"\"), \"N\", \"D\", \"B\", \"P\", or \"X\""
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
