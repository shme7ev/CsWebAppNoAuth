DROP TABLE IF EXISTS files;

CREATE TABLE files (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    file_name VARCHAR(255) NOT NULL,
    original_file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    size BIGINT NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    upload_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    uploaded_by VARCHAR(100) NOT NULL,
    description VARCHAR(1000),
    is_deleted BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMP,
    deleted_by VARCHAR(100)
);

CREATE INDEX idx_files_file_name ON files(file_name);
CREATE INDEX idx_files_upload_date ON files(upload_date);
CREATE INDEX idx_files_uploaded_by ON files(uploaded_by);
CREATE INDEX idx_files_is_deleted ON files(is_deleted);

-- INSERT INTO files (file_name, original_file_name, content_type, size, file_path, uploaded_by, description) VALUES
-- ('sample.txt', 'Sample Document.txt', 'text/plain', 1024, '/app/storage/sample.txt', 'admin', 'Sample file for testing');
