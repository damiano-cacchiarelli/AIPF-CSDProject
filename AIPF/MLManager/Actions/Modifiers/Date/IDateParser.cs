using System;

namespace AIPF.MLManager.Modifiers.Date
{
    public interface IDateParser<R>
    {
        public R Date { get; }

        public void SetDate(R date);

        public R ToR(DateTime date)
        {
            throw new NotImplementedException("The parse function is not implemented");
        }

        public static float ToMinute(DateTime date)
        {
            return date.Hour * 60 + date.Minute;
        }
    }
}