﻿using System.Collections.Generic;
using Wumpus.Entities;

namespace Wumpus.Requests
{
    public class GetAuditLogParams : QueryMap
    {
        public Optional<ulong> UserId { get; set; }
        public Optional<AuditLogEvent> ActionType { get; set; }
        public Optional<ulong> Before { get; set; }
        public Optional<int> Limit { get; set; }

        public override IDictionary<string, object> GetQueryMap()
        {
            var dict = new Dictionary<string, object>();
            if (UserId.IsSpecified)
                dict["userId"] = UserId.ToString();
            if (ActionType.IsSpecified)
                dict["actionType"] = ((int)ActionType.Value).ToString();
            if (Before.IsSpecified)
                dict["before"] = Before.Value.ToString();
            if (Limit.IsSpecified)
                dict["limit"] = Limit.Value.ToString();
            return dict;
        }

        public void Validate()
        {
            Preconditions.NotZero(UserId, nameof(UserId));
            Preconditions.NotNegative(Limit, nameof(Limit));
        }
    }
}
