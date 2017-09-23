﻿using System.Collections.Generic;

namespace Wumpus.Requests
{
    public class GetChannelMessagesParams : QueryMap
    {
        public Optional<ulong> Around { get; set; }
        public Optional<ulong> Before { get; set; }
        public Optional<ulong> After { get; set; }
        public Optional<int> Limit { get; set; }

        public override IDictionary<string, object> GetQueryMap()
        {
            var dict = new Dictionary<string, object>();
            if (Limit.IsSpecified)
                dict["limit"] = Limit.Value.ToString();
            if (Around.IsSpecified)
                dict["around"] = Around.Value.ToString();
            if (Before.IsSpecified)
                dict["before"] = Before.Value.ToString();
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
