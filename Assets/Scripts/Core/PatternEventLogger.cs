using System;
using System.Collections.Generic;
using UnityEngine;

namespace PatternSurvivor.Core
{
    public sealed class PatternEventRecord
    {
        public PatternEventRecord(string pattern, string message, int index, float time)
        {
            Pattern = pattern;
            Message = message;
            Index = index;
            Time = time;
        }

        public string Pattern { get; }
        public string Message { get; }
        public int Index { get; }
        public float Time { get; }
    }

    public static class PatternEventLogger
    {
        private const int MaxRecords = 80;
        private static readonly List<PatternEventRecord> Records = new List<PatternEventRecord>();
        private static int nextIndex;

        public static event Action<PatternEventRecord> EventRecorded;

        public static IReadOnlyList<PatternEventRecord> History => Records;

        public static void Record(string pattern, string message)
        {
            var record = new PatternEventRecord(pattern, message, ++nextIndex, Time.unscaledTime);
            Records.Add(record);
            if (Records.Count > MaxRecords)
            {
                Records.RemoveAt(0);
            }

            Debug.Log($"[{record.Time:0.00}] {pattern}: {message}");
            EventRecorded?.Invoke(record);
        }

        public static PatternEventRecord LatestFor(string pattern)
        {
            for (int i = Records.Count - 1; i >= 0; i--)
            {
                if (Records[i].Pattern == pattern)
                {
                    return Records[i];
                }
            }

            return null;
        }

        public static void Clear()
        {
            Records.Clear();
            nextIndex = 0;
        }
    }
}
