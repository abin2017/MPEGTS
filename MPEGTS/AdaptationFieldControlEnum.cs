using System;

namespace MPEGTS
{
    public enum AdaptationFieldControlEnum
    {
        Unknown = 0,
        NoAdaptationFieldPayloadOnly = 1,
        AdaptationFieldOnlyNoPayload = 2,
        AdaptationFieldFollowedByPayload = 3
    }
}
