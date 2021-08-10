namespace TestApp
{
    public record ScheduleFormatEntry(int? Begin, int? End, int? Step)
    {
        public static ScheduleFormatEntry Always { get; } = new(null, null, null);
        public static ScheduleFormatEntry SinglePoint(int point) => new(point, null, null);
        
        public int? GetNextMatch(int value, SearchDirection searchDirection, int firstValue)
        {
            if (Step == null)
            {
                switch (searchDirection)
                {
                    case SearchDirection.Forward:
                        return Begin > value ? Begin : value;
                    case SearchDirection.Backward:
                        return End < value ? End : value;
                }

                return value;
            }

            var from = Begin ?? firstValue;
            
            var offset = value - from;
            if (offset % Step == 0)
            {
                // Если попали в шаг, возвращаем number
                return value;
            }

            // Если не попали в шаг, возвращаем ближайшее подходящее значение с округлением.
            // Если это значение выходит за пределы интервала, возвращаем null.
            var increment = searchDirection == SearchDirection.Forward ? 1 : -1;
            var result = from + (offset / Step.Value + increment) * Step.Value; 
            return result < from || result > End ? null : result;
        }
    }
}