using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace SphereBase.Server
{
    internal class Database
    {
        private readonly Regex GETRegex = new Regex("((GET) [a-zA-Z0-9_]+)");
        private readonly Regex SETRegex = new Regex("((SET) [a-zA-Z0-9_]+ (TO) [a-zA-Z0-9]+)");

        private ConcurrentDictionary<string, string> Data { get; set; } = new();

        /// <summary>
        /// Evaluate a Database query.
        /// </summary>
        /// <param name="query">The query user wanted to evaluate.</param>
        /// <returns>Response.</returns>
        public Task<string> EvaluateQuery(string query)
        {
            if (GETRegex.IsMatch(query))
            {
                string key = query.Split(' ')[1];
				
				string? value;
				if(!Data.TryGetValue(key, out value)) return Task.FromResult("null");
				
                return Task.FromResult(value);
            }
            else if (SETRegex.IsMatch(query))
            {
                string[] split = query.Split(' ');
                string key = split[1], value = split[3];
				
                if (!Data.ContainsKey(key))
                {
                    if(Data.TryAdd(key, value)) return Task.FromResult($"OK 200");
					return Task.FromResult("ERR 500 Internal server error.");
                }
				
				string? oldValue;
				if(!Data.TryGetValue(key, out oldValue)) return Task.FromResult("ERR 500 Internal server error.");
                if(Data.TryUpdate(key, value, oldValue)) return Task.FromResult("OK 200");
				return Task.FromResult("ERR 500 Internal server error.");
            }
            else
                return Task.FromResult("ERR 400 Bad request");
        }
    }
}