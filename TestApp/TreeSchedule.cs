using System;
using System.Linq;
using Pidgin;
using TestApp.IntervalTrees;

// ReSharper disable CommentTypo

namespace TestApp
{
    public class TreeSchedule
    {
        private IntervalTree<int, ScheduleFormatEntry> _years = new();
        private IntervalTree<int, ScheduleFormatEntry> _months = new();
        private IntervalTree<int, ScheduleFormatEntry> _days = new();
        private IntervalTree<int, ScheduleFormatEntry> _dayOfWeeks = new();
        private IntervalTree<int, ScheduleFormatEntry> _hours = new();
        private IntervalTree<int, ScheduleFormatEntry> _minutes = new();
        private IntervalTree<int, ScheduleFormatEntry> _seconds = new();
        private IntervalTree<int, ScheduleFormatEntry> _millis = new();

        /// <summary>
        /// Создает пустой экземпляр, который будет соответствовать
        /// расписанию типа "*.*.* * *:*:*.*" (раз в 1 мс).
        /// </summary>
        public TreeSchedule()
            : this("*.*.* * *:*:*.*")
        {
        }

        /// <summary>
        /// Создает экземпляр из строки с представлением расписания.
        /// </summary>
        /// <param name="scheduleString">Строка расписания.
        /// Формат строки:
        ///     yyyy.MM.dd w HH:mm:ss.fff
        ///     yyyy.MM.dd HH:mm:ss.fff
        ///     HH:mm:ss.fff
        ///     yyyy.MM.dd w HH:mm:ss
        ///     yyyy.MM.dd HH:mm:ss
        ///     HH:mm:ss
        /// Где yyyy - год (2000-2100)
        ///     MM - месяц (1-12)
        ///     dd - число месяца (1-31 или 32). 32 означает последнее число месяца
        ///     w - день недели (0-6). 0 - воскресенье, 6 - суббота
        ///     HH - часы (0-23)
        ///     mm - минуты (0-59)
        ///     ss - секунды (0-59)
        ///     fff - миллисекунды (0-999). Если не указаны, то 0
        /// Каждую часть даты/времени можно задавать в виде списков и диапазонов.
        /// Например:
        ///     1,2,3-5,10-20/3
        ///     означает список 1,2,3,4,5,10,13,16,19
        /// Дробью задается шаг в списке.
        /// Звездочка означает любое возможное значение.
        /// Например (для часов):
        ///     */4
        ///     означает 0,4,8,12,16,20
        /// Вместо списка чисел месяца можно указать 32. Это означает последнее
        /// число любого месяца.
        /// Пример:
        ///     *.9.*/2 1-5 10:00:00.000
        ///     означает 10:00 во все дни с пн. по пт. по нечетным числам в сентябре
        ///     *:00:00
        ///     означает начало любого часа
        ///     *.*.01 01:30:00
        ///     означает 01:30 по первым числам каждого месяца
        /// </param>
        public TreeSchedule(string scheduleString)
        {
            var format = ParserHelper.FullFormatParser.ParseOrThrow(scheduleString);
            BuildTree(_years, format.Date.Years, 0);
            BuildTree(_months, format.Date.Months, 1);
            BuildTree(_days, format.Date.Days, 1);
            BuildTree(_dayOfWeeks, format.DayOfWeek, 0);
            BuildTree(_hours, format.Time.Hours, 0);
            BuildTree(_minutes, format.Time.Minutes, 0);
            BuildTree(_seconds, format.Time.Seconds, 0);
            BuildTree(_millis, format.Time.Milliseconds, 0);

            void BuildTree(IntervalTree<int, ScheduleFormatEntry> tree, ScheduleFormatEntry[] schedule, int firstValue)
            {
                foreach (var entry in schedule)
                {
                    tree.Add(entry.Begin ?? firstValue, entry.End ?? entry.Begin ?? int.MaxValue, entry);
                }
            }
        }

        /// <summary>
        /// Возвращает следующий ближайший к заданному времени момент в расписании или
        /// само заданное время, если оно есть в расписании.
        /// </summary>
        /// <param name="t1">Заданное время</param>
        /// <returns>Ближайший момент времени в расписании</returns>
        public DateTime NearestEvent(DateTime t1) => Closest(t1, SearchDirection.Forward);

        /// <summary>
        /// Возвращает предыдущий ближайший к заданному времени момент в расписании или
        /// само заданное время, если оно есть в расписании.
        /// </summary>
        /// <param name="t1">Заданное время</param>
        /// <returns>Ближайший момент времени в расписании</returns>
        public DateTime NearestPrevEvent(DateTime t1) => Closest(t1, SearchDirection.Backward);

        /// <summary>
        /// Возвращает следующий момент времени в расписании.
        /// </summary>
        /// <param name="t1">Время, от которого нужно отступить</param>
        /// <returns>Следующий момент времени в расписании</returns>
        public DateTime NextEvent(DateTime t1) => NearestEvent(t1.AddMilliseconds(1));

        /// <summary>
        /// Возвращает предыдущий момент времени в расписании.
        /// </summary>
        /// <param name="t1">Время, от которого нужно отступить</param>
        /// <returns>Предыдущий момент времени в расписании</returns>
        public DateTime PrevEvent(DateTime t1) => NearestPrevEvent(t1.AddMilliseconds(-1));
        
        private DateTime Closest(DateTime t1, SearchDirection searchDirection)
        {
            void AdjustValue(IntervalTree<int, ScheduleFormatEntry> tree, int firstVal, int value, out int increment, out bool overflow)
            {
                var initVal = int.MaxValue; 
                var proposedVal = initVal;
                foreach (var entry in tree.Query(value))
                {
                    var nextMatch = entry.GetNextMatch(value, searchDirection, firstVal);
                    if (nextMatch != null)
                    {
                        proposedVal = Math.Min(proposedVal, nextMatch.Value);
                    }
                }

                if (proposedVal == initVal)
                {
                    var query = searchDirection == SearchDirection.Forward
                        ? tree.QueryLeftToRight(value, int.MaxValue)
                        : tree.QueryRightToLeft(0, value);
                    var nextInterval = query.FirstOrDefault();
                    if (nextInterval != null)
                    {
                        var nextMatch = nextInterval.GetNextMatch(value, searchDirection, firstVal);
                        if (nextMatch != null)
                        {
                            proposedVal = Math.Min(proposedVal, nextMatch.Value);
                        }
                    }
                }

                overflow = proposedVal == initVal;
                
                if (proposedVal != initVal)
                {
                    increment = proposedVal - value;
                }
                else
                {
                    increment = 0;
                }
            }

            while (true)
            {
                LoopStart:

                bool overflow;
                int increment;

                AdjustValue(_years, 0, t1.Year, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year + increment, 1, 1);
                }
                else if (overflow)
                {
                    throw new ArgumentOutOfRangeException(nameof(t1));
                }

                var month = t1.Month;
                AdjustValue(_months, 1, t1.Month, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year, t1.Month, 1).AddMonths(increment);
                }
                else if (overflow)
                {
                    t1 = new DateTime(t1.Year + 1, 1, 1);
                }
                if (t1.Month < month)
                {
                    goto LoopStart;
                }

                var day = t1.Day;
                var lastDayOfMonth = t1.Day == DateTime.DaysInMonth(t1.Year, t1.Month) && _days.Query(32).Any();

                if (!lastDayOfMonth)
                {
                    AdjustValue(_days, 1, t1.Day, out increment, out overflow);
                    if (increment > 0)
                    {
                        if (t1.Day + increment == 32)
                        {
                            t1 = new DateTime(t1.Year, t1.Month, DateTime.DaysInMonth(t1.Year, t1.Month));
                        }
                        else
                        {
                            t1 = new DateTime(t1.Year, t1.Month, t1.Day).AddDays(increment);
                        }
                    }
                    else if (overflow)
                    {
                        t1 = new DateTime(t1.Year, t1.Month, DateTime.DaysInMonth(t1.Year, t1.Month)).AddDays(1);
                    }

                    if (t1.Day < day)
                    {
                        goto LoopStart;
                    }
                }

                day = t1.Day;
                AdjustValue(_dayOfWeeks, 0, (int) t1.DayOfWeek, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day).AddDays(increment);
                }
                else if (overflow)
                {
                    t1 = new DateTime(t1.Year, t1.Month, DateTime.DaysInMonth(t1.Year, t1.Month)).AddDays(1);
                }
                if (t1.Day < day)
                {
                    goto LoopStart;
                }
                
                var hour = t1.Hour;
                AdjustValue(_hours, 0, t1.Hour, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, 0, 0).AddHours(increment);
                }
                else if (overflow)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, 23, 0, 0).AddHours(1);
                }
                if (t1.Hour < hour)
                {
                    goto LoopStart;
                }

                var minute = t1.Minute;
                AdjustValue(_minutes, 0, t1.Minute, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, t1.Minute, 0).AddMinutes(increment);
                }
                else if (overflow)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, 59, 0).AddMinutes(1);
                }
                if (t1.Minute < minute)
                {
                    goto LoopStart;
                }

                var second = t1.Second;
                AdjustValue(_seconds, 0, t1.Second, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, t1.Minute, t1.Second).AddSeconds(increment);
                }
                else if (overflow)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, t1.Minute, 59).AddSeconds(1);
                }
                if (t1.Second < second)
                {
                    goto LoopStart;
                }

                var millisecond = t1.Millisecond;
                AdjustValue(_millis, 0, t1.Millisecond, out increment, out overflow);
                if (increment > 0)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, t1.Minute, t1.Second, t1.Millisecond).AddMilliseconds(increment);
                }
                else if (overflow)
                {
                    t1 = new DateTime(t1.Year, t1.Month, t1.Day, t1.Hour, t1.Minute, t1.Second, 999).AddMilliseconds(1);
                }
                if (t1.Millisecond < millisecond)
                {
                    goto LoopStart;
                }

                return t1;
            }
        }
    }
}