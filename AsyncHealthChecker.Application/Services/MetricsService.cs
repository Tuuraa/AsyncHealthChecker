using Prometheus;

namespace AsyncHealthChecker.Application.Services.Implementations;

public static class MetricsService
{
    public static readonly Counter HttpRequests = Metrics.CreateCounter(
        "http_requests_total",
        "Total number of HTTP requests",
        new CounterConfiguration
        {
            LabelNames = ["method", "endpoint"]
        });

    public static readonly Histogram HttpRequestDuration = Metrics.CreateHistogram(
        "http_request_duration_seconds",
        "HTTP request duration in seconds",
        new HistogramConfiguration
        {
            LabelNames = ["method", "endpoint"]
        });
    
    public static readonly Counter TasksProcessed = Metrics.CreateCounter(
        "tasks_processed_total",
        "Total number of processed tasks");

    public static readonly Counter UrlsChecked = Metrics.CreateCounter(
        "urls_checked_total",
        "Total number of checked URLs",
        new CounterConfiguration
        {
            LabelNames = ["available"]
        });

    public static readonly Gauge ActiveWorkers = Metrics.CreateGauge(
        "active_workers",
        "Number of active workers");
}