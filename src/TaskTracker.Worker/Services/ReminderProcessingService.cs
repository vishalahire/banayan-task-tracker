using TaskTracker.Application.Interfaces;

namespace TaskTracker.Worker.Services;

public class ReminderProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderProcessingService> _logger;
    private readonly TimeSpan _processingInterval;
    private readonly TimeSpan _reminderWindow;

    public ReminderProcessingService(
        IServiceProvider serviceProvider,
        ILogger<ReminderProcessingService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        
        // Configure processing interval (default: 5 minutes)
        var intervalMinutes = configuration.GetValue<int>("ReminderProcessing:IntervalMinutes", 5);
        _processingInterval = TimeSpan.FromMinutes(intervalMinutes);
        
        // Configure reminder window (default: 24 hours)
        var windowHours = configuration.GetValue<int>("ReminderProcessing:WindowHours", 24);
        _reminderWindow = TimeSpan.FromHours(windowHours);

        _logger.LogInformation(
            "Reminder processing service configured with {IntervalMinutes} minute intervals and {WindowHours} hour window",
            intervalMinutes, windowHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder processing service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing reminders");
            }

            try
            {
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("Reminder processing service stopped");
    }

    private async Task ProcessRemindersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting reminder processing run");
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();

            // Get tasks due in the configured window
            var tasksDue = await reminderService.GetTasksDueForReminderAsync(_reminderWindow, cancellationToken);
            var tasksArray = tasksDue.ToArray();

            _logger.LogInformation("Found {TaskCount} tasks due within {WindowHours} hours", 
                tasksArray.Length, _reminderWindow.TotalHours);

            int remindersProcessed = 0;
            int remindersSkipped = 0;
            int remindersSucceeded = 0;
            int remindersFailed = 0;

            foreach (var task in tasksArray)
            {
                try
                {
                    // Determine reminder type based on time until due date
                    var timeUntilDue = task.DueDate - DateTimeOffset.UtcNow;
                    var reminderType = DetermineReminderType(timeUntilDue);

                    // Check if reminder has already been sent (idempotency)
                    var alreadySent = await reminderService.HasReminderBeenSentAsync(
                        task.Id, reminderType, task.DueDate, cancellationToken);

                    if (alreadySent)
                    {
                        remindersSkipped++;
                        _logger.LogDebug("Reminder already sent for task {TaskId} ({ReminderType})", 
                            task.Id, reminderType);
                        continue;
                    }

                    // Simulate sending reminder (in real implementation, this would send email/notification)
                    var deliverySuccessful = await SimulateSendReminderAsync(task, reminderType, cancellationToken);

                    // Log the reminder attempt
                    var logSuccess = await reminderService.LogReminderSentAsync(
                        task.Id, 
                        reminderType, 
                        deliverySuccessful, 
                        deliverySuccessful ? "Reminder sent successfully" : "Failed to send reminder",
                        cancellationToken);

                    if (logSuccess)
                    {
                        remindersProcessed++;
                        if (deliverySuccessful)
                        {
                            remindersSucceeded++;
                            _logger.LogInformation(
                                "Reminder sent successfully for task '{TaskTitle}' (ID: {TaskId}, Type: {ReminderType})", 
                                task.Title, task.Id, reminderType);
                        }
                        else
                        {
                            remindersFailed++;
                            _logger.LogWarning(
                                "Failed to send reminder for task '{TaskTitle}' (ID: {TaskId}, Type: {ReminderType})", 
                                task.Title, task.Id, reminderType);
                        }
                    }
                    else
                    {
                        _logger.LogError("Failed to log reminder for task {TaskId}", task.Id);
                    }
                }
                catch (Exception ex)
                {
                    remindersFailed++;
                    _logger.LogError(ex, "Error processing reminder for task {TaskId} ({TaskTitle})", 
                        task.Id, task.Title);
                }
            }

            stopwatch.Stop();
            _logger.LogInformation(
                "Reminder processing completed in {ElapsedMs}ms. Processed: {Processed}, Succeeded: {Succeeded}, Failed: {Failed}, Skipped: {Skipped}",
                stopwatch.ElapsedMilliseconds, remindersProcessed, remindersSucceeded, remindersFailed, remindersSkipped);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Fatal error during reminder processing run (elapsed: {ElapsedMs}ms)", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private static string DetermineReminderType(TimeSpan timeUntilDue)
    {
        return timeUntilDue.TotalHours switch
        {
            <= 1 => "1Hour",
            <= 4 => "4Hours", 
            <= 24 => "24Hours",
            _ => "24Hours" // Default fallback
        };
    }

    private async Task<bool> SimulateSendReminderAsync(
        Application.DTOs.ReminderTaskDto task, 
        string reminderType, 
        CancellationToken cancellationToken)
    {
        // Simulate network delay for sending reminder
        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);

        // Simulate 95% success rate
        var random = new Random();
        var success = random.NextDouble() < 0.95;

        _logger.LogDebug(
            "Simulated sending {ReminderType} reminder to {UserEmail} for task '{TaskTitle}' - {Result}",
            reminderType, task.OwnerEmail, task.Title, success ? "SUCCESS" : "FAILED");

        return success;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reminder processing service is stopping...");
        await base.StopAsync(cancellationToken);
    }
}