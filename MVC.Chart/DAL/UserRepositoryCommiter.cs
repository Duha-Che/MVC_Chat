using System;
using System.Threading;

namespace DAL
{
    public class UserRepositoryCommiter : IDisposable
    {
        private static Timer Timer;
        private readonly TimeSpan SavingPeriod = TimeSpan.FromMinutes(2);  // Save Db every n minutes
        private UserRepository _owner;
        private static ManualResetEventSlim Blocker;
        private static object CreationSyncRoot = new object();

        public static UserRepositoryCommiter Create( TimeSpan period)
        {
            if (period == TimeSpan.Zero || period.TotalMinutes < 1.0 )
                throw new ArgumentOutOfRangeException("period", "period is zero or too small. Set proper saving period (one minute or more)");

            return new UserRepositoryCommiter(UserRepository.Instance, period);
        }

        private static Timer _timer;
        private static ManualResetEventSlim _blocker;

        private UserRepositoryCommiter(UserRepository owner, TimeSpan period)
        {
            _owner = owner;
            SavingPeriod = period;

            if (Timer != null)
                return;

            lock (CreationSyncRoot)
            {
                if (Timer != null)
                    return;
                _timer = Timer = new Timer(Save, _owner, period, period);
                _blocker = Blocker = new ManualResetEventSlim(true);
            }

            Log.Message("[UserRepositoryCommiter] Stat commiting UserRepository every {0} minutes", period.TotalMinutes);
        }

        private void Save(object repository)
        {
            Log.Message("[UserRepositoryCommiter] Save started");
            if (!_blocker.Wait(1))
            {
                Log.Message("[UserRepositoryCommiter] Save skipped");
                return;  //previous step hasn't fnished yet or disposing begins - breaks action anyway.
            }

            _blocker.Reset();
            try
            {
                var owner = repository as UserRepository;
                if (owner == null)
                    return;
                owner.Commit();
            }
            finally
            {
                _blocker.Set();
                Log.Message("[UserRepositoryCommiter] Save done");
            }
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                Log.Message("[UserRepositoryCommiter] Disposing...");
                _blocker.Wait();
                _blocker.Set();
                _timer.Dispose();
                _timer = null;
                _blocker.Dispose();
                _blocker = null;
                Log.Message("[UserRepositoryCommiter] Disposed");
            }
        }
    }
}
