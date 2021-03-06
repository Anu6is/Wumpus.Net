﻿using System.Collections.Generic;

namespace Wumpus.Requests
{
    public class GetGuildMembersParams : QueryMap
    {
        public Optional<int> Limit { get; set; }
        public Optional<ulong> After { get; set; }

        public override IDictionary<string, object> GetQueryMap()
        {
            var dict = new Dictionary<string, object>();
            if (Limit.IsSpecified)
                dict["limit"] = Limit.Value.ToString();
            if (After.IsSpecified)
                dict["after"] = After.Value.ToString();
            return dict;
        }

        public void Validate()
        {
            Preconditions.NotNegative(Limit, nameof(Limit));
        }
    }
}
