-- TaskTracker Database Schema
-- PostgreSQL DDL for TaskTracker application

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(254) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL
);

-- Create indexes for users
CREATE UNIQUE INDEX ix_users_email ON users(email);

-- Tasks table
CREATE TABLE tasks (
    id UUID PRIMARY KEY,
    title VARCHAR(200) NOT NULL,
    description VARCHAR(2000),
    status VARCHAR(50) NOT NULL,
    priority VARCHAR(50) NOT NULL,
    due_date TIMESTAMPTZ,
    tags JSONB,
    owner_user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ
);

-- Create indexes for tasks
CREATE INDEX ix_tasks_owner_user_id ON tasks(owner_user_id);
CREATE INDEX ix_tasks_status ON tasks(status);
CREATE INDEX ix_tasks_due_date ON tasks(due_date);
CREATE INDEX ix_tasks_owner_status ON tasks(owner_user_id, status);

-- Attachments table
CREATE TABLE attachments (
    id UUID PRIMARY KEY,
    file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    file_size_bytes BIGINT NOT NULL,
    storage_path VARCHAR(500) NOT NULL,
    task_id UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    uploaded_by_user_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    created_at TIMESTAMPTZ NOT NULL
);

-- Create indexes for attachments
CREATE INDEX ix_attachments_task_id ON attachments(task_id);
CREATE INDEX ix_attachments_uploaded_by_user_id ON attachments(uploaded_by_user_id);

-- Audit Events table
CREATE TABLE audit_events (
    id UUID PRIMARY KEY,
    action VARCHAR(50) NOT NULL,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    entity_id UUID,
    entity_type VARCHAR(100),
    details VARCHAR(1000),
    created_at TIMESTAMPTZ NOT NULL
);

-- Create indexes for audit_events
CREATE INDEX ix_audit_events_user_id ON audit_events(user_id);
CREATE INDEX ix_audit_events_created_at ON audit_events(created_at);
CREATE INDEX ix_audit_events_entity ON audit_events(entity_type, entity_id);
CREATE INDEX ix_audit_events_action ON audit_events(action);

-- Reminder Logs table
CREATE TABLE reminder_logs (
    id UUID PRIMARY KEY,
    task_id UUID NOT NULL REFERENCES tasks(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    reminder_sent_at TIMESTAMPTZ NOT NULL,
    task_due_date TIMESTAMPTZ NOT NULL,
    reminder_type VARCHAR(50) NOT NULL,
    delivery_successful BOOLEAN NOT NULL,
    delivery_details VARCHAR(500),
    created_at TIMESTAMPTZ NOT NULL
);

-- Create unique constraint for reminder idempotency
CREATE UNIQUE INDEX ix_reminder_logs_unique_reminder ON reminder_logs(task_id, reminder_type, task_due_date);

-- Additional indexes for reminder_logs
CREATE INDEX ix_reminder_logs_task_id ON reminder_logs(task_id);
CREATE INDEX ix_reminder_logs_user_id ON reminder_logs(user_id);
CREATE INDEX ix_reminder_logs_reminder_sent_at ON reminder_logs(reminder_sent_at);

-- Insert sample data constraints and check constraints
ALTER TABLE tasks ADD CONSTRAINT chk_tasks_status 
    CHECK (status IN ('New', 'InProgress', 'Completed', 'Archived'));

ALTER TABLE tasks ADD CONSTRAINT chk_tasks_priority 
    CHECK (priority IN ('Low', 'Medium', 'High', 'Critical'));

ALTER TABLE audit_events ADD CONSTRAINT chk_audit_action
    CHECK (action IN ('TaskCreated', 'TaskUpdated', 'TaskDeleted', 'TaskCompleted', 
                     'AttachmentAdded', 'AttachmentRemoved', 'ReminderSent', 
                     'UserLogin', 'UserProfileUpdated'));

-- Comments for documentation
COMMENT ON TABLE users IS 'Application users who can create and manage tasks';
COMMENT ON TABLE tasks IS 'Task items with ownership, status, and metadata';
COMMENT ON TABLE attachments IS 'File attachments associated with tasks';
COMMENT ON TABLE audit_events IS 'Audit log for tracking user actions';
COMMENT ON TABLE reminder_logs IS 'Log of reminders sent for tasks, with idempotency';

COMMENT ON COLUMN tasks.tags IS 'JSON array of string tags for categorization';
COMMENT ON INDEX ix_reminder_logs_unique_reminder IS 'Prevents duplicate reminders for same task and reminder type';