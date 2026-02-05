using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SmallUtilities
{
    public abstract class TimerCore
    {
        protected float elapsed = 0f;

        public void tick() => elapsed += Time.deltaTime;

        public void reset() => elapsed = 0f;

        public float time() => elapsed;

        public static void tickAllIn(IEnumerable<TimerCore> timers)
        {
            foreach (var timer in timers)
                timer.tick();
        }
        public static void resetAllIn(IEnumerable<TimerCore> timers)
        {
            foreach (var timer in timers)
                if(timer != null)
                    timer.reset();
        }
    }

    public class Timer : TimerCore
    {
        private readonly float timer = 0f;

        public Timer(float timer)
        {
            if (timer < 0f) throw new ArgumentOutOfRangeException("Timer cannot be less than 0");
            if (timer == 0f) throw new ArgumentNullException(nameof(timer));
            this.timer = timer;
        }

        public Timer(int timer)
        {
            if (timer < 0) throw new ArgumentOutOfRangeException("Timer cannot be less than 0");
            if (timer == 0) throw new ArgumentNullException(nameof(timer));
            this.timer = (float)timer;
        }

        public float showTimer() => timer;

        public bool isOver() => elapsed >= timer;

        public void autoTick()
        {
            if (!isOver())
                tick();
        }
    }

    public class SteppedTimer : TimerCore
    {
        private readonly List<float> steppedTimer = new List<float>();

        public SteppedTimer(IEnumerable<float> steppedTimer)
        {
            if (steppedTimer == null || steppedTimer.ToList().Count == 0) throw new ArgumentNullException(nameof(steppedTimer));
            foreach (var timer in steppedTimer)
            {
                if (timer < 0f) throw new ArgumentOutOfRangeException("No timer in 'steppedTimer' can be less than 0.");
                if (timer == 0f) throw new ArgumentNullException(nameof(timer));
            }
            this.steppedTimer = new List<float>(steppedTimer);
        }
        public List<float> showSteppedTimer() => steppedTimer;

        public List<bool> isOverStepped()
        {
            if (steppedTimer == null || steppedTimer.Count == 0) throw new ArgumentNullException("The timer wasn't inicialized.");
            var result = new List<bool>(steppedTimer.Count);
            foreach (var timer in steppedTimer)
                result.Add(elapsed >= timer);
            return result.ToList<bool>();
        }

        public bool allOver()
        {
            foreach (var timer in steppedTimer)
                if(!(elapsed >= timer)) return false;

            return true;
        }

        public void autoTick()
        {
            if (!allOver())
                tick();
        }
    }
}