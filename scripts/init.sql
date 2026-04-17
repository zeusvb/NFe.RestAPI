CREATE SCHEMA IF NOT EXISTS nfe;

CREATE TABLE IF NOT EXISTS nfe.companies (
    id SERIAL PRIMARY KEY,
    cnpj VARCHAR(14) NOT NULL UNIQUE,
    company_name VARCHAR(255) NOT NULL,
    fancy_name VARCHAR(255),
    certificate_thumbprint VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS nfe.users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) NOT NULL UNIQUE,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'User',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS nfe.nfe_documents (
    id SERIAL PRIMARY KEY,
    company_id INTEGER NOT NULL REFERENCES nfe.companies(id),
    nfe_number VARCHAR(50) NOT NULL,
    series VARCHAR(5) NOT NULL,
    protocol_number VARCHAR(50),
    access_key VARCHAR(44),
    status VARCHAR(50),
    xml_content TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(company_id, nfe_number, series)
);

CREATE TABLE IF NOT EXISTS nfe.nfe_events (
    id SERIAL PRIMARY KEY,
    nfe_id INTEGER NOT NULL REFERENCES nfe.nfe_documents(id),
    event_type VARCHAR(50),
    event_status VARCHAR(50),
    event_data TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
