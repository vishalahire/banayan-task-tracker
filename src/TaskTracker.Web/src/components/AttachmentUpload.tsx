import { useState, useRef } from 'react';
import { TaskAttachment } from '../types';
import { attachmentApi } from '../api/attachmentApi';
import Alert from './Alert';

interface AttachmentUploadProps {
  taskId: string;
  onUploadComplete: (attachment: TaskAttachment) => void;
}

export default function AttachmentUpload({ taskId, onUploadComplete }: AttachmentUploadProps) {
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState('');
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    try {
      setIsUploading(true);
      setError('');

      const attachment = await attachmentApi.uploadAttachment(taskId, file);
      onUploadComplete(attachment);

      // Clear the input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Upload failed');
    } finally {
      setIsUploading(false);
    }
  };

  return (
    <div className="border-2 border-dashed border-gray-300 rounded-lg p-6">
      {error && <Alert type="error" message={error} onClose={() => setError('')} />}
      
      <div className="text-center">
        <svg
          className="mx-auto h-12 w-12 text-gray-400"
          stroke="currentColor"
          fill="none"
          viewBox="0 0 48 48"
        >
          <path
            d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02"
            strokeWidth={2}
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
        <div className="mt-4">
          <label htmlFor="file-upload" className="cursor-pointer">
            <span className="text-blue-600 hover:text-blue-500 font-medium">
              Upload a file
            </span>
            <input
              id="file-upload"
              ref={fileInputRef}
              type="file"
              className="sr-only"
              onChange={handleFileSelect}
              disabled={isUploading}
            />
          </label>
          <p className="text-gray-500 text-sm mt-2">
            {isUploading ? 'Uploading...' : 'PNG, JPG, PDF up to 10MB'}
          </p>
        </div>
      </div>
    </div>
  );
}