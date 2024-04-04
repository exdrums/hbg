namespace Common.Utils
{
    public abstract class EnumString
    {
        public string Value { get; set; }
        public override string ToString() => Value;

        public EnumString(string value)
        {
            this.Value = value;
        }
    }
}