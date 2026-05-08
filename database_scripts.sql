-- ================================================================
-- SCRIPTS DE CRIAÇÃO DAS TABELAS
-- Banco: SQL Server
-- Execute estes scripts antes de rodar as migrations do EF Core,
-- ou use apenas como referência da estrutura esperada.
-- ================================================================

-- Tabela de Usuários
CREATE TABLE Usuarios (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    Nome        NVARCHAR(100) NOT NULL,
    NomeUsuario NVARCHAR(50)  NOT NULL UNIQUE,
    SenhaHash   NVARCHAR(255) NOT NULL  -- Hash BCrypt, nunca senha em texto plano
);

-- Tabela de Endereços (relacionada a Usuarios)
CREATE TABLE Enderecos (
    Id          INT           IDENTITY(1,1) PRIMARY KEY,
    Cep         NVARCHAR(9)   NOT NULL,
    Logradouro  NVARCHAR(150) NOT NULL,
    Complemento NVARCHAR(100) NULL,       -- Campo opcional
    Bairro      NVARCHAR(100) NOT NULL,
    Cidade      NVARCHAR(100) NOT NULL,
    Uf          NVARCHAR(2)   NOT NULL,
    Numero      NVARCHAR(20)  NOT NULL,
    UsuarioId   INT           NOT NULL,

    CONSTRAINT FK_Enderecos_Usuarios
        FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
);

-- Índice para acelerar consultas por usuário (muito comum no sistema)
CREATE INDEX IX_Enderecos_UsuarioId ON Enderecos(UsuarioId);
