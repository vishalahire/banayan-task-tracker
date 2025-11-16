import { useState, useEffect } from 'react';
import { reminderApi, PendingReminder, ReminderProcessingResult } from '../api/reminderApi';
import Alert from '../components/Alert';

export default function RemindersPage() {
  const [pendingReminders, setPendingReminders] = useState<PendingReminder[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [lastResult, setLastResult] = useState<ReminderProcessingResult | null>(null);

  const loadPendingReminders = async () => {
    try {
      setIsLoading(true);
      setError('');
      const reminders = await reminderApi.getPendingReminders();
      setPendingReminders(reminders);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load pending reminders');
    } finally {
      setIsLoading(false);
    }
  };

  const processPendingReminders = async () => {
    try {
      setIsProcessing(true);
      setError('');
      setSuccess('');
      
      const result = await reminderApi.processPendingReminders();
      setLastResult(result);
      
      if (result.processedCount > 0) {
        setSuccess(`Successfully processed ${result.processedCount} reminders (${result.successfulCount} sent, ${result.failedCount} failed)`);
      } else {
        setSuccess('No reminders were processed (all were already sent or skipped)');
      }
      
      // Reload the pending reminders list
      await loadPendingReminders();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to process reminders');
    } finally {
      setIsProcessing(false);
    }
  };

  useEffect(() => {
    loadPendingReminders();
  }, []);

  const formatTimeUntilDue = (hours: number) => {
    if (hours < 0) return 'Overdue';
    if (hours < 1) return `${Math.round(hours * 60)} minutes`;
    if (hours < 24) return `${Math.round(hours)} hours`;
    return `${Math.round(hours / 24)} days`;
  };

  const getReminderTypeColor = (type: string) => {
    switch (type) {
      case '1Hour': return 'bg-red-100 text-red-800';
      case '4Hours': return 'bg-yellow-100 text-yellow-800';
      case '24Hours': return 'bg-blue-100 text-blue-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusColor = (sent: boolean) => {
    return sent ? 'bg-green-100 text-green-800' : 'bg-orange-100 text-orange-800';
  };

  return (
    <div>
      {/* Header */}
      <div className="flex justify-between items-center mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Reminders</h1>
          <p className="text-sm text-gray-600 mt-1">
            Manage reminders for tasks due in the next 24 hours
          </p>
        </div>
        <button
          onClick={processPendingReminders}
          disabled={isProcessing || isLoading}
          className="bg-blue-600 hover:bg-blue-700 disabled:bg-blue-300 text-white px-4 py-2 rounded-md font-medium transition-colors"
        >
          {isProcessing ? 'Processing...' : 'Send All Reminders'}
        </button>
      </div>

      {/* Alerts */}
      {error && <Alert type="error" message={error} onClose={() => setError('')} />}
      {success && <Alert type="success" message={success} onClose={() => setSuccess('')} />}

      {/* Processing Result Summary */}
      {lastResult && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <h3 className="text-lg font-medium text-blue-900 mb-2">Last Processing Summary</h3>
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4 text-sm">
            <div>
              <div className="font-medium text-blue-700">Total Pending</div>
              <div className="text-blue-900">{lastResult.totalPending}</div>
            </div>
            <div>
              <div className="font-medium text-green-700">Successful</div>
              <div className="text-green-900">{lastResult.successfulCount}</div>
            </div>
            <div>
              <div className="font-medium text-red-700">Failed</div>
              <div className="text-red-900">{lastResult.failedCount}</div>
            </div>
            <div>
              <div className="font-medium text-yellow-700">Skipped</div>
              <div className="text-yellow-900">{lastResult.skippedCount}</div>
            </div>
            <div>
              <div className="font-medium text-blue-700">Processed</div>
              <div className="text-blue-900">{lastResult.processedCount}</div>
            </div>
          </div>
          {lastResult.errors.length > 0 && (
            <div className="mt-3">
              <div className="font-medium text-red-700 mb-1">Errors:</div>
              <ul className="text-sm text-red-600 list-disc list-inside">
                {lastResult.errors.map((error, index) => (
                  <li key={index}>{error}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {/* Loading */}
      {isLoading ? (
        <div className="text-center py-8">
          <div className="text-gray-500">Loading pending reminders...</div>
        </div>
      ) : (
        <>
          {/* Summary */}
          <div className="mb-6">
            <div className="bg-white rounded-lg shadow p-4">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-4 text-center">
                <div>
                  <div className="text-2xl font-bold text-gray-900">{pendingReminders.length}</div>
                  <div className="text-sm text-gray-600">Total Tasks Due</div>
                </div>
                <div>
                  <div className="text-2xl font-bold text-orange-600">
                    {pendingReminders.filter(r => !r.hasReminderBeenSent).length}
                  </div>
                  <div className="text-sm text-gray-600">Pending Reminders</div>
                </div>
                <div>
                  <div className="text-2xl font-bold text-green-600">
                    {pendingReminders.filter(r => r.hasReminderBeenSent).length}
                  </div>
                  <div className="text-sm text-gray-600">Already Sent</div>
                </div>
              </div>
            </div>
          </div>

          {/* Reminders List */}
          {pendingReminders.length === 0 ? (
            <div className="text-center py-8">
              <div className="text-gray-500 mb-4">No tasks due in the next 24 hours</div>
            </div>
          ) : (
            <div className="bg-white rounded-lg shadow overflow-hidden">
              <div className="overflow-x-auto">
                <table className="min-w-full divide-y divide-gray-200">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Task
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Owner
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Due Date
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Time Until Due
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Reminder Type
                      </th>
                      <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                        Status
                      </th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {pendingReminders.map((reminder) => (
                      <tr key={reminder.taskId} className="hover:bg-gray-50">
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm font-medium text-gray-900">
                            {reminder.taskTitle}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">{reminder.ownerDisplayName}</div>
                          <div className="text-sm text-gray-500">{reminder.ownerEmail}</div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">
                            {new Date(reminder.dueDate).toLocaleDateString()}
                          </div>
                          <div className="text-sm text-gray-500">
                            {new Date(reminder.dueDate).toLocaleTimeString()}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <div className="text-sm text-gray-900">
                            {formatTimeUntilDue(reminder.hoursUntilDue)}
                          </div>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getReminderTypeColor(reminder.reminderType)}`}>
                            {reminder.reminderType}
                          </span>
                        </td>
                        <td className="px-6 py-4 whitespace-nowrap">
                          <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(reminder.hasReminderBeenSent)}`}>
                            {reminder.hasReminderBeenSent ? 'Sent' : 'Pending'}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}