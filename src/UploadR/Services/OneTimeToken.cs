using System;

namespace UploadR.Services
{
    public class OneTimeToken
    {
        public Guid UserGuid;
        public Guid Token;
        public bool IsUsed;
        public DateTimeOffset ExpiresAt;

        public static OneTimeToken New(Guid userGuid, TimeSpan durationTime = default)
        {
            if (durationTime == default)
            {
                durationTime = TimeSpan.FromMinutes(30);
            }

            var ott = new OneTimeToken
            {
                UserGuid = userGuid,
                Token = Guid.NewGuid(),
                ExpiresAt = DateTimeOffset.Now.Add(durationTime),
                IsUsed = false
            };

            return ott;
        }
    }
}
