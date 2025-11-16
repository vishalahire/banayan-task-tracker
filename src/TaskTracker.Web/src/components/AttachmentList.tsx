import { TaskAttachment } from '../types';
import { attachmentApi } from '../api/attachmentApi';

interface AttachmentListProps {
  attachments: TaskAttachment[];
  onDelete: (attachmentId: string) => void;
}

export default function AttachmentList({ attachments, onDelete }: AttachmentListProps) {
  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleDownload = (attachment: TaskAttachment) => {
    const downloadUrl = attachmentApi.downloadAttachment(attachment.id);
    window.open(downloadUrl, '_blank');
  };

  const handleDelete = async (attachment: TaskAttachment) => {
    if (window.confirm(`Are you sure you want to delete ${attachment.fileName}?`)) {
      try {
        await attachmentApi.deleteAttachment(attachment.id);
        onDelete(attachment.id);
      } catch (err) {
        alert('Failed to delete attachment');
      }
    }
  };

  if (attachments.length === 0) {
    return (
      <div className="text-center py-6 text-gray-500">
        No attachments yet
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {attachments.map((attachment) => (
        <div
          key={attachment.id}
          className="flex items-center justify-between p-3 border border-gray-200 rounded-lg"
        >
          <div className="flex items-center space-x-3">
            <div className="flex-shrink-0">
              <svg
                className="h-8 w-8 text-gray-400"
                fill="currentColor"
                viewBox="0 0 20 20"
              >
                <path
                  fillRule="evenodd"
                  d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4zm2 6a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1zm1 3a1 1 0 100 2h6a1 1 0 100-2H7z"
                  clipRule="evenodd"
                />
              </svg>
            </div>
            <div>
              <div className="text-sm font-medium text-gray-900">
                {attachment.fileName}
              </div>
              <div className="text-xs text-gray-500">
                {formatFileSize(attachment.fileSize)} • {attachment.contentType}
              </div>
            </div>
          </div>

          <div className="flex items-center space-x-2">
            <button
              onClick={() => handleDownload(attachment)}
              className="text-blue-600 hover:text-blue-800 text-sm font-medium"
            >
              Download
            </button>
            <button
              onClick={() => handleDelete(attachment)}
              className="text-red-600 hover:text-red-800 text-sm font-medium"
            >
              Delete
            </button>
          </div>
        </div>
      ))}
    </div>
  );
}