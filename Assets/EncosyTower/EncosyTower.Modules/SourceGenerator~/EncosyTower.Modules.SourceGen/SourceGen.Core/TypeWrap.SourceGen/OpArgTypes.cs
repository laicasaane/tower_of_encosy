namespace EncosyTower.Modules.TypeWrap.SourceGen
{
    public readonly struct OpArgTypes
    {
        public readonly OpType First;
        public readonly OpType Second;

        public OpArgTypes(OpType first, OpType second = default)
        {
            First = first;
            Second = second;
        }

        public void Deconstruct(
              out bool isValid
            , out OpType firstType, out string firstName
        )
        {
            firstType = First;

            if (string.IsNullOrEmpty(firstType.Value) == false)
            {
                firstName = "value";
                isValid = true;
            }
            else
            {
                firstName = string.Empty;
                isValid = false;
            }
        }

        public void Deconstruct(
              out bool isValid
            , out OpType firstType, out string firstName
            , out OpType secondType, out string secondName
        )
        {
            firstType = First;
            secondType = Second;

            if (string.IsNullOrEmpty(secondType.Value))
            {
                if (string.IsNullOrEmpty(firstType.Value))
                {
                    firstName = secondName = string.Empty;
                    isValid = false;
                }
                else
                {
                    firstName = "value";
                    secondName = string.Empty;
                    isValid = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(firstType.Value))
                {
                    firstName = secondName = string.Empty;
                    isValid = false;
                }
                else
                {
                    firstName = "left";
                    secondName = "right";
                    isValid = true;
                }
            }
        }
    }
}
