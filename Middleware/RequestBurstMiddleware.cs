
using System.Collections.Concurrent;
using BookParadise.Data;
using BookParadise.Models;
using Microsoft.AspNetCore.Routing;

namespace BookParadise.Middleware
{
    public class RequestBurstMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly int[] BURST_THRESHOLDS = { 3, 5, 10, 20 };

        private readonly ConcurrentDictionary<string, List<DateTime>>
            _requestTracker = new();

        // Moi nguong chi canh bao mot lan trong 60 giay.
        private readonly ConcurrentDictionary<string, DateTime>
            _lastBurstEvent = new();

        public RequestBurstMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string path = context.Request.Path.Value ?? "";

            // Chi giam sat dung hai API nay.
            bool isSachApi =
                path.Equals(
                    "/api/Sach",
                    StringComparison.OrdinalIgnoreCase)
                ||
                path.Equals(
                    "/api/Sach/home-data",
                    StringComparison.OrdinalIgnoreCase);

            // Cac API khac khong duoc theo doi.
            if (!isSachApi)
            {
                await _next(context);
                return;
            }

            string ip = context.Connection.RemoteIpAddress?.ToString()
                        ?? "127.0.0.1";

            var endpoint = context.GetEndpoint() as RouteEndpoint;

            string route = endpoint?.RoutePattern.RawText
                           ?? path;

            string trackerKey = $"{ip}|{route.ToLowerInvariant()}";
            DateTime now = DateTime.UtcNow;

            var timestamps = _requestTracker.GetOrAdd(
                trackerKey,
                _ => new List<DateTime>());

            int currentCount;
            var thresholdsToCreate = new List<int>();

            lock (timestamps)
            {
                // Chi dem request trong 60 giay gan nhat.
                timestamps.RemoveAll(
                    t => (now - t).TotalSeconds >= 60);

                timestamps.Add(now);
                currentCount = timestamps.Count;

                foreach (int threshold in BURST_THRESHOLDS)
                {
                    if (currentCount != threshold)
                    {
                        continue;
                    }

                    string alertKey = $"{trackerKey}|{threshold}";

                    if (!_lastBurstEvent.TryGetValue(
                            alertKey, out DateTime lastTime)
                        || (now - lastTime).TotalSeconds >= 60)
                    {
                        _lastBurstEvent[alertKey] = now;
                        thresholdsToCreate.Add(threshold);
                    }
                }
            }

            foreach (int threshold in thresholdsToCreate)
            {
                try
                {
                    using var scope =
                        context.RequestServices.CreateScope();

                    var db = scope.ServiceProvider
                        .GetRequiredService<BookParadiseDbContext>();

                    int riskScore;
                    string severity;
                    string description;

                    if (threshold >= 20)
                    {
                        riskScore = 85;
                        severity = "High";
                        description =
                            $"Phát hiện IP {ip} gửi {currentCount} requests/phút vào {route}";
                    }
                    else if (threshold >= 10)
                    {
                        riskScore = 70;
                        severity = "Medium";
                        description =
                            $"Phát hiện IP {ip} gửi {currentCount} requests/phút vào {route}";
                    }
                    else if (threshold >= 5)
                    {
                        riskScore = 45;
                        severity = "Low";
                        description =
                            $"Phát hiện IP {ip} gửi {currentCount} requests/phút vào {route}";
                    }
                    else
                    {
                        riskScore = 0;
                        severity = "Info";
                        description =
                            $"Phát hiện IP {ip} gửi {currentCount} requests/phút vào {route}";
                    }

                    db.SecurityEvents.Add(new SecurityEvent
                    {
                        EventType = "request_burst",
                        SourceIp = ip,
                        Description = description,
                        RiskScore = riskScore,
                        Severity = severity,
                        DetectionStatus = "Pending",
                        ReviewStatus = "New",
                        IsSimulated = false,
                        CreatedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                }
                catch
                {
                    string alertKey = $"{trackerKey}|{threshold}";
                    _lastBurstEvent.TryRemove(alertKey, out _);
                    throw;
                }
            }

            await _next(context);
        }
    }
}
