using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sports_Analysis.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Sports_Analysis.Data;

namespace Sports_Analysis.Services
{
    public interface IFootballDataService
    {
        Task<List<FootballMatch>> GetMatchesAsync(int offset = 0, int limit = 100);
        Task<List<FootballMatch>> GetAllMatchesAsync();
        Task<bool> RefreshCacheAsync();
    }

    public class FootballDataService : IFootballDataService
    {
        private static List<FootballMatch>? _allMatchesCache = null;
        private static DateTime _cacheTimestamp = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5); // Reduced for development
        private readonly ILogger<FootballDataService> _logger;
        private readonly FootballDbContext _context;
        private static readonly object _cacheLock = new object();

        public FootballDataService(ILogger<FootballDataService> logger, FootballDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<FootballMatch>> GetMatchesAsync(int offset = 0, int limit = 100)
        {
            try
            {
                var all = await GetAllMatchesAsync();
                return all.Skip(offset).Take(limit).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting matches with offset {Offset} and limit {Limit}", offset, limit);
                throw;
            }
        }

        public async Task<List<FootballMatch>> GetAllMatchesAsync()
        {
            try
            {
                lock (_cacheLock)
                {
                    if (_allMatchesCache != null && (DateTime.UtcNow - _cacheTimestamp) < CacheDuration)
                    {
                        _logger.LogInformation("Returning cached matches. Cache age: {Age} minutes", 
                            (DateTime.UtcNow - _cacheTimestamp).TotalMinutes);
                        return _allMatchesCache;
                    }
                }

                return await LoadMatchesFromDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all matches");
                throw;
            }
        }

        public async Task<bool> RefreshCacheAsync()
        {
            try
            {
                var matches = await LoadMatchesFromDatabaseAsync();
                lock (_cacheLock)
                {
                    _allMatchesCache = matches;
                    _cacheTimestamp = DateTime.UtcNow;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache");
                return false;
            }
        }

        private async Task<List<FootballMatch>> LoadMatchesFromDatabaseAsync()
        {
            try
            {
                _logger.LogInformation("Fetching matches from database");
                var matches = await _context.FootballMatches
                    .FromSqlRaw("SELECT * FROM dbo.FootballMatches")
                    .OrderByDescending(m => m.Date)
                    .ToListAsync();
                
                _logger.LogInformation("Successfully loaded {Count} matches from database", matches.Count);

                lock (_cacheLock)
                {
                    _allMatchesCache = matches;
                    _cacheTimestamp = DateTime.UtcNow;
                }

                return matches;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching matches from database");
                throw;
            }
        }
    }
} 